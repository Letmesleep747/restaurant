using System.ComponentModel.DataAnnotations;

namespace restaurant.Models
{
    public class Promotion
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        // "Percentage" or "FixedAmount"
        [Required]
        [StringLength(20)]
        public string DiscountType { get; set; } = "Percentage";

        [Range(0.01, 100000)]
        public decimal DiscountValue { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
