using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RROMS.Pages.Admin.Restaurant
{
    public class TablesModel : PageModel
    {
        public List<RestaurantTableItem> RestaurantTables { get; set; }
            = new List<RestaurantTableItem>();

        public void OnGet()
        {
            RestaurantTables = new List<RestaurantTableItem>
            {
                new RestaurantTableItem
                {
                    TableNumber = "T01",
                    DiningArea = "Indoor Dining",
                    Capacity = 2,
                    IsActive = true
                },

                new RestaurantTableItem
                {
                    TableNumber = "T02",
                    DiningArea = "Indoor Dining",
                    Capacity = 4,
                    IsActive = true
                },

                new RestaurantTableItem
                {
                    TableNumber = "T03",
                    DiningArea = "Outdoor Dining",
                    Capacity = 4,
                    IsActive = true
                },

                new RestaurantTableItem
                {
                    TableNumber = "T04",
                    DiningArea = "Private Room",
                    Capacity = 8,
                    IsActive = true
                }
            };
        }


        public class RestaurantTableItem
        {
            public string TableNumber { get; set; } = string.Empty;

            public string DiningArea { get; set; } = string.Empty;

            public int Capacity { get; set; }

            public bool IsActive { get; set; }
        }
    }
}