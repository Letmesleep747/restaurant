namespace restaurant.Models
{
    public class Table
    {
        public int Id { get; set; }
        public string Area { get; set; } // or DiningAreaId if normalized
        public int Seats { get; set; }
        public string Code { get; set; } // e.g., T1, T2
    }
}