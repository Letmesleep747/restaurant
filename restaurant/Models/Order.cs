using System;
using System.Collections.Generic;

namespace restaurant.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime Created { get; set; }
        public string Status { get; set; } // Preparing, Ready, Served, Completed, Cancelled
        public decimal Total { get; set; }
        public ICollection<OrderItem> Items { get; set; }
    }
}