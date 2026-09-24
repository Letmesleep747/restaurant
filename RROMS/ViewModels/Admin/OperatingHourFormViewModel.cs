using System.ComponentModel.DataAnnotations;

namespace RROMS.ViewModels.Admin
{
    public class OperatingHourFormViewModel
    {
        public int OperatingHourId { get; set; }

        public DayOfWeek Day { get; set; }

        [Display(Name = "Opening Time")]
        [DataType(DataType.Time)]
        public TimeSpan? OpeningTime { get; set; }

        [Display(Name = "Closing Time")]
        [DataType(DataType.Time)]
        public TimeSpan? ClosingTime { get; set; }

        [Display(Name = "Closed")]
        public bool IsClosed { get; set; }
    }
}