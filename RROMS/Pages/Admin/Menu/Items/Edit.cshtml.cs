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
            var categories = await _context.MenuCategories
                .AsNoTracking()
                .Where(c => c.IsActive || c.Id == Item.MenuCategoryId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            Categories = new SelectList(
                categories,
                "Id",
                "Name",
                Item.MenuCategoryId
            );
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var item = await _context.MenuItems
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
                return NotFound();

            Item = item;

            await LoadCategoriesAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (id != Item.Id)
                return BadRequest();

            Item.Name = Item.Name?.Trim() ?? string.Empty;

            Item.Description = Item.Description?.Trim();

            Item.ImageUrl = string.IsNullOrWhiteSpace(Item.ImageUrl)
                ? null
                : Item.ImageUrl.Trim();

            var category = await _context.MenuCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.Id == Item.MenuCategoryId);

            if (category == null || !category.IsActive)
            {
                ModelState.AddModelError(
                    "Item.MenuCategoryId",
                    "Please select an active menu category."
                );
            }

            if (!IsValidImageUrl(Item.ImageUrl))
            {
                ModelState.AddModelError(
                    "Item.ImageUrl",
                    "Please enter a valid HTTP or HTTPS image URL."
                );
            }

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return Page();
            }

            var existing = await _context.MenuItems
                .FirstOrDefaultAsync(i => i.Id == id);

            if (existing == null)
                return NotFound();

            // Update both category columns.
            existing.MenuCategoryId = category!.Id;
            existing.Category = category.Name;

            existing.Name = Item.Name;
            existing.Description = Item.Description;
            existing.Price = Item.Price;
            existing.ImageUrl = Item.ImageUrl;
            existing.IsAvailable = Item.IsAvailable;

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] =
                "Menu item updated successfully.";

            return RedirectToPage("./Index");
        }

        private static bool IsValidImageUrl(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return true;

            if (imageUrl.StartsWith(
                "/uploads/menu-items/",
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return Uri.TryCreate(
                    imageUrl,
                    UriKind.Absolute,
                    out var uri)
                && (uri.Scheme == Uri.UriSchemeHttps ||
                    uri.Scheme == Uri.UriSchemeHttp)
                && !string.IsNullOrWhiteSpace(uri.Host);
        }
    }
}
