using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace restaurant.Models.Shared
{
    public class DiningTable
    {
        public int Id { get; set; }

        [Required, StringLength(10)]
        public string TableNumber { get; set; } = string.Empty;

        [Range(1, 20)]
        public int Capacity { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
