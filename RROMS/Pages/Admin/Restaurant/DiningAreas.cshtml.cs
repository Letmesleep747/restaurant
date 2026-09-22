using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RROMS.Pages.Admin.Restaurant
{
    public class DiningAreasModel : PageModel
    {
        public List<DiningAreaItem> DiningAreas { get; set; }
            = new List<DiningAreaItem>();

        public void OnGet()
        {
            DiningAreas = new List<DiningAreaItem>
            {
                new DiningAreaItem
                {
                    Id = 1,
                    Name = "Indoor Dining",
                    Description = "Main air-conditioned dining section.",
                    IsActive = true
                },

                new DiningAreaItem
                {
                    Id = 2,
                    Name = "Outdoor Dining",
                    Description = "Outdoor seating section.",
                    IsActive = true
                },

                new DiningAreaItem
                {
                    Id = 3,
                    Name = "Private Room",
                    Description = "Private dining area for groups.",
                    IsActive = true
                }
            };
        }


        public class DiningAreaItem
        {
            public int Id { get; set; }

            public string Name { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            public bool IsActive { get; set; }
        }
    }
}