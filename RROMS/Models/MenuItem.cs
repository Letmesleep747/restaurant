using System.ComponentModel.DataAnnotations;

namespace RROMS.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        [Required]
        public int MenuCategoryId { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Range(0.01, 999999.99)]
        public decimal Price { get; set; }

        public bool IsAvailable { get; set; } = true;

        [StringLength(1000)]
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public MenuCategory? MenuCategory { get; set; }
    }
}
