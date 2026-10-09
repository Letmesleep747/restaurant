
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RROMS.Data;
using RROMS.Models;

namespace RROMS.Pages.Admin.SystemSettings
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SystemSetting Setting { get; set; } = new();

        public async Task OnGetAsync()
        {
            var existing = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Id == 1);

            if (existing != null)
                Setting = existing;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var existing = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Id == 1);

            if (existing == null)
            {
                Setting.Id = 1;
                Setting.UpdatedAt = DateTime.UtcNow;

                _context.SystemSettings.Add(Setting);
            }
            else
            {
                existing.RestaurantName = Setting.RestaurantName;
                existing.ContactEmail = Setting.ContactEmail;
                existing.ContactPhone = Setting.ContactPhone;
                existing.Address = Setting.Address;
                existing.DefaultReservationDurationMinutes =
                    Setting.DefaultReservationDurationMinutes;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "System settings saved successfully.";

            return RedirectToPage();
        }
    }
}