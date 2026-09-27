using Microsoft.AspNetCore.Identity;
using RROMS.Models;

namespace RROMS.Data.Seed
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(
            IServiceProvider services,
            IConfiguration configuration)
        {
            using var scope = services.CreateScope();


            var roleManager =
                scope.ServiceProvider
                    .GetRequiredService<RoleManager<IdentityRole>>();


            var userManager =
                scope.ServiceProvider
                    .GetRequiredService<UserManager<ApplicationUser>>();


            string[] roles =
            {
                "Administrator",
                "RestaurantStaff",
                "Customer"
            };


            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var roleResult =
                        await roleManager.CreateAsync(
                            new IdentityRole(role));


                    if (!roleResult.Succeeded)
                    {
                        throw new Exception(
                            $"Unable to create role: {role}");
                    }
                }
            }


            var adminEmail =
                configuration["SeedAdmin:Email"];

            var adminPassword =
                configuration["SeedAdmin:Password"];


            if (string.IsNullOrWhiteSpace(adminEmail) ||
                string.IsNullOrWhiteSpace(adminPassword))
            {
                return;
            }


            var admin =
                await userManager.FindByEmailAsync(
                    adminEmail);


            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,

                    FullName =
                        "System Administrator",

                    EmailConfirmed = true,

                    IsActive = true,

                    CreatedAt =
                        DateTime.UtcNow
                };


                var createResult =
                    await userManager.CreateAsync(
                        admin,
                        adminPassword);


                if (!createResult.Succeeded)
                {
                    var errors =
                        string.Join(
                            ", ",
                            createResult.Errors
                                .Select(e => e.Description));


                    throw new Exception(
                        $"Unable to create Administrator: {errors}");
                }
            }


            if (!await userManager.IsInRoleAsync(
                    admin,
                    "Administrator"))
            {
                await userManager.AddToRoleAsync(
                    admin,
                    "Administrator");
            }
        }
    }
}