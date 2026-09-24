using System.ComponentModel.DataAnnotations;

namespace RROMS.Models
{
    public class Reservation
    {
        public int ReservationId { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = "";

        [Required]
        [Phone]
        public string CustomerPhone { get; set; } = "";

        [Required]
        [DataType(DataType.Date)]
        public DateTime ReservationDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan ReservationTime { get; set; }

        [Range(1, 20)]
        public int NumberOfGuests { get; set; }

        [Required]
        public int TableNumber { get; set; }

        [StringLength(500)]
        public string? SpecialRequest { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";
    }
}