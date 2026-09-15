using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using restaurant.Data;
using restaurant.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity (use AddIdentity so role support is explicit and no extra UI package is required)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    // configure other options as needed
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

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
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // must be before UseAuthorization
app.UseAuthorization();

// Map Razor Pages
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    // Ensure database is created/migrated before seeding roles/users
    var db = services.GetRequiredService<ApplicationDbContext>();
    try
    {
        // Apply any pending migrations (preferred)
        await db.Database.MigrateAsync();
    }
    catch (Exception migrateEx)
    {
        // If migrations cannot be applied (e.g., no migrations or build issues while debugging),
        // fall back to EnsureCreated in development so Identity tables exist for seeding.
        try
        {
            await db.Database.EnsureCreatedAsync();
        }
        catch
        {
            // swallow - we'll let subsequent calls surface an error with more context
        }
    }
    // Seed roles
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = new[] { "Administrator", "Staff", "Customer" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Create a default admin user for development/testing and assign the Administrator role
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var adminEmail = builder.Configuration["AdminUser:Email"] ?? "admin@local";
    var adminPassword = builder.Configuration["AdminUser:Password"] ?? "P@ssw0rd!23";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = "Administrator" };
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

app.Run();