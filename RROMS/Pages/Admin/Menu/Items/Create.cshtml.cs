
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RROMS.Data;
using RROMS.Models;

namespace RROMS.Pages.Admin.Menu.Items
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
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
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync(),
                "Id",
                "Name");
        }

        public async Task OnGetAsync()
        {
            await LoadCategoriesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Item.Name = Item.Name?.Trim() ?? string.Empty;
            Item.Description = Item.Description?.Trim();

            var category = await _context.MenuCategories
                .FirstOrDefaultAsync(c =>
                    c.Id == Item.MenuCategoryId);

            if (category == null || !category.IsActive)
            {
                ModelState.AddModelError(
                    "Item.MenuCategoryId",
                    "Please select an active menu category.");
            }

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return Page();
            }

            Item.CreatedAt = DateTime.UtcNow;

            _context.MenuItems.Add(Item);
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] =
                "Menu item created successfully.";

            return RedirectToPage("./Index");
        }
    }
}