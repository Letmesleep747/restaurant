using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using restaurant.Data;
using restaurant.Models;
using restaurant.Models.Enums;
using restaurant.Services;

namespace restaurant.Pages.Customer.Reservations
{
    [Authorize(Roles = "Customer")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IReservationAvailabilityService _availability;
        private readonly INotificationService _notifications;

        public CreateModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
            IReservationAvailabilityService availability, INotificationService notifications)
        {
            _db = db;
            _userManager = userManager;
            _availability = availability;
            _notifications = notifications;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required, DataType(DataType.Date)]
            [Display(Name = "Reservation Date")]
            public DateTime ReservationDate { get; set; } = DateTime.Today.AddDays(1);

            [Required, DataType(DataType.Time)]
            [Display(Name = "Arrival Time")]
            public TimeSpan StartTime { get; set; } = new TimeSpan(19, 0, 0);

            [Required, Range(1, 20)]
            [Display(Name = "Party Size")]
            public int PartySize { get; set; } = 2;

            [StringLength(500)]
            [Display(Name = "Special Requests")]
            public string? SpecialRequests { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            if (Input.ReservationDate.Date < DateTime.Today)
            {
                ModelState.AddModelError("Input.ReservationDate", "Reservation date cannot be in the past.");
            }

            if (!ModelState.IsValid)
                return Page();

            const int durationMinutes = 90;

            var table = await _availability.FindAvailableTableAsync(
                Input.ReservationDate, Input.StartTime, durationMinutes, Input.PartySize);

            if (table == null)
            {
                ModelState.AddModelError(string.Empty,
                    "No tables are available for that date, time, and party size. Please try a different slot.");
                return Page();
            }

            var reservation = new Reservation
            {
                CustomerId = userId,
                TableId = table.Id,
                ReservationDate = Input.ReservationDate.Date,
                StartTime = Input.StartTime,
                DurationMinutes = durationMinutes,
                PartySize = Input.PartySize,
                SpecialRequests = Input.SpecialRequests,
                Status = ReservationStatus.Confirmed,
                CreatedAt = DateTime.UtcNow
            };

            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();

            await _notifications.NotifyAsync(userId, "Reservation confirmed",
                $"Table {table.TableNumber} booked for {reservation.ReservationDate:dd MMM yyyy} at {reservation.StartTime:hh\\:mm}.",
                NotificationType.Reservation, "/Customer/Reservations/Index");

            TempData["StatusMessage"] = $"Reservation confirmed — Table {table.TableNumber}.";
            return RedirectToPage("/Customer/Reservations/Index");
        }
    }
}
