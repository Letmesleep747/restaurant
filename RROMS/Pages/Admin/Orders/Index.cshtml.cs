
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RROMS.Pages.Admin.Orders
{
    public class IndexModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public DateTime? OrderDate { get; set; }

        public int TotalOrders { get; private set; }
        public int ActiveOrders { get; private set; }
        public int CompletedOrders { get; private set; }
        public int CancelledOrders { get; private set; }

        public decimal TotalOrderValue { get; private set; }

        public void OnGet()
        {
            // Order and payment queries will be added after
            // the shared customer-side schema is integrated.

            TotalOrders = 0;
            ActiveOrders = 0;
            CompletedOrders = 0;
            CancelledOrders = 0;
            TotalOrderValue = 0m;
        }
    }
}