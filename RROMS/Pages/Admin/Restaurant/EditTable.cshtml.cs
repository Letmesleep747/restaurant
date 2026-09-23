using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RROMS.Models;
using RROMS.Services.Admin;
using RROMS.ViewModels.Admin;

namespace RROMS.Pages.Admin.Restaurant
{
    public class EditTableModel : PageModel
    {
        private readonly RestaurantManagementService
            _restaurantService;


        public EditTableModel(
            RestaurantManagementService restaurantService)
        {
            _restaurantService =
                restaurantService;
        }


        [BindProperty]
        public RestaurantTableFormViewModel Input { get; set; }
            = new();


        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }


        public List<DiningArea> DiningAreas { get; set; }
            = new();



        public async Task<IActionResult> OnGetAsync()
        {
            var restaurantTable =
                await _restaurantService
                    .GetRestaurantTableByIdAsync(Id);


            if (restaurantTable == null)
            {
                return NotFound();
            }


            await LoadDiningAreasAsync(
                restaurantTable.DiningAreaId);


            Input =
                new RestaurantTableFormViewModel
                {
                    DiningAreaId =
                        restaurantTable.DiningAreaId,

                    TableNumber =
                        restaurantTable.TableNumber,

                    Capacity =
                        restaurantTable.Capacity,

                    IsActive =
                        restaurantTable.IsActive
                };


            return Page();
        }



        public async Task<IActionResult> OnPostAsync()
        {
            var existingTable =
                await _restaurantService
                    .GetRestaurantTableByIdAsync(Id);


            if (existingTable == null)
            {
                return NotFound();
            }


            await LoadDiningAreasAsync(
                existingTable.DiningAreaId);


            if (!ModelState.IsValid)
            {
                return Page();
            }


            // If Admin changes Dining Area,
            // the new Dining Area must be active.
            if (Input.DiningAreaId
                != existingTable.DiningAreaId)
            {
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
            }


            var cleanTableNumber =
                Input.TableNumber
                    .Trim()
                    .ToUpper();


            // Duplicate validation
            var tableExists =
                await _restaurantService
                    .TableNumberExistsAsync(
                        cleanTableNumber,
                        Id);


            if (tableExists)
            {
                ModelState.AddModelError(
                    "Input.TableNumber",
                    "A table with this number already exists.");

                return Page();
            }


            var success =
                await _restaurantService
                    .UpdateRestaurantTableAsync(
                        Id,
                        Input.DiningAreaId,
                        cleanTableNumber,
                        Input.Capacity,
                        Input.IsActive);


            if (!success)
            {
                return NotFound();
            }


            TempData["SuccessMessage"] =
                "Restaurant table updated successfully.";


            return RedirectToPage("Tables");
        }



        private async Task LoadDiningAreasAsync(
            int currentDiningAreaId)
        {
            DiningAreas =
                await _restaurantService
                    .GetDiningAreasForTableEditAsync(
                        currentDiningAreaId);
        }
    }
}