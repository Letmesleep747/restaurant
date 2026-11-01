using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RROMS.Models;
using RROMS.ViewModels.Admin;

namespace RROMS.Pages.Admin.Users
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public string RoleFilter { get; set; } = "All";

        public List<UserListItemViewModel> Users { get; set; } = new();

        public async Task OnGetAsync()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                // Apply the selected role filter.
                if (!string.IsNullOrWhiteSpace(RoleFilter)
                    && RoleFilter != "All"
                    && !roles.Contains(RoleFilter))
                {
                    continue;
                }

                Users.Add(new UserListItemViewModel
                {
                    Id = user.Id,

                    FullName = user.FullName,

                    Email = user.Email ?? "-",

                    Role = roles.Count > 0
                        ? string.Join(", ", roles)
                        : "No Role",

                    IsActive = user.IsActive,

                    CreatedAt = user.CreatedAt
                });
            }
        }
    }
}