using System;
using System.Security.Cryptography.X509Certificates;
namespace restaurant.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser User { get; set; }
        public int TableId { get; set; }
        public Table Table { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int Guests { get; set; }
        public string Status { get; set; } // e.g., "Pending", "Confirmed", "Cancelled"
        public string Notes { get; set; }
    }
}
