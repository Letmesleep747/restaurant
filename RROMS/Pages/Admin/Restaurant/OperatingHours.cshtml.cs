using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RROMS.Pages.Admin.Restaurant
{
    public class OperatingHoursModel : PageModel
    {
        public List<OperatingHourItem> OperatingHours { get; set; }
            = new List<OperatingHourItem>();

        public void OnGet()
        {
            OperatingHours = new List<OperatingHourItem>
            {
                new OperatingHourItem
                {
                    Day = "Monday",
                    OpeningTime = "10:00 AM",
                    ClosingTime = "10:00 PM"
                },

                new OperatingHourItem
                {
                    Day = "Tuesday",
                    OpeningTime = "10:00 AM",
                    ClosingTime = "10:00 PM"
                },

                new OperatingHourItem
                {
                    Day = "Wednesday",
                    OpeningTime = "10:00 AM",
                    ClosingTime = "10:00 PM"
                },

                new OperatingHourItem
                {
                    Day = "Thursday",
                    OpeningTime = "10:00 AM",
                    ClosingTime = "10:00 PM"
                },

                new OperatingHourItem
                {
                    Day = "Friday",
                    OpeningTime = "10:00 AM",
                    ClosingTime = "11:00 PM"
                },

                new OperatingHourItem
                {
                    Day = "Saturday",
                    OpeningTime = "10:00 AM",
                    ClosingTime = "11:00 PM"
                },

                new OperatingHourItem
                {
                    Day = "Sunday",
                    IsClosed = true
                }
            };
        }


        public class OperatingHourItem
        {
            public string Day { get; set; } = string.Empty;

            public string OpeningTime { get; set; } = string.Empty;

            public string ClosingTime { get; set; } = string.Empty;

            public bool IsClosed { get; set; }
        }
    }
}