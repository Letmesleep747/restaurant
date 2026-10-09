
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RROMS.Data;
using RROMS.Models;

namespace RROMS.Pages.Admin.Menu.Categories
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MenuCategory Category { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Category.Name = Category.Name?.Trim() ?? string.Empty;
            Category.Description = Category.Description?.Trim();

            if (await _context.MenuCategories
                .AnyAsync(c => c.Name == Category.Name))
            {
                ModelState.AddModelError(
                    "Category.Name",
                    "A category with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            Category.CreatedAt = DateTime.UtcNow;

            _context.MenuCategories.Add(Category);
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] =
                "Menu category created successfully.";

            return RedirectToPage("./Index");
        }
    }
}