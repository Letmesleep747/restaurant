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
            var categories = await _context.MenuCategories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            Categories = new SelectList(
                categories,
                "Id",
                "Name",
                Item.MenuCategoryId
            );
        }

        public async Task OnGetAsync()
        {
            await LoadCategoriesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Item.Name = Item.Name?.Trim() ?? string.Empty;
            Item.Description = Item.Description?.Trim();

            Item.ImageUrl = string.IsNullOrWhiteSpace(Item.ImageUrl)
                ? null
                : Item.ImageUrl.Trim();

            var category = await _context.MenuCategories
                .FirstOrDefaultAsync(c => c.Id == Item.MenuCategoryId);

            if (category == null || !category.IsActive)
            {
                ModelState.AddModelError(
                    "Item.MenuCategoryId",
                    "Please select an active menu category."
                );
            }
            else
            {
                // Category is obtained from the selected category record.
                Item.Category = category.Name;

                // Remove the validation error because Category is not
                // submitted separately by the form.
                ModelState.Remove("Item.Category");
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

            Item.CreatedAt = DateTime.UtcNow;

            _context.MenuItems.Add(Item);

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "Menu item created successfully.";

            return RedirectToPage("./Index");
        }

        private static bool IsValidImageUrl(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return true;

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
