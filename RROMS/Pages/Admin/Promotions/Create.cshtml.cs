
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RROMS.Data;
using RROMS.Models;

namespace RROMS.Pages.Admin.Promotions
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Promotion Promotion { get; set; } = new();

        public void OnGet()
        {
            Promotion.StartDate = DateTime.Today;
            Promotion.EndDate = DateTime.Today.AddDays(7);
            Promotion.DiscountType = "Percentage";
            Promotion.IsActive = true;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ValidatePromotion();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            Promotion.Name = Promotion.Name.Trim();
            Promotion.Description =
                string.IsNullOrWhiteSpace(Promotion.Description)
                    ? null
                    : Promotion.Description.Trim();

            Promotion.CreatedAt = DateTime.UtcNow;

            _context.Promotions.Add(Promotion);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Promotion created successfully.";

            return RedirectToPage("./Index");
        }

        private void ValidatePromotion()
        {
            if (Promotion.DiscountType != "Percentage" &&
                Promotion.DiscountType != "FixedAmount")
            {
                ModelState.AddModelError(
                    "Promotion.DiscountType",
                    "Please select a valid discount type.");
            }

            if (Promotion.DiscountType == "Percentage" &&
                (Promotion.DiscountValue <= 0 ||
                 Promotion.DiscountValue > 100))
            {
                ModelState.AddModelError(
                    "Promotion.DiscountValue",
                    "Percentage discounts must be between 0.01 and 100.");
            }

            if (Promotion.StartDate.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    "Promotion.StartDate",
                    "The start date cannot be in the past.");
            }

            if (Promotion.EndDate.Date < Promotion.StartDate.Date)
            {
                ModelState.AddModelError(
                    "Promotion.EndDate",
                    "The end date must be on or after the start date.");
            }
        }
    }
}