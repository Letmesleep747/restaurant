using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RROMS.Data;
using RROMS.Models;

namespace RROMS.Pages.Admin.Menu.Items
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context) { _context = context; }

        public IList<MenuItem> Items { get; set; } = new List<MenuItem>();

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task OnGetAsync()
        {
            Items = await _context.MenuItems
                .AsNoTracking()
                .Include(i => i.MenuCategory)
                .OrderBy(i => i.Name)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostToggleAvailabilityAsync(int id)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null) return NotFound();

            item.IsAvailable = !item.IsAvailable;
            await _context.SaveChangesAsync();

            StatusMessage = item.IsAvailable ? "Menu item is now available." : "Menu item is now unavailable.";
            return RedirectToPage();
        }
    }
}
