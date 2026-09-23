using System.ComponentModel.DataAnnotations;

namespace RROMS.Models
{
    public class DiningArea
    {
        [Key]
        public int DiningAreaId { get; set; }


        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;


        [StringLength(300)]
        public string? Description { get; set; }


        public bool IsActive { get; set; } = true;


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        // One Dining Area can contain many Restaurant Tables
        public ICollection<RestaurantTable> RestaurantTables { get; set; }
            = new List<RestaurantTable>();
    }
}