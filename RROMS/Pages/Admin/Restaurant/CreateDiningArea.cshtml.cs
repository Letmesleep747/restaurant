using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RROMS.Models;
using RROMS.Services.Admin;
using RROMS.ViewModels.Admin;

namespace RROMS.Pages.Admin.Restaurant
{
    public class CreateDiningAreaModel : PageModel
    {
        private readonly RestaurantManagementService
            _restaurantService;

        public CreateDiningAreaModel(
            RestaurantManagementService restaurantService)
        {
            _restaurantService = restaurantService;
        }


        [BindProperty]
        public DiningAreaFormViewModel Input { get; set; }
            = new();


        public void OnGet()
        {
        }


        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var cleanName = Input.Name.Trim();


            if (await _restaurantService
                .DiningAreaNameExistsAsync(cleanName))
            {
                ModelState.AddModelError(
                    "Input.Name",
                    "A dining area with this name already exists.");

                return Page();
            }


            var diningArea = new DiningArea
            {
                Name = cleanName,

                Description =
                    string.IsNullOrWhiteSpace(Input.Description)
                    ? null
                    : Input.Description.Trim(),

                IsActive = Input.IsActive
            };


            await _restaurantService
                .CreateDiningAreaAsync(diningArea);


            TempData["SuccessMessage"] =
                "Dining area created successfully.";


            return RedirectToPage("DiningAreas");
        }
    }
}