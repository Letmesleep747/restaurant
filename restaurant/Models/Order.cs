using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        public Payment? Payment { get; set; }
    }

    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }
        [Required]
        public int MenuItemId { get; set; }

        [ForeignKey(nameof(MenuItemId))]
        public MenuItem? MenuItem { get; set; }

        [Range(1, 50)]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal UnitPrice { get; set; }

        [NotMapped]
        public decimal Subtotal => Quantity * UnitPrice;
    }
}