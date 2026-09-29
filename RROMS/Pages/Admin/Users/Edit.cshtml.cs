using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RROMS.Models;
using RROMS.ViewModels.Admin;

namespace RROMS.Pages.Admin.Users
{
    public class EditModel : PageModel
    {
        private readonly UserManager<ApplicationUser>
            _userManager;


        private static readonly string[] AllowedRoles =
        {
            "Administrator",
            "RestaurantStaff",
            "Customer"
        };


        public EditModel(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }


        [BindProperty]
        public UserEditViewModel Input { get; set; }
            = new();


        public bool IsEditingCurrentUser { get; set; }


        public async Task<IActionResult> OnGetAsync(
            string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }


            var user =
                await _userManager.FindByIdAsync(id);


            if (user == null)
            {
                return NotFound();
            }


            var roles =
                await _userManager.GetRolesAsync(user);


            Input = new UserEditViewModel
            {
                Id = user.Id,

                FullName = user.FullName,

                Email = user.Email ?? string.Empty,

                PhoneNumber = user.PhoneNumber,

                Role = roles.FirstOrDefault()
                    ?? "Customer",

                IsActive = user.IsActive
            };


            IsEditingCurrentUser =
                _userManager.GetUserId(User)
                == user.Id;


            return Page();
        }


        public async Task<IActionResult> OnPostAsync(
            string id)
        {
            var user =
                await _userManager.FindByIdAsync(id);


            if (user == null)
            {
                return NotFound();
            }


            Input.Id = id;

            Input.Email =
                Input.Email.Trim();


            var currentUserId =
                _userManager.GetUserId(User);


            bool isCurrentUser =
                currentUserId == user.Id;


            IsEditingCurrentUser =
                isCurrentUser;


            // Never allow an administrator
            // to remove/deactivate their own account.
            if (isCurrentUser)
            {
                Input.Role = "Administrator";
                Input.IsActive = true;
            }


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


            // Check duplicate email
            var existingUser =
                await _userManager.FindByEmailAsync(
                    Input.Email);


            if (existingUser != null &&
                existingUser.Id != user.Id)
            {
                ModelState.AddModelError(
                    "Input.Email",
                    "Another account already uses this email.");

                return Page();
            }


            // Update basic user information
            user.FullName =
                Input.FullName.Trim();

            user.Email =
                Input.Email;

            user.UserName =
                Input.Email;

            user.PhoneNumber =
                string.IsNullOrWhiteSpace(
                    Input.PhoneNumber)
                    ? null
                    : Input.PhoneNumber.Trim();

            user.IsActive =
                Input.IsActive;


            var updateResult =
                await _userManager.UpdateAsync(user);


            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return Page();
            }


            // Don't allow current Admin
            // to change their own role.
            if (!isCurrentUser)
            {
                var currentRoles =
                    await _userManager
                        .GetRolesAsync(user);


                if (currentRoles.Count > 0)
                {
                    var removeResult =
                        await _userManager
                            .RemoveFromRolesAsync(
                                user,
                                currentRoles);


                    if (!removeResult.Succeeded)
                    {
                        foreach (var error
                            in removeResult.Errors)
                        {
                            ModelState.AddModelError(
                                string.Empty,
                                error.Description);
                        }

                        return Page();
                    }
                }


                var roleResult =
                    await _userManager.AddToRoleAsync(
                        user,
                        Input.Role);


                if (!roleResult.Succeeded)
                {
                    foreach (var error
                        in roleResult.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description);
                    }

                    return Page();
                }
            }


            TempData["SuccessMessage"] =
                $"{user.FullName} has been updated successfully.";


            return RedirectToPage("./Index");
        }
    }
}