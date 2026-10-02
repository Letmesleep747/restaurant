using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using restaurant.Models.Enums;
using restaurant.Models.Shared;

namespace restaurant.Models
{
    public class Order
    {
        public int Id { get; set; }
        [Required]
        public string CustomerId { get; set; } = string.Empty;
        [ForeignKey(nameof(CustomerId))]
        public ApplicationUser? Customer { get; set; }

        // Link a dine in order to a table reservation (optional)
        public int? ReservationId { get; set; }

        [ForeignKey(nameof(ReservationId))]
        public Reservation? Reservation { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Placed;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(300)]
        public string? Notes { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        [NotMapped]
        public Payment? Payment => Payments?.OrderByDescending(p => p.TransactionDate).FirstOrDefault();
    }

    // OrderItem moved to its own file (Models/OrderItem.cs)
}