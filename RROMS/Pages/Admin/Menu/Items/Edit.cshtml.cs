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
        private readonly IWebHostEnvironment _environment;
        private const long MaxImageSize = 5 * 1024 * 1024; // 5 MB

        public EditModel(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public MenuItem Item { get; set; } = new();

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public SelectList Categories { get; set; } = default!;

        private async Task LoadCategoriesAsync()
        {
            Categories = new SelectList(
                await _context.MenuCategories
                    .AsNoTracking()
                    .Where(c => c.IsActive || c.Id == Item.MenuCategoryId)
                    .OrderBy(c => c.Name)
                    .ToListAsync(),
                "Id", "Name", Item.MenuCategoryId);
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var item = await _context.MenuItems.AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null) return NotFound();

            Item = item;
            await LoadCategoriesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (id != Item.Id) return BadRequest();

            Item.Name = Item.Name?.Trim() ?? string.Empty;
            Item.Description = Item.Description?.Trim();

            var category = await _context.MenuCategories
                .FirstOrDefaultAsync(c => c.Id == Item.MenuCategoryId);

            if (category == null || (!category.IsActive && category.Id != Item.MenuCategoryId))
            {
                ModelState.AddModelError("Item.MenuCategoryId", "Please select a valid category.");
            }

            if (ImageFile != null && ImageFile.Length > 0 && !IsValidImage(ImageFile))
            {
                ModelState.AddModelError(nameof(ImageFile), "Upload a JPG, JPEG, PNG or WEBP image smaller than 5 MB.");
            }

            if (!ModelState.IsValid)
            {
                // Reload the existing image URL so the current image remains visible.
                var current = await _context.MenuItems.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
                Item.ImageUrl = current?.ImageUrl;
                await LoadCategoriesAsync();
                return Page();
            }

            var existing = await _context.MenuItems.FirstOrDefaultAsync(i => i.Id == id);
            if (existing == null) return NotFound();

            var oldImageUrl = existing.ImageUrl;
            string? newImageUrl = null;

            try
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    newImageUrl = await SaveImageAsync(ImageFile);
                    existing.ImageUrl = newImageUrl;
                }

                existing.Name = Item.Name;
                existing.Description = Item.Description;
                existing.MenuCategoryId = Item.MenuCategoryId;
                existing.Price = Item.Price;
                existing.IsAvailable = Item.IsAvailable;

                await _context.SaveChangesAsync();
            }
            catch
            {
                if (newImageUrl != null) DeleteImage(newImageUrl);
                throw;
            }

            // Delete the previous file only after the database update succeeds.
            if (newImageUrl != null && !string.IsNullOrWhiteSpace(oldImageUrl))
                DeleteImage(oldImageUrl);

            TempData["StatusMessage"] = "Menu item updated successfully.";
            return RedirectToPage("./Index");
        }

        private static bool IsValidImage(IFormFile file)
        {
            if (file.Length > MaxImageSize) return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new Dictionary<string, string>
            {
                [".jpg"] = "image/jpeg", [".jpeg"] = "image/jpeg",
                [".png"] = "image/png", [".webp"] = "image/webp"
            };

            if (!allowed.TryGetValue(extension, out var expectedType) ||
                !string.Equals(file.ContentType, expectedType, StringComparison.OrdinalIgnoreCase))
                return false;

            using var stream = file.OpenReadStream();
            var header = new byte[12];
            var read = stream.Read(header, 0, header.Length);
            return extension switch
            {
                ".jpg" or ".jpeg" => read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
                ".png" => read >= 8 && header.Take(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
                ".webp" => read >= 12 && header.Take(4).SequenceEqual("RIFF"u8.ToArray()) && header.Skip(8).Take(4).SequenceEqual("WEBP"u8.ToArray()),
                _ => false
            };
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "menu-items");
            Directory.CreateDirectory(uploadFolder);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(uploadFolder, fileName);
            await using var stream = new FileStream(physicalPath, FileMode.CreateNew);
            await file.CopyToAsync(stream);

            return $"/uploads/menu-items/{fileName}";
        }

        private void DeleteImage(string imageUrl)
        {
            if (!imageUrl.StartsWith("/uploads/menu-items/", StringComparison.OrdinalIgnoreCase)) return;
            var fileName = Path.GetFileName(imageUrl);
            var path = Path.Combine(_environment.WebRootPath, "uploads", "menu-items", fileName);
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }
    }
}
