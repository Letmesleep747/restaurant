using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RROMS.Models;
using RROMS.Services.Admin;

namespace RROMS.Pages.Admin.Restaurant
{
    public class DiningAreasModel : PageModel
    {
        private readonly RestaurantManagementService
            _restaurantService;

        public DiningAreasModel(
            RestaurantManagementService restaurantService)
        {
            _restaurantService = restaurantService;
        }

        public List<DiningArea> DiningAreas { get; set; }
            = new();


        public async Task OnGetAsync()
        {
            DiningAreas =
                await _restaurantService.GetDiningAreasAsync();
        }


        public async Task<IActionResult> OnPostToggleStatusAsync(
            int id)
        {
            var success =
                await _restaurantService.ToggleDiningAreaStatusAsync(id);

            if (!success)
            {
                TempData["ErrorMessage"] =
                    "Dining area could not be found.";

                return RedirectToPage();
            }

            TempData["SuccessMessage"] =
                "Dining area status updated successfully.";

            return RedirectToPage();
        }
    }
}