using System.ComponentModel.DataAnnotations;

namespace RROMS.ViewModels.Admin
{
    public class UserEditViewModel
    {
        public string Id { get; set; } = string.Empty;


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


        [Display(Name = "Account Active")]
        public bool IsActive { get; set; }
    }
}