using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RROMS.Models;
using RROMS.Services.Admin;
using RROMS.ViewModels.Admin;

namespace RROMS.Pages.Admin.Restaurant
{
    public class CreateTableModel : PageModel
    {
        private readonly RestaurantManagementService
            _restaurantService;


        public CreateTableModel(
            RestaurantManagementService restaurantService)
        {
            _restaurantService =
                restaurantService;
        }


        [BindProperty]
        public RestaurantTableFormViewModel Input { get; set; }
            = new();


        public List<DiningArea> DiningAreas { get; set; }
            = new();


        public async Task OnGetAsync()
        {
            await LoadDiningAreasAsync();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            await LoadDiningAreasAsync();


            if (!ModelState.IsValid)
            {
                return Page();
            }


            // Verify dining area is active
            var diningAreaIsActive =
                await _restaurantService
                    .IsDiningAreaActiveAsync(
                        Input.DiningAreaId);


            if (!diningAreaIsActive)
            {
                ModelState.AddModelError(
                    "Input.DiningAreaId",
                    "The selected dining area is not available.");

                return Page();
            }


            var cleanTableNumber =
                Input.TableNumber
                    .Trim()
                    .ToUpper();


            // Prevent duplicate table numbers
            var tableExists =
                await _restaurantService
                    .TableNumberExistsAsync(
                        cleanTableNumber);


            if (tableExists)
            {
                ModelState.AddModelError(
                    "Input.TableNumber",
                    "A table with this number already exists.");

                return Page();
            }


            var restaurantTable =
                new RestaurantTable
                {
                    DiningAreaId =
                        Input.DiningAreaId,

                    TableNumber =
                        cleanTableNumber,

                    Capacity =
                        Input.Capacity,

                    IsActive =
                        Input.IsActive
                };


            await _restaurantService
                .CreateRestaurantTableAsync(
                    restaurantTable);


            TempData["SuccessMessage"] =
                "Restaurant table created successfully.";


            return RedirectToPage("Tables");
        }


        private async Task LoadDiningAreasAsync()
        {
            DiningAreas =
                await _restaurantService
                    .GetActiveDiningAreasAsync();
        }
    }
}