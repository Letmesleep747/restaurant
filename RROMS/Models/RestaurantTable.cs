using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RROMS.Models
{
    public class RestaurantTable
    {
        [Key]
        public int RestaurantTableId { get; set; }


        [Required]
        public int DiningAreaId { get; set; }


        [Required]
        [StringLength(20)]
        public string TableNumber { get; set; } = string.Empty;


        [Range(1, 100)]
        public int Capacity { get; set; }


        public bool IsActive { get; set; } = true;


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        // Navigation Property
        [ForeignKey("DiningAreaId")]
        public DiningArea? DiningArea { get; set; }
    }
}