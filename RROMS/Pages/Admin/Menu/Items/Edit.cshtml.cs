
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RROMS.Data;
using RROMS.Models;

namespace RROMS.Pages.Admin.Menu.Items
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MenuItem Item { get; set; } = new();

        public SelectList Categories { get; set; } = default!;

        private async Task LoadCategoriesAsync()
        {
            Categories = new SelectList(
                await _context.MenuCategories
                    .AsNoTracking()
                    .Where(c => c.IsActive ||
                                c.Id == Item.MenuCategoryId)
                    .OrderBy(c => c.Name)
                    .ToListAsync(),
                "Id",
                "Name",
                Item.MenuCategoryId);
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var item = await _context.MenuItems
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            Item = item;

            await LoadCategoriesAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (id != Item.Id)
            {
                return BadRequest();
            }

            Item.Name = Item.Name?.Trim() ?? string.Empty;
            Item.Description = Item.Description?.Trim();

            var category = await _context.MenuCategories
                .FirstOrDefaultAsync(c =>
                    c.Id == Item.MenuCategoryId);

            if (category == null ||
                (!category.IsActive &&
                 category.Id != Item.MenuCategoryId))
            {
                ModelState.AddModelError(
                    "Item.MenuCategoryId",
                    "Please select a valid category.");
            }

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return Page();
            }

            var existing = await _context.MenuItems
                .FirstOrDefaultAsync(i => i.Id == id);

            if (existing == null)
            {
                return NotFound();
            }

            existing.Name = Item.Name;
            existing.Description = Item.Description;
            existing.MenuCategoryId = Item.MenuCategoryId;
            existing.Price = Item.Price;
            existing.IsAvailable = Item.IsAvailable;

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] =
                "Menu item updated successfully.";

            return RedirectToPage("./Index");
        }
    }
}