using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RROMS.Data;
using RROMS.Models;

namespace RROMS.Pages.Admin.Reservations
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

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

        public IList<Reservation> ReservationRecords { get; private set; }
            = new List<Reservation>();

        public async Task OnGetAsync()
        {
            // Statistics are calculated across all reservations,
            // regardless of the search filters.
            var allReservations = _context.Reservations
                .AsNoTracking();

            TotalReservations =
                await allReservations.CountAsync();

            PendingReservations =
                await allReservations.CountAsync(
                    r => r.Status == "Pending");

            ConfirmedReservations =
                await allReservations.CountAsync(
                    r => r.Status == "Confirmed");

            CancelledReservations =
                await allReservations.CountAsync(
                    r => r.Status == "Cancelled");

            // Load reservation records and related customer/table data.
            var query = _context.Reservations
                .AsNoTracking()
                .Include(r => r.Customer)
                .Include(r => r.Table)
                    .ThenInclude(t => t!.DiningArea)
                .AsQueryable();

            // Search by reservation ID, customer name,
            // customer email, or customer ID.
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var term = SearchTerm.Trim();
                var hasReservationId = int.TryParse(
                    term, out var reservationId);

                query = query.Where(r =>
                    (hasReservationId && r.Id == reservationId)
                    || r.CustomerId.Contains(term)
                    || (r.Customer != null
                        && (
                            r.Customer.FullName.Contains(term)
                            || (r.Customer.Email != null
                                && r.Customer.Email.Contains(term))
                        )));
            }

            // Filter by the selected reservation date.
            if (ReservationDate.HasValue)
            {
                var startDate = ReservationDate.Value.Date;
                var endDate = startDate.AddDays(1);

                query = query.Where(r =>
                    r.ReservationDate >= startDate
                    && r.ReservationDate < endDate);
            }

            // Status values must match the strings stored in SQL.
            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                query = query.Where(
                    r => r.Status == StatusFilter);
            }

            ReservationRecords = await query
                .OrderByDescending(r => r.ReservationDate)
                .ThenBy(r => r.StartTime)
                .ToListAsync();
        }
    }
}