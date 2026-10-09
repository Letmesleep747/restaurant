
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RROMS.Pages.Admin.Reservations
{
    public class IndexModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public DateTime? ReservationDate { get; set; }

        public int TotalReservations { get; private set; }
        public int PendingReservations { get; private set; }
        public int ConfirmedReservations { get; private set; }
        public int CancelledReservations { get; private set; }

        public bool DataIntegrationPending { get; private set; } = true;

        public void OnGet()
        {
            // Customer reservation tables have not been integrated yet.
            // Connect the real database query after the shared schema
            // and customer-side models have been aligned.

            TotalReservations = 0;
            PendingReservations = 0;
            ConfirmedReservations = 0;
            CancelledReservations = 0;
        }

        public IActionResult OnGetDetails(int id)
        {
            // Detail lookup will be implemented when the real
            // Reservations table is integrated.
            return RedirectToPage("./Index");
        }
    }
}