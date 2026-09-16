using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace restaurant.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Redirect the site root to the customer-facing landing page
            return RedirectToPage("/Customer/Index");
        }
    }
}
