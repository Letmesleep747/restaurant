
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RROMS.Data;
using RROMS.Models;

namespace RROMS.Pages.Admin.Announcements
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Announcement> Announcements { get; set; }
            = new List<Announcement>();

        public async Task OnGetAsync()
        {
            Announcements = await _context.Announcements
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostTogglePublishAsync(int id)
        {
            var announcement = await _context.Announcements
                .FirstOrDefaultAsync(a => a.Id == id);

            if (announcement == null)
                return NotFound();

            if (!announcement.IsActive && !announcement.IsPublished)
            {
                TempData["ErrorMessage"] =
                    "Activate the announcement before publishing it.";

                return RedirectToPage();
            }

            announcement.IsPublished = !announcement.IsPublished;
            announcement.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = announcement.IsPublished
                ? "Announcement published successfully."
                : "Announcement unpublished successfully.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(int id)
        {
            var announcement = await _context.Announcements
                .FirstOrDefaultAsync(a => a.Id == id);

            if (announcement == null)
                return NotFound();

            announcement.IsActive = !announcement.IsActive;

            if (!announcement.IsActive)
                announcement.IsPublished = false;

            announcement.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = announcement.IsActive
                ? "Announcement activated."
                : "Announcement deactivated.";

            return RedirectToPage();
        }
    }
}