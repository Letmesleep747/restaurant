using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using restaurant.Data;
using restaurant.Models;

namespace restaurant.Pages.Admin.Announcements
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Announcement Announcement { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var item = await _context.Announcements
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (item == null)
                return NotFound();

            Announcement = item;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Announcement.StartDate.HasValue &&
                Announcement.EndDate.HasValue &&
                Announcement.EndDate.Value.Date < Announcement.StartDate.Value.Date)
            {
                ModelState.AddModelError("Announcement.EndDate",
                    "End date cannot be earlier than the start date.");
            }

            if (!ModelState.IsValid)
                return Page();

            var existing = await _context.Announcements
                .FirstOrDefaultAsync(a => a.Id == Announcement.Id);

            if (existing == null)
                return NotFound();

            existing.Title = Announcement.Title;
            existing.Message = Announcement.Message;
            existing.StartDate = Announcement.StartDate;
            existing.EndDate = Announcement.EndDate;
            existing.IsActive = Announcement.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            // An inactive announcement can never stay published.
            if (!existing.IsActive)
                existing.IsPublished = false;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Announcement updated successfully.";
            return RedirectToPage("./Index");
        }
    }
}
