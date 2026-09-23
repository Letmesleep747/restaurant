using System.ComponentModel.DataAnnotations;

namespace RROMS.ViewModels.Admin
{
    public class DiningAreaFormViewModel
    {
        [Required(ErrorMessage = "Dining area name is required.")]
        [StringLength(
            100,
            ErrorMessage = "Dining area name cannot exceed 100 characters.")]
        [Display(Name = "Dining Area Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(
            300,
            ErrorMessage = "Description cannot exceed 300 characters.")]
        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}