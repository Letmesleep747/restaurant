using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace restaurant.Pages
{
    // Site root should not dump customers on a reservation-heavy dashboard.
    public class IndexModel : PageModel
    {
        public IActionResult OnGet() => RedirectToPage("/Customer/Index");
    }
}
