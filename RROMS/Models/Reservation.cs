
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RROMS.Models
{
    [Table("Reservations")]
    public class Reservation
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        public int? TableId { get; set; }

        [Required]
        public DateTime ReservationDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public int DurationMinutes { get; set; }

        [Required]
        [Range(1, 20)]
        public int PartySize { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        [StringLength(500)]
        public string? SpecialRequests { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Relationships
        [ForeignKey(nameof(CustomerId))]
        public ApplicationUser? Customer { get; set; }

        [ForeignKey(nameof(TableId))]
        public RestaurantTable? RestaurantTable { get; set; }
    }
}
