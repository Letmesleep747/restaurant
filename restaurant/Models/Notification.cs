using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using restaurant.Models.Enums;

namespace restaurant.Models
{
    public class Notification
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        [Required, StringLength(120)]
        public string Title { get; set; } = string.Empty;
        
        [Required, StringLength(500)]
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; } = NotificationType.System;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(200)]
        public string? LinkUrl { get; set; }
    }
}
