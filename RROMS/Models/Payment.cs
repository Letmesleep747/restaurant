using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RROMS.Models
{
    [Table("Payments")]
    public class Payment
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        public string Method { get; set; } = "Cash";

        [Required]
        public string Status { get; set; } = "Unpaid";

        public DateTime? TransactionDate { get; set; }

        public string? TransactionReference { get; set; }

        // Relationship
        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }
    }
}