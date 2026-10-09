
using System.ComponentModel.DataAnnotations;

namespace RROMS.Models
{
    public class SystemSetting
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string RestaurantName { get; set; } = "RROMS Restaurant";

        [EmailAddress]
        [StringLength(150)]
        public string? ContactEmail { get; set; }

        [Phone]
        [StringLength(30)]
        public string? ContactPhone { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [Range(30, 240)]
        public int DefaultReservationDurationMinutes { get; set; } = 90;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}