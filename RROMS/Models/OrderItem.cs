using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RROMS.Models
{
    [Table("OrderItems")]
    public class OrderItem
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int MenuItemId { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        // Relationships
        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }

        [ForeignKey(nameof(MenuItemId))]
        public MenuItem? MenuItem { get; set; }
    }
}