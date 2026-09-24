using System.ComponentModel.DataAnnotations;

namespace RROMS.Models
{
    public class OperatingHour
    {
        [Key]
        public int OperatingHourId { get; set; }

        [Required]
        public DayOfWeek Day { get; set; }

        public TimeSpan? OpeningTime { get; set; }

        public TimeSpan? ClosingTime { get; set; }

        public bool IsClosed { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}