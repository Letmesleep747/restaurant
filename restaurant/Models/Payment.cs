using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using restaurant.Models.Enums;

namespace restaurant.Models
{
    public class Payment
    {
        public int Id { get; set; }
        [Required]
        public int OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; } = PaymentMethod.Card;
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime? TransactionDate { get; set; }

        [StringLength(100)]
        public string? TransactionReference { get; set; }

    }
}
