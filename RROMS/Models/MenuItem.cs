using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RROMS.Models
{
    [Table("MenuItems")]
    public class MenuItem
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        public int MenuCategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        public bool IsAvailable { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public string? Category { get; set; }

        public string? ImageUrl { get; set; }
    }
}