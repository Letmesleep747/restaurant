using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace restaurant.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;
        // add profile fields like PhoneNumber, Address, etc. as needed

        [StringLength(250)]
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
