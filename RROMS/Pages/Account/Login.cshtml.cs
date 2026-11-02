
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RROMS.Models;

namespace RROMS.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [BindProperty]
        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Please enter your password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public bool RememberMe { get; set; }

        public IActionResult OnGet(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Email);

            // Reject missing or inactive accounts without signing them in.
            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password.");
                return Page();
            }

            // Only restaurant staff, staff, or administrators may use this
            // staff login page.
            var isAuthorizedStaff =
                await _userManager.IsInRoleAsync(user, "RestaurantStaff")
                || await _userManager.IsInRoleAsync(user, "Staff")
                || await _userManager.IsInRoleAsync(user, "Administrator");

            if (!isAuthorizedStaff)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password.");
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(
                user,
                Password,
                RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(returnUrl)
                    && Url.IsLocalUrl(returnUrl)
                    && returnUrl.StartsWith(
                        "/Staff",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return LocalRedirect(returnUrl);
                }

                return RedirectToPage("/Staff/Dashboard");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "This account is temporarily locked. Please try again later.");
            }
            else
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password.");
            }

            return Page();
        }
    }
}
