
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RROMS.Models
{
    [Table("RestaurantTables")]
    public class RestaurantTable
    {
        [Key]
        public int RestaurantTableId { get; set; }

        [Required]
        public int DiningAreaId { get; set; }

        [Required]
        [StringLength(20)]
        public string TableNumber { get; set; } = string.Empty;

        [Required]
        public int Capacity { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }
    }
}
