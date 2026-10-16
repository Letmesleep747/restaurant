using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using restaurant.Models;

namespace restaurant.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required, DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public void OnGet(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
                ModelState.AddModelError(string.Empty, ErrorMessage);

            // Customers land on the menu first
            ReturnUrl = returnUrl ?? "/Customer/Menu";
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= "/Customer/Menu";

            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email)
                       ?? await _userManager.FindByNameAsync(Input.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!, Input.Password, Input.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // Repair accounts that registered without a Customer role
                var roles = await _userManager.GetRolesAsync(user);
                var isStaffOrAdmin = roles.Contains("Administrator") || roles.Contains("Staff") || roles.Contains("RestaurantStaff");
                if (!isStaffOrAdmin && !roles.Contains("Customer"))
                {
                    await _userManager.AddToRoleAsync(user, "Customer");
                }

                // Always refresh the sign-in cookie so role claims are up to date
                await _signInManager.RefreshSignInAsync(user);

                // Never send customers to Access Denied / Admin / Identity return URLs
                if (string.IsNullOrEmpty(returnUrl)
                    || !Url.IsLocalUrl(returnUrl)
                    || returnUrl.Contains("AccessDenied", StringComparison.OrdinalIgnoreCase)
                    || returnUrl.Contains("/Admin", StringComparison.OrdinalIgnoreCase)
                    || returnUrl.Contains("/Staff", StringComparison.OrdinalIgnoreCase)
                    || returnUrl.Contains("/Identity/", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToPage("/Customer/Menu");
                }

                return LocalRedirect(returnUrl);
            }

            if (result.IsLockedOut)
                ModelState.AddModelError(string.Empty, "This account is locked out due to multiple failed attempts. Try again later.");
            else
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");

            return Page();
        }
    }
}
