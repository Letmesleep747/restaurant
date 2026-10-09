
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RROMS.Data;
using RROMS.Models;

namespace RROMS.Pages.Admin.Promotions
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Promotion> Promotions { get; set; }
            = new List<Promotion>();

        public async Task OnGetAsync()
        {
            Promotions = await _context.Promotions
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            var promotion = await _context.Promotions
                .FirstOrDefaultAsync(p => p.Id == id);

            if (promotion == null)
            {
                return NotFound();
            }

            promotion.IsActive = !promotion.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                promotion.IsActive
                    ? "Promotion activated successfully."
                    : "Promotion deactivated successfully.";

            return RedirectToPage();
        }
    }
}