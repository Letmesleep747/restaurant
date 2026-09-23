using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using restaurant.Data;
using restaurant.Models;

namespace restaurant.Pages.Customer.Feedback
{
    [Authorize(Roles = "Customer")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public List<Order> CompletedOrders { get; set; } = new();

        public class InputModel
        {
            public int? OrderId { get; set; }

            [Required, Range(1, 5)]
            public int Rating { get; set; } = 5;

            [StringLength(1000)]
            public string? Comments { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int? orderId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            CompletedOrders = await _db.Orders
                .Where(o => o.CustomerId == userId && o.Status == restaurant.Models.Enums.OrderStatus.Completed)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            Input.OrderId = orderId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            if (!ModelState.IsValid)
            {
                await OnGetAsync(Input.OrderId);
                return Page();
            }

            var feedback = new restaurant.Models.Feedback
            {
                CustomerId = userId,
                OrderId = Input.OrderId,
                Rating = Input.Rating,
                Comments = Input.Comments,
                CreatedAt = DateTime.UtcNow
            };

            _db.Feedbacks.Add(feedback);
            await _db.SaveChangesAsync();

            TempData["StatusMessage"] = "Thank you for your feedback!";
            return RedirectToPage("/Customer/Index");
        }
    }
}
