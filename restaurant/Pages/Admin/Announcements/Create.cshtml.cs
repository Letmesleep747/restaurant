using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using restaurant.Data;
using restaurant.Models;

namespace restaurant.Pages.Admin.Announcements
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Announcement Announcement { get; set; } = new();

        public void OnGet()
        {
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

            Announcement.CreatedAt = DateTime.UtcNow;
            Announcement.UpdatedAt = null;

            await _context.Announcements.AddAsync(Announcement);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Announcement created successfully.";
            return RedirectToPage("./Index");
        }
    }
}
