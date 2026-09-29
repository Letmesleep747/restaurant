using System.ComponentModel.DataAnnotations;

namespace RROMS.ViewModels.Admin
{
    public class UserCreateViewModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;


        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }


        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; } = "RestaurantStaff";


        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;


        [Required(ErrorMessage = "Please confirm the password.")]
        [DataType(DataType.Password)]
        [Compare(
            "Password",
            ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
            = string.Empty;
    }
}