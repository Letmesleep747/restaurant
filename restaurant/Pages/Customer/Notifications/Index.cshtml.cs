using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using restaurant.Data;
using restaurant.Models;

namespace restaurant.Pages.Customer.Notifications
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public List<Notification> Notifications { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            Notifications = await _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();

            var unread = Notifications.Where(n => !n.IsRead).ToList();
            if (unread.Count > 0)
            {
                foreach (var n in unread) n.IsRead = true;
                await _db.SaveChangesAsync();
            }

            return Page();
        }
    }
}
