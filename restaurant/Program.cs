using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using restaurant.Data;
using restaurant.Hubs;
using restaurant.Models;
using restaurant.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Customer services
builder.Services.AddScoped<IReservationAvailabilityService, ReservationAvailabilityService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Identity (use AddIdentity so role support is explicit and no extra UI package is required)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// Force cookie auth to our Customer Account pages (NOT /Identity/Account/*)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
    options.SlidingExpiration = true;
});

// Authorization policies (example)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Administrator"));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // must be before UseAuthorization
app.UseAuthorization();

// Map Razor Pages
app.MapRazorPages();
app.MapHub<NotificationHub>("/hubs/notifications");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();
    try
    {
        await db.Database.MigrateAsync();
    }
    catch
    {
        try
        {
            await db.Database.EnsureCreatedAsync();
        }
        catch
        {
            // swallow - subsequent calls will surface errors with more context
        }
    }

    var canConnect = false;
    try
    {
        canConnect = await db.Database.CanConnectAsync();
    }
    catch
    {
        canConnect = false;
    }

    if (!canConnect)
    {
        Console.WriteLine("Warning: Cannot connect to the database. Skipping role/user seeding.");
    }
    else
    {
        try
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roles = new[] { "Administrator", "Staff", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var adminEmail = builder.Configuration["AdminUser:Email"] ?? "admin@local";
            var adminPassword = builder.Configuration["AdminUser:Password"] ?? "P@ssw0rd!23";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Administrator",
                    IsActive = true
                };
                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Administrator");
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(adminUser, "Administrator"))
                    await userManager.AddToRoleAsync(adminUser, "Administrator");
            }
        }
        catch (Exception seedEx)
        {
            Console.WriteLine($"Role/user seeding failed: {seedEx.Message}");
        }
    }
}

app.Run();
