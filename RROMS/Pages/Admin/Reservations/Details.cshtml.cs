using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RROMS.Data;
using RROMS.Models;

namespace RROMS.Pages.Admin.Reservations
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Reservation? Reservation { get; private set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Reservation = await _context.Reservations
                .AsNoTracking()
                .Include(r => r.Customer)
                .Include(r => r.Table)
                    .ThenInclude(t => t!.DiningArea)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (Reservation == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}