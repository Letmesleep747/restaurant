using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using restaurant.Data;
using restaurant.Models;

namespace restaurant.Pages.Admin.Promotions
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Promotion Promotion { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var promotion = await _context.Promotions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (promotion == null)
            {
                return NotFound();
            }

            Promotion = promotion;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Promotion.DiscountType != "Percentage" &&
                Promotion.DiscountType != "FixedAmount")
            {
                ModelState.AddModelError("Promotion.DiscountType",
                    "Please select a valid discount type.");
            }

            if (Promotion.DiscountType == "Percentage" &&
                (Promotion.DiscountValue <= 0 || Promotion.DiscountValue > 100))
            {
                ModelState.AddModelError("Promotion.DiscountValue",
                    "Percentage discounts must be between 0.01 and 100.");
            }

            if (Promotion.EndDate.Date < Promotion.StartDate.Date)
            {
                ModelState.AddModelError("Promotion.EndDate",
                    "The end date must be on or after the start date.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existing = await _context.Promotions
                .FirstOrDefaultAsync(p => p.Id == Promotion.Id);

            if (existing == null)
            {
                return NotFound();
            }

            existing.Name = Promotion.Name.Trim();
            existing.Description = string.IsNullOrWhiteSpace(Promotion.Description)
                ? null
                : Promotion.Description.Trim();
            existing.DiscountType = Promotion.DiscountType;
            existing.DiscountValue = Promotion.DiscountValue;
            existing.StartDate = Promotion.StartDate;
            existing.EndDate = Promotion.EndDate;
            existing.IsActive = Promotion.IsActive;
            // CreatedAt is intentionally left unchanged.

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Promotion updated successfully.";
            return RedirectToPage("./Index");
        }
    }
}
