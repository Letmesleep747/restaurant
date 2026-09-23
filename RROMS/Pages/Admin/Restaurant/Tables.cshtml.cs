using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RROMS.Models;
using RROMS.Services.Admin;

namespace RROMS.Pages.Admin.Restaurant
{
    public class TablesModel : PageModel
    {
        private readonly RestaurantManagementService _restaurantService;


        public TablesModel(
            RestaurantManagementService restaurantService)
        {
            _restaurantService = restaurantService;
        }


        public List<RestaurantTable> RestaurantTables { get; set; }
            = new();


        public async Task OnGetAsync()
        {
            RestaurantTables =
                await _restaurantService.GetRestaurantTablesAsync();
        }


        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            var success =
                await _restaurantService
                    .ToggleRestaurantTableStatusAsync(id);


            if (!success)
            {
                TempData["ErrorMessage"] =
                    "Restaurant table could not be found.";

                return RedirectToPage();
            }


            TempData["SuccessMessage"] =
                "Restaurant table status updated successfully.";


            return RedirectToPage();
        }
    }
}