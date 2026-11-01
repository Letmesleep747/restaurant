using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using restaurant.Data;
using restaurant.Models;

namespace restaurant.Components
{
    // Shows announcements that the admin has published, are active,
    // and fall inside their (optional) start/end dates.
    public class ActiveAnnouncementsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ActiveAnnouncementsViewComponent> _logger;

        public ActiveAnnouncementsViewComponent(
            ApplicationDbContext db,
            ILogger<ActiveAnnouncementsViewComponent> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(int take = 3)
        {
            var today = DateTime.Today;
            var items = new List<Announcement>();

            try
            {
                items = await _db.Announcements
                    .AsNoTracking()
                    .Where(a => a.IsActive
                             && a.IsPublished
                             && (a.StartDate == null || a.StartDate <= today)
                             && (a.EndDate == null || a.EndDate >= today))
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(take)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Don't break the customer home page if the Announcements
                // table doesn't exist yet in this database.
                _logger.LogWarning(ex, "Could not load announcements.");
            }

            return View(items);
        }
    }
}
