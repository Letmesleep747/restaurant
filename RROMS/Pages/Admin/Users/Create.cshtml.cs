using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RROMS.Models;
using RROMS.ViewModels.Admin;

namespace RROMS.Pages.Admin.Users
{
    public class CreateModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

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
        public UserCreateViewModel Input { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Normalise the email input.
            Input.Email = Input.Email?.Trim() ?? string.Empty;

            // Revalidate the model after normalising the email.
            ModelState.ClearValidationState(nameof(Input));
            TryValidateModel(Input, nameof(Input));

            // Validate the selected role.
            if (!AllowedRoles.Contains(Input.Role))
            {
                ModelState.AddModelError(
                    "Input.Role",
                    "Please select a valid user role.");
            }

            // Stop account creation if validation fails.
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Normalise the full name.
            Input.FullName = Input.FullName.Trim();

            // Check whether the email is already registered.
            var existingUser =
                await _userManager.FindByEmailAsync(Input.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "Input.Email",
                    "An account with this email already exists.");

                return Page();
            }

            // Create the new user.
            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                FullName = Input.FullName,

                PhoneNumber =
                    string.IsNullOrWhiteSpace(Input.PhoneNumber)
                        ? null
                        : Input.PhoneNumber.Trim(),

                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Create the account using ASP.NET Core Identity.
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

            // Assign the selected role.
            var roleResult =
                await _userManager.AddToRoleAsync(
                    user,
                    Input.Role);

            if (!roleResult.Succeeded)
            {
                // Remove the account if role assignment fails.
                await _userManager.DeleteAsync(user);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return Page();
            }

            // Display success message.
            TempData["SuccessMessage"] =
                $"{user.FullName} has been created successfully.";

            return RedirectToPage("./Index");
        }
    }
}
