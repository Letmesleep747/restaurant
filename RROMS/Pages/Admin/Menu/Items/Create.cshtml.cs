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
        private readonly IWebHostEnvironment _environment;
        private const long MaxImageSize = 5 * 1024 * 1024; // 5 MB

        public CreateModel(ApplicationDbContext context, IWebHostEnvironment environment)
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
                .FirstOrDefaultAsync(c => c.Id == Item.MenuCategoryId);

            if (category == null || !category.IsActive)
            {
                ModelState.AddModelError("Item.MenuCategoryId", "Please select an active menu category.");
            }

            if (ImageFile != null && ImageFile.Length > 0 && !IsValidImage(ImageFile))
            {
                ModelState.AddModelError(nameof(ImageFile), "Upload a JPG, JPEG, PNG or WEBP image smaller than 5 MB.");
            }

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return Page();
            }

            string? savedImagePath = null;
            try
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    savedImagePath = await SaveImageAsync(ImageFile);
                    Item.ImageUrl = savedImagePath;
                }

                Item.CreatedAt = DateTime.UtcNow;
                _context.MenuItems.Add(Item);
                await _context.SaveChangesAsync();
            }
            catch
            {
                if (savedImagePath != null)
                {
                    DeleteImage(savedImagePath);
                }
                throw;
            }

            TempData["StatusMessage"] = "Menu item created successfully.";
            return RedirectToPage("./Index");
        }

        private static bool IsValidImage(IFormFile file)
        {
            if (file.Length > MaxImageSize) return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new Dictionary<string, string>
            {
                [".jpg"] = "image/jpeg",
                [".jpeg"] = "image/jpeg",
                [".png"] = "image/png",
                [".webp"] = "image/webp"
            };

            if (!allowed.TryGetValue(extension, out var expectedType) ||
                !string.Equals(file.ContentType, expectedType, StringComparison.OrdinalIgnoreCase))
                return false;

            return HasValidImageSignature(file, extension);
        }

        internal static bool HasValidImageSignature(IFormFile file, string extension)
        {
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
