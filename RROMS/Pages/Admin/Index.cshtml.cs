using Microsoft.AspNetCore.Mvc.RazorPages;
using RROMS.ViewModels.Admin;

namespace RROMS.Pages.Admin
{
    public class IndexModel : PageModel
    {
        public DashboardViewModel Dashboard { get; set; }
            = new DashboardViewModel();

        public void OnGet()
        {
            Dashboard = new DashboardViewModel
            {
                TotalTables = 24,
                TodayReservations = 12,
                TodayOrders = 18,
                TodaySales = 1250.50m
            };
        }
    }
}