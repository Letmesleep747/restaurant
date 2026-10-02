using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using restaurant.Data;
using restaurant.Models;
using restaurant.Models.Enums;

namespace restaurant.Pages.Customer
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public string FullName { get; set; } = string.Empty;
        public int UpcomingReservationCount { get; set; }
        public int ActiveOrderCount { get; set; }
        public int UnreadNotificationCount { get; set; }
        public List<Reservation> UpcomingReservations { get; set; } = new();
        public List<Order> RecentOrders { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                // Anonymous guest user: provide defaults and allow browsing
                FullName = "Guest";
                UpcomingReservationCount = 0;
                ActiveOrderCount = 0;
                UnreadNotificationCount = 0;
                UpcomingReservations = new();
                RecentOrders = new();
                return Page();
            }

            var user = await _userManager.FindByIdAsync(userId);
            FullName = user?.FullName ?? User.Identity?.Name ?? "Customer";

            var activeReservationStatuses = new[] { ReservationStatus.Pending, ReservationStatus.Confirmed };
            var activeOrderStatuses = new[] { OrderStatus.Placed, OrderStatus.Preparing, OrderStatus.ReadyToServe };

            UpcomingReservations = await _db.Reservations
                .Include(r => r.Table)
                .Where(r => r.CustomerId == userId
                            && activeReservationStatuses.Contains(r.Status)
                            && r.ReservationDate >= DateTime.Today)
                .OrderBy(r => r.ReservationDate).ThenBy(r => r.StartTime)
                .Take(5)
                .ToListAsync();

            RecentOrders = await _db.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.CustomerId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            UpcomingReservationCount = await _db.Reservations
                .CountAsync(r => r.CustomerId == userId
                                  && activeReservationStatuses.Contains(r.Status)
                                  && r.ReservationDate >= DateTime.Today);

            ActiveOrderCount = await _db.Orders
                .CountAsync(o => o.CustomerId == userId && activeOrderStatuses.Contains(o.Status));

            UnreadNotificationCount = await _db.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            return Page();
        }
    }
}
