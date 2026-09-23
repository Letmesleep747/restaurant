using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using restaurant.Data;
using restaurant.Models;
using restaurant.Models.Enums;
using restaurant.Services;

namespace restaurant.Pages.Customer.Reservations
{
    [Authorize(Roles = "Customer")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notifications;

        public IndexModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
            INotificationService notifications)
        {
            _db = db;
            _userManager = userManager;
            _notifications = notifications;
        }

        public List<Reservation> Reservations { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            Reservations = await _db.Reservations
                .Include(r => r.Table)
                .Where(r => r.CustomerId == userId)
                .OrderByDescending(r => r.ReservationDate).ThenByDescending(r => r.StartTime)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var reservation = await _db.Reservations
                .FirstOrDefaultAsync(r => r.Id == id && r.CustomerId == userId);

            if (reservation != null &&
                (reservation.Status == ReservationStatus.Pending || reservation.Status == ReservationStatus.Confirmed))
            {
                reservation.Status = ReservationStatus.Cancelled;
                reservation.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                await _notifications.NotifyAsync(userId, "Reservation cancelled",
                    $"Your reservation on {reservation.ReservationDate:dd MMM yyyy} has been cancelled.",
                    NotificationType.Reservation);
            }

            return RedirectToPage();
        }
    }
}
