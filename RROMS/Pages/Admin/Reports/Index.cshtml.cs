
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RROMS.Pages.Admin.Reports
{
    public class IndexModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public int TotalReservations { get; private set; }
        public int TotalOrders { get; private set; }
        public int CompletedOrders { get; private set; }

        public decimal TotalRevenue { get; private set; }

        public bool HasReportData { get; private set; }

        public string? ValidationMessage { get; private set; }

        public void OnGet()
        {
            if (StartDate.HasValue &&
                EndDate.HasValue &&
                StartDate.Value.Date > EndDate.Value.Date)
            {
                ValidationMessage =
                    "Start date must be on or before the end date.";
                return;
            }

            // Report data will be queried from the real transaction
            // tables after the customer-side schema is integrated.
            TotalReservations = 0;
            TotalOrders = 0;
            CompletedOrders = 0;
            TotalRevenue = 0m;
            HasReportData = false;
        }
    }
}