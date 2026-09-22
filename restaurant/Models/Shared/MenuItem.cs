using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace restaurant.Models.Shared
{
    // Normally managed by the Admin module.
    // Menu Browsing + Ordering
    // If Admin module already has a MenuItem model, dlt this file and update the 'using' / namespace as needed.
    public class MenuItem
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required, Column(TypeName = "decimal(8,2)")]
        [Range(0, 10000)]
        public decimal Price { get; set; }

        [Required, StringLength(50)]
        public string Category { get; set; } = "Main Course";
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}
