using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RROMS.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        public int? ReservationId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        public string Status { get; set; } = "Pending";

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        public string? Notes { get; set; }

        // Relationships
        [ForeignKey(nameof(CustomerId))]
        public ApplicationUser? Customer { get; set; }

        [ForeignKey(nameof(ReservationId))]
        public Reservation? Reservation { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new();

        public List<Payment> Payments { get; set; } = new();
    }
}