
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RROMS.Data;
using RROMS.Models;

namespace RROMS.Pages.Admin.Menu.Categories
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<MenuCategory> Categories { get; set; }
            = new List<MenuCategory>();

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _context.MenuCategories
                .AsNoTracking()
                .Include(c => c.MenuItems)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            var category = await _context.MenuCategories
                .FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            category.IsActive = !category.IsActive;

            await _context.SaveChangesAsync();

            StatusMessage = category.IsActive
                ? "Category activated successfully."
                : "Category deactivated successfully.";

            return RedirectToPage();
        }
    }
}