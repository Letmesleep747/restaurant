using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using restaurant.Data;
using restaurant.Models;
using restaurant.Models.Enums;
using restaurant.Services;

namespace restaurant.Pages.Customer.Orders
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notifications;

        public DetailsModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
            INotificationService notifications)
        {
            _db = db;
            _userManager = userManager;
            _notifications = notifications;
        }

        public Order? Order { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            Order = await _db.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem)
                .Include(o => o.Payments)
                .Include(o => o.Reservation)
                .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == userId);

            if (Order == null) return NotFound();
            return Page();
        }

        // Polled by the browser every few seconds so the status badge on
        // this page updates live without a full reload — this is what
        // gives the customer "real-time" order tracking.
        public async Task<IActionResult> OnGetStatusAsync(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var order = await _db.Orders.Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == userId);
            if (order == null) return NotFound();

            return new JsonResult(new
            {
                status = order.Status.ToString(),
                paymentStatus = order.Payment?.Status.ToString() ?? "Unpaid"
            });
        }

        // Simulates taking payment (no real payment gateway in scope).
        public async Task<IActionResult> OnPostPayAsync(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var order = await _db.Orders.Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == userId);
            if (order == null) return NotFound();

            var payment = order.Payments.OrderByDescending(p => p.TransactionDate).FirstOrDefault();

            if (payment == null)
            {
                var newPayment = new Payment
                {
                    OrderId = order.Id,
                    Amount = order.TotalAmount,
                    Method = PaymentMethod.Online,
                    Status = PaymentStatus.Paid,
                    TransactionDate = DateTime.UtcNow,
                    TransactionReference = $"TXN-{Guid.NewGuid().ToString()[..8].ToUpper()}"
                };
                order.Payments.Add(newPayment);
                _db.Payments.Add(newPayment);
            }
            else
            {
                payment.Status = PaymentStatus.Paid;
                payment.TransactionDate = DateTime.UtcNow;
            }

            order.Status = OrderStatus.Completed;

            await _db.SaveChangesAsync();
            await _notifications.NotifyAsync(userId, "Payment confirmed",
                $"Payment of {order.TotalAmount:C} for order #{order.Id} was successful.",
                NotificationType.Payment, $"/Customer/Orders/Details/{order.Id}");

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == userId);
            if (order == null) return NotFound();

            // Only allow cancelling before the kitchen starts preparing it.
            if (order.Status == OrderStatus.Placed)
            {
                order.Status = OrderStatus.Cancelled;
                await _db.SaveChangesAsync();
                await _notifications.NotifyAsync(userId, "Order cancelled",
                    $"Order #{order.Id} has been cancelled.", NotificationType.Order,
                    $"/Customer/Orders/Details/{order.Id}");
            }

            return RedirectToPage(new { id });
        }
    }
}
