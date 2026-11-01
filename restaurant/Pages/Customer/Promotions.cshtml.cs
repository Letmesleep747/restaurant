using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using restaurant.Data;
using restaurant.Models;

namespace restaurant.Pages.Customer
{
    public class PromotionsModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public PromotionsModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public List<Promotion> Promotions { get; set; } = new();

        public async Task OnGetAsync()
        {
            var today = DateTime.Today;

            Promotions = await _db.Promotions
                .AsNoTracking()
                .Where(p => p.IsActive
                         && p.StartDate <= today
                         && p.EndDate >= today)
                .OrderBy(p => p.EndDate)
                .ToListAsync();
        }
    }
}
