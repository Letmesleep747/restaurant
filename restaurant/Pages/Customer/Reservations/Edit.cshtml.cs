using System.ComponentModel.DataAnnotations;
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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IReservationAvailabilityService _availability;
        private readonly INotificationService _notifications;

        public EditModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
            IReservationAvailabilityService availability, INotificationService notifications)
        {
            _db = db;
            _userManager = userManager;
            _availability = availability;
            _notifications = notifications;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty]
        public int Id { get; set; }

        public class InputModel
        {
            [Required, DataType(DataType.Date)]
            [Display(Name = "Reservation Date")]
            public DateTime ReservationDate { get; set; }

            [Required, DataType(DataType.Time)]
            [Display(Name = "Arrival Time")]
            public TimeSpan StartTime { get; set; }

            [Required, Range(1, 20)]
            [Display(Name = "Party Size")]
            public int PartySize { get; set; }

            [StringLength(500)]
            [Display(Name = "Special Requests")]
            public string? SpecialRequests { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var reservation = await _db.Reservations
                .FirstOrDefaultAsync(r => r.Id == id && r.CustomerId == userId);
            if (reservation == null) return NotFound();

            if (reservation.Status != ReservationStatus.Pending && reservation.Status != ReservationStatus.Confirmed)
            {
                TempData["StatusMessage"] = "This reservation can no longer be modified.";
                return RedirectToPage("/Customer/Reservations/Index");
            }

            Id = reservation.Id;
            Input = new InputModel
            {
                ReservationDate = reservation.ReservationDate,
                StartTime = reservation.StartTime,
                PartySize = reservation.PartySize,
                SpecialRequests = reservation.SpecialRequests
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var reservation = await _db.Reservations
                .FirstOrDefaultAsync(r => r.Id == Id && r.CustomerId == userId);
            if (reservation == null) return NotFound();

            if (Input.ReservationDate.Date < DateTime.Today)
                ModelState.AddModelError("Input.ReservationDate", "Reservation date cannot be in the past.");

            if (!ModelState.IsValid)
                return Page();

            var table = await _availability.FindAvailableTableAsync(
                Input.ReservationDate, Input.StartTime, reservation.DurationMinutes,
                Input.PartySize, excludeReservationId: reservation.Id);

            if (table == null)
            {
                ModelState.AddModelError(string.Empty,
                    "No tables are available for that date, time, and party size. Please try a different slot.");
                return Page();
            }

            reservation.ReservationDate = Input.ReservationDate.Date;
            reservation.StartTime = Input.StartTime;
            reservation.PartySize = Input.PartySize;
            reservation.SpecialRequests = Input.SpecialRequests;
            reservation.TableId = table.Id;
            reservation.Status = ReservationStatus.Confirmed;
            reservation.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            await _notifications.NotifyAsync(userId, "Reservation updated",
                $"Your reservation is now confirmed for {reservation.ReservationDate:dd MMM yyyy} at {reservation.StartTime:hh\\:mm} (Table {table.TableNumber}).",
                NotificationType.Reservation, "/Customer/Reservations/Index");

            TempData["StatusMessage"] = "Reservation updated.";
            return RedirectToPage("/Customer/Reservations/Index");
        }
    }
}
