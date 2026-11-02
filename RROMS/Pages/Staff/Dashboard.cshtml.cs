using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RROMS.Data;
using RROMS.Models;

namespace RROMS.Pages.Staff
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TodaysReservationCount { get; set; }

        public List<Reservation> TodaysReservations { get; set; }
            = new List<Reservation>();

        public async Task OnGetAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // Count actual reservations for today.
            TodaysReservationCount = await _context.Reservations
                .AsNoTracking()
                .CountAsync(r =>
                    r.ReservationDate >= today &&
                    r.ReservationDate < tomorrow);

            // Load today's actual reservation details.
            TodaysReservations = await _context.Reservations
                .AsNoTracking()
                .Include(r => r.Customer)
                .Include(r => r.RestaurantTable)
                .Where(r =>
                    r.ReservationDate >= today &&
                    r.ReservationDate < tomorrow)
                .OrderBy(r => r.StartTime)
                .Take(10)
                .ToListAsync();
        }
    }
}