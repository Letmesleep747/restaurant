using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RROMS.Models;
using RROMS.ViewModels.Admin;

namespace RROMS.Pages.Admin.Users
{
    public class CreateModel : PageModel
    {
        private readonly UserManager<ApplicationUser>
            _userManager;

        private static readonly string[] AllowedRoles =
        {
            "Administrator",
            "RestaurantStaff",
            "Customer"
        };


        public CreateModel(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }


        [BindProperty]
        public UserCreateViewModel Input { get; set; }
            = new();


        public void OnGet()
        {
        }


        public async Task<IActionResult> OnPostAsync()
        {
            Input.Email =
                Input.Email.Trim();


            if (!AllowedRoles.Contains(Input.Role))
            {
                ModelState.AddModelError(
                    "Input.Role",
                    "Invalid role selected.");
            }


            if (!ModelState.IsValid)
            {
                return Page();
            }


            var existingUser =
                await _userManager.FindByEmailAsync(
                    Input.Email);


            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "Input.Email",
                    "An account with this email already exists.");

                return Page();
            }


            var user = new ApplicationUser
            {
                UserName = Input.Email,

                Email = Input.Email,

                FullName = Input.FullName.Trim(),

                PhoneNumber =
                    string.IsNullOrWhiteSpace(
                        Input.PhoneNumber)
                        ? null
                        : Input.PhoneNumber.Trim(),

                EmailConfirmed = true,

                IsActive = true,

                CreatedAt = DateTime.UtcNow
            };


            var createResult =
                await _userManager.CreateAsync(
                    user,
                    Input.Password);


            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return Page();
            }


            var roleResult =
                await _userManager.AddToRoleAsync(
                    user,
                    Input.Role);


            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return Page();
            }


            TempData["SuccessMessage"] =
                $"{user.FullName} has been created successfully.";


            return RedirectToPage("./Index");
        }
    }
}