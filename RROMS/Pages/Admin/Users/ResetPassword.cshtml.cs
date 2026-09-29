using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RROMS.Models;
using RROMS.ViewModels.Admin;

namespace RROMS.Pages.Admin.Users
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResetPasswordModel(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public UserResetPasswordViewModel Input { get; set; }
            = new();

        public bool IsCurrentUser { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);

            IsCurrentUser = user.Id == currentUserId;

            Input.UserId = user.Id;
            Input.FullName = user.FullName;
            Input.Email = user.Email ?? string.Empty;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.FindByIdAsync(Input.UserId);

            if (user == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);

            if (user.Id == currentUserId)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "You cannot reset your own password from this page. Use the change password function instead.");

                Input.FullName = user.FullName;
                Input.Email = user.Email ?? string.Empty;

                return Page();
            }

            if (!ModelState.IsValid)
            {
                Input.FullName = user.FullName;
                Input.Email = user.Email ?? string.Empty;

                return Page();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(
                user,
                token,
                Input.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                Input.FullName = user.FullName;
                Input.Email = user.Email ?? string.Empty;

                return Page();
            }

            TempData["SuccessMessage"] =
                $"Password for {user.FullName} has been reset successfully.";

            return RedirectToPage("./Index");
        }
    }
}