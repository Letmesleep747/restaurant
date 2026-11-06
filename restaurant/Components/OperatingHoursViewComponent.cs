using Microsoft.AspNetCore.Mvc;
using restaurant.Services;

namespace restaurant.Components
{
    public class OperatingHoursViewModel
    {
        public OperatingHoursSnapshot Snapshot { get; set; } = null!;
        public bool Compact { get; set; }
    }

    // Shows the weekly opening hours (default) or just an "Open now / Closed now"
    // line (compact: true). Renders nothing until the admin has set up hours.
    public class OperatingHoursViewComponent : ViewComponent
    {
        private readonly IOperatingHoursService _hours;

        public OperatingHoursViewComponent(IOperatingHoursService hours)
        {
            _hours = hours;
        }

        public async Task<IViewComponentResult> InvokeAsync(bool compact = false)
        {
            var snapshot = await _hours.GetSnapshotAsync();
            return View(new OperatingHoursViewModel { Snapshot = snapshot, Compact = compact });
        }
    }
}
