using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using restaurant.Data;
using restaurant.Models.Shared;

namespace restaurant.Pages.Customer
{
    public class MenuModel : PageModel
    {
        private static readonly string[] PreferredCategoryOrder =
        {
            "Starter", "Starters", "Main Course", "Mains", "Dessert", "Desserts", "Beverage", "Beverages", "Drink", "Drinks"
        };

        private readonly ApplicationDbContext _db;

        public MenuModel(ApplicationDbContext db)
        {
            _db = db;
        }

        [BindProperty(SupportsGet = true)]
        public string? CategoryFilter { get; set; }

        public List<string> Categories { get; set; } = new();
        public Dictionary<string, int> CategoryCounts { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public List<MenuItem> Items { get; set; } = new();
        public List<MenuCategoryGroup> Groups { get; set; } = new();

        public async Task OnGetAsync()
        {
            var available = await _db.MenuItems
                .Where(m => m.IsAvailable)
                .OrderBy(m => m.Name)
                .ToListAsync();

            CategoryCounts = available
                .GroupBy(m => string.IsNullOrWhiteSpace(m.Category) ? "Other" : m.Category.Trim(),
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

            Categories = CategoryCounts.Keys
                .OrderBy(CategorySortKey)
                .ThenBy(c => c)
                .ToList();

            var filtered = available;
            if (!string.IsNullOrWhiteSpace(CategoryFilter))
            {
                filtered = available
                    .Where(m => string.Equals(
                        string.IsNullOrWhiteSpace(m.Category) ? "Other" : m.Category.Trim(),
                        CategoryFilter,
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            Items = filtered;

            Groups = filtered
                .GroupBy(m => string.IsNullOrWhiteSpace(m.Category) ? "Other" : m.Category.Trim(),
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => CategorySortKey(g.Key))
                .ThenBy(g => g.Key)
                .Select(g => new MenuCategoryGroup
                {
                    Category = g.Key,
                    Items = g.OrderBy(i => i.Name).ToList()
                })
                .ToList();
        }

        private static int CategorySortKey(string category)
        {
            var index = Array.FindIndex(PreferredCategoryOrder,
                c => string.Equals(c, category, StringComparison.OrdinalIgnoreCase));
            return index >= 0 ? index : 100;
        }

        public class MenuCategoryGroup
        {
            public string Category { get; set; } = string.Empty;
            public List<MenuItem> Items { get; set; } = new();
        }
    }
}
