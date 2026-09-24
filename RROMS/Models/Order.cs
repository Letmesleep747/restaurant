using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RROMS.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        public int? ReservationId { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = "";

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public string OrderStatus { get; set; } = "Preparing";

        [StringLength(500)]
        public string? SpecialRequest { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new();
    }
}