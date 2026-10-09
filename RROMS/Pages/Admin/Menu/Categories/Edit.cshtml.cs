
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RROMS.Data;
using RROMS.Models;

namespace RROMS.Pages.Admin.Menu.Categories
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MenuCategory Category { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var category = await _context.MenuCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            Category = category;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (id != Category.Id)
            {
                return BadRequest();
            }

            Category.Name = Category.Name?.Trim() ?? string.Empty;
            Category.Description = Category.Description?.Trim();

            if (await _context.MenuCategories.AnyAsync(c =>
                c.Name == Category.Name && c.Id != id))
            {
                ModelState.AddModelError(
                    "Category.Name",
                    "A category with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existing = await _context.MenuCategories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existing == null)
            {
                return NotFound();
            }

            existing.Name = Category.Name;
            existing.Description = Category.Description;
            existing.IsActive = Category.IsActive;

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] =
                "Menu category updated successfully.";

            return RedirectToPage("./Index");
        }
    }
}