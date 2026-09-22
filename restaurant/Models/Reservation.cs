using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using restaurant.Models.Enums;

namespace restaurant.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        [ForeignKey(nameof(CustomerId))]

        public ApplicationUser? Customer { get; set; }
        public int? TableId { get; set; }

        [ForeignKey(nameof(TableId))]
        public DiningTable? Table { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ReservationDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        // Standard sitting length used for overlap checking.
        public int DurationMinutes { get; set; } = 90;

        [Range(1,20)]
        public int PartySize { get; set; }
        public ResercationStatus Status { get; set; } = ResercationStatus.Pending;

        [StringLength(500)]
        public string? SpecialRequests { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [NotMapped]
        public DateTime EndDateTime => StartDateTime.AddMinutes(DurationMinutes);
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
