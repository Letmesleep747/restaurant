
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RROMS.Pages.Admin.Orders
{
    public class DetailsModel : PageModel
    {
        public int Id { get; private set; }

        public void OnGet(int id)
        {
            Id = id;
        }
    }
}