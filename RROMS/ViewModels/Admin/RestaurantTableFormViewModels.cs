using System.ComponentModel.DataAnnotations;

namespace RROMS.ViewModels.Admin
{
    public class RestaurantTableFormViewModel
    {
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Please select a dining area.")]
        [Display(Name = "Dining Area")]
        public int DiningAreaId { get; set; }


        [Required(ErrorMessage = "Table number is required.")]
        [StringLength(
            20,
            ErrorMessage = "Table number cannot exceed 20 characters.")]
        [Display(Name = "Table Number")]
        public string TableNumber { get; set; } = string.Empty;


        [Required(ErrorMessage = "Table capacity is required.")]
        [Range(
            1,
            100,
            ErrorMessage = "Capacity must be between 1 and 100.")]
        public int Capacity { get; set; }


        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}