using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using restaurant.Models;

namespace restaurant.Pages.Dev
{
    [AllowAnonymous]
    public class SeedModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<SeedModel> _logger;

        public SeedModel(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ILogger<SeedModel> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async System.Threading.Tasks.Task OnGetAsync()
        {
            try
            {
                string[] roles = new[] { "Administrator", "Staff", "Customer" };
                foreach (var role in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                        _logger.LogInformation("Created role {role}", role);
                    }
                }

                var adminEmail = "admin@local";
                var admin = await _userManager.FindByEmailAsync(adminEmail);
                if (admin == null)
                {
                    admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = "Administrator" };
                    var result = await _userManager.CreateAsync(admin, "P@ssw0rd!23");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(admin, "Administrator");
                        _logger.LogInformation("Created admin user {email}", adminEmail);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to create admin user: {errors}", string.Join(';', result.Errors));
                    }
                }
                else
                {
                    if (!await _userManager.IsInRoleAsync(admin, "Administrator"))
                    {
                        await _userManager.AddToRoleAsync(admin, "Administrator");
                        _logger.LogInformation("Assigned Administrator role to existing user {email}", adminEmail);
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Seeding failed");
            }
        }
    }
}
