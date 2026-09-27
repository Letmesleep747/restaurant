using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RROMS.Models;
using RROMS.ViewModels.Account;

namespace RROMS.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser>
            _signInManager;

        private readonly UserManager<ApplicationUser>
            _userManager;


        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager =
                signInManager;

            _userManager =
                userManager;
        }


        [BindProperty]
        public LoginViewModel Input { get; set; }
            = new();


        public void OnGet()
        {
        }


        public async Task<IActionResult> OnPostAsync(
            string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }


            var email =
                Input.Email.Trim();


            var user =
                await _userManager
                    .FindByEmailAsync(email);


            if (user == null ||
                !user.IsActive)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password.");

                return Page();
            }


            var result =
                await _signInManager
                    .PasswordSignInAsync(
                        user,
                        Input.Password,
                        Input.RememberMe,
                        lockoutOnFailure: true);


            if (result.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(
                        returnUrl) &&
                    Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(
                        returnUrl);
                }


                if (await _userManager.IsInRoleAsync(
                        user,
                        "Administrator"))
                {
                    return RedirectToPage(
                        "/Admin/Index");
                }


                return RedirectToPage(
                    "/Account/AccessDenied");
            }


            if (result.IsLockedOut)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Account temporarily locked due to repeated failed login attempts.");

                return Page();
            }


            ModelState.AddModelError(
                string.Empty,
                "Invalid email or password.");


            return Page();
        }
    }
}