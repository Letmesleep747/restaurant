using System.ComponentModel.DataAnnotations;

namespace restaurant.Models.Shared
{
    public class OperatingHour
    {
        [Key]
        public int OperatingHourId { get; set; }

        public DayOfWeek Day { get; set; }

        public TimeSpan? OpeningTime { get; set; }

        public TimeSpan? ClosingTime { get; set; }

        public bool IsClosed { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
