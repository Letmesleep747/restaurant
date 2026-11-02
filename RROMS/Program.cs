
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RROMS.Data;
using RROMS.Models;

var builder = WebApplication.CreateBuilder(args);

// Razor Pages: protect the staff area with an authorization policy.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder(
        "/Staff",
        "StaffAccess");

    options.Conventions.AllowAnonymousToPage(
        "/Account/Login");
});

// Existing Azure SQL database connection.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ASP.NET Core Identity.
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;

        // Password requirements for newly created accounts.
        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;

        // Don't require email confirmation for this staff login flow.
        options.SignIn.RequireConfirmedAccount = false;

        // Lock accounts temporarily after repeated failed attempts.
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Authentication cookie settings.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.Name = "RROMS.StaffAuth";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Only authorized staff and administrators can access staff pages.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StaffAccess", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(
            "RestaurantStaff",
            "Staff",
            "Administrator");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// Authentication must run before authorization.
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
