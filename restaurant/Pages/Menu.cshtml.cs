using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using restaurant.Data;
using restaurant.Models;
using restaurant.Models.Shared;

namespace restaurant.Pages.Customer
{
    public class MenuModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public MenuModel(ApplicationDbContext db)
        {
            _db = db;
        }

        [BindProperty(SupportsGet = true)]
        public string? CategoryFilter { get; set; }

        public List<string> Categories { get; set; } = new();
        public List<MenuItem> Items { get; set; } = new();

        public async Task OnGetAsync()
        {
            Categories = await _db.MenuItems
                .Select(m => m.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var query = _db.MenuItems.Where(m => m.IsAvailable).AsQueryable();

            if (!string.IsNullOrWhiteSpace(CategoryFilter))
                query = query.Where(m => m.Category == CategoryFilter);

            Items = await query.OrderBy(m => m.Category).ThenBy(m => m.Name).ToListAsync();
        }
    }
}
