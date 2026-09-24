using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RROMS.Models;
using RROMS.Services.Admin;
using RROMS.ViewModels.Admin;

namespace RROMS.Pages.Admin.Restaurant
{
    public class OperatingHoursModel : PageModel
    {
        private readonly RestaurantManagementService
            _restaurantService;


        public OperatingHoursModel(
            RestaurantManagementService restaurantService)
        {
            _restaurantService =
                restaurantService;
        }


        [BindProperty]
        public List<OperatingHourFormViewModel>
            Hours
        { get; set; } = new();


        public async Task OnGetAsync()
        {
            await LoadOperatingHoursAsync();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            ValidateOperatingHours();


            if (!ModelState.IsValid)
            {
                return Page();
            }


            var operatingHours =
                Hours.Select(h =>
                    new OperatingHour
                    {
                        OperatingHourId =
                            h.OperatingHourId,

                        Day =
                            h.Day,

                        OpeningTime =
                            h.IsClosed
                                ? null
                                : h.OpeningTime,

                        ClosingTime =
                            h.IsClosed
                                ? null
                                : h.ClosingTime,

                        IsClosed =
                            h.IsClosed
                    })
                    .ToList();


            var success =
                await _restaurantService
                    .UpdateOperatingHoursAsync(
                        operatingHours);


            if (!success)
            {
                TempData["ErrorMessage"] =
                    "Operating hours could not be updated.";

                return RedirectToPage();
            }


            TempData["SuccessMessage"] =
                "Operating hours updated successfully.";


            return RedirectToPage();
        }


        private async Task LoadOperatingHoursAsync()
        {
            var operatingHours =
                await _restaurantService
                    .GetOperatingHoursAsync();


            Hours =
                operatingHours
                    .Select(o =>
                        new OperatingHourFormViewModel
                        {
                            OperatingHourId =
                                o.OperatingHourId,

                            Day =
                                o.Day,

                            OpeningTime =
                                o.OpeningTime,

                            ClosingTime =
                                o.ClosingTime,

                            IsClosed =
                                o.IsClosed
                        })
                    .ToList();
        }


        private void ValidateOperatingHours()
        {
            for (int i = 0; i < Hours.Count; i++)
            {
                var hour = Hours[i];


                // No time validation required for closed days
                if (hour.IsClosed)
                {
                    continue;
                }


                if (!hour.OpeningTime.HasValue)
                {
                    ModelState.AddModelError(
                        $"Hours[{i}].OpeningTime",
                        "Opening time is required.");
                }


                if (!hour.ClosingTime.HasValue)
                {
                    ModelState.AddModelError(
                        $"Hours[{i}].ClosingTime",
                        "Closing time is required.");
                }


                if (hour.OpeningTime.HasValue &&
                    hour.ClosingTime.HasValue &&
                    hour.ClosingTime.Value
                    <= hour.OpeningTime.Value)
                {
                    ModelState.AddModelError(
                        $"Hours[{i}].ClosingTime",
                        "Closing time must be later than opening time.");
                }
            }
        }
    }
}