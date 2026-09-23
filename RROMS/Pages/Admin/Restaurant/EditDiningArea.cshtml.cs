using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RROMS.Services.Admin;
using RROMS.ViewModels.Admin;

namespace RROMS.Pages.Admin.Restaurant
{
    public class EditDiningAreaModel : PageModel
    {
        private readonly RestaurantManagementService
            _restaurantService;


        public EditDiningAreaModel(
            RestaurantManagementService restaurantService)
        {
            _restaurantService = restaurantService;
        }


        [BindProperty]
        public DiningAreaFormViewModel Input { get; set; }
            = new();


        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }


        public async Task<IActionResult> OnGetAsync()
        {
            var diningArea =
                await _restaurantService
                    .GetDiningAreaByIdAsync(Id);


            if (diningArea == null)
            {
                return NotFound();
            }


            Input = new DiningAreaFormViewModel
            {
                Name = diningArea.Name,
                Description = diningArea.Description,
                IsActive = diningArea.IsActive
            };


            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }


            var cleanName = Input.Name.Trim();


            if (await _restaurantService
                .DiningAreaNameExistsAsync(cleanName, Id))
            {
                ModelState.AddModelError(
                    "Input.Name",
                    "A dining area with this name already exists.");

                return Page();
            }


            var success =
                await _restaurantService.UpdateDiningAreaAsync(
                    Id,
                    cleanName,
                    string.IsNullOrWhiteSpace(Input.Description)
                        ? null
                        : Input.Description.Trim(),
                    Input.IsActive);


            if (!success)
            {
                return NotFound();
            }


            TempData["SuccessMessage"] =
                "Dining area updated successfully.";


            return RedirectToPage("DiningAreas");
        }
    }
}