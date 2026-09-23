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
    [Authorize(Roles = "Customer")]
    public class CheckoutModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notifications;

        public CheckoutModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
            INotificationService notifications)
        {
            _db = db;
            _userManager = userManager;
            _notifications = notifications;
        }

        [BindProperty]
        public List<CartItemInput> CartItems { get; set; } = new();

        [BindProperty]
        public int? ReservationId { get; set; }

        [BindProperty]
        public string? Notes { get; set; }

        public List<Reservation> UpcomingReservations { get; set; } = new();

        public class CartItemInput
        {
            public int MenuItemId { get; set; }
            public int Quantity { get; set; }
        }

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            UpcomingReservations = await _db.Reservations
                .Where(r => r.CustomerId == userId
                            && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed)
                            && r.ReservationDate >= DateTime.Today)
                .OrderBy(r => r.ReservationDate)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            if (CartItems == null || CartItems.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Your cart is empty.");
                await OnGetAsync();
                return Page();
            }

            // Re-validate every item against the database — never trust
            // prices/availability posted from the browser.
            var menuItemIds = CartItems.Select(c => c.MenuItemId).ToList();
            var menuItems = await _db.MenuItems
                .Where(m => menuItemIds.Contains(m.Id) && m.IsAvailable)
                .ToListAsync();

            var order = new Order
            {
                CustomerId = userId,
                ReservationId = ReservationId,
                Notes = Notes,
                Status = OrderStatus.Placed,
                OrderDate = DateTime.UtcNow
            };

            decimal total = 0;
            foreach (var cartLine in CartItems)
            {
                var menuItem = menuItems.FirstOrDefault(m => m.Id == cartLine.MenuItemId);
                if (menuItem == null || cartLine.Quantity < 1) continue;

                var lineTotal = menuItem.Price * cartLine.Quantity;
                total += lineTotal;

                order.OrderItems.Add(new OrderItem
                {
                    MenuItemId = menuItem.Id,
                    Quantity = cartLine.Quantity,
                    UnitPrice = menuItem.Price
                });
            }

            if (order.OrderItems.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "None of the items in your cart are currently available.");
                await OnGetAsync();
                return Page();
            }

            order.TotalAmount = total;

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            await _notifications.NotifyAsync(userId, "Order placed",
                $"Your order #{order.Id} for {order.TotalAmount:C} has been placed.",
                NotificationType.Order, $"/Customer/Orders/Details/{order.Id}");

            TempData["ClearCart"] = true;
            return RedirectToPage("/Customer/Orders/Details", new { id = order.Id });
        }
    }
}
