using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RROMS.Data;

namespace RROMS.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalTables { get; private set; }
        public int ActiveTables { get; private set; }

        public int TodayReservations { get; private set; }
        public int TodayOrders { get; private set; }
        public decimal TodaySales { get; private set; }

        public int PendingReservations { get; private set; }
        public int ConfirmedReservations { get; private set; }
        public int CancelledReservations { get; private set; }

        public List<RecentReservation> RecentReservations { get; private set; }
            = new();

        public List<RecentOrder> RecentOrders { get; private set; }
            = new();

        public async Task OnGetAsync()
        {
            // Malaysia local date (UTC+8).
            var today = DateTime.UtcNow.AddHours(8).Date;
            var tomorrow = today.AddDays(1);

            // Restaurant tables.
            TotalTables = await _context.RestaurantTables
                .AsNoTracking()
                .CountAsync();

            ActiveTables = await _context.RestaurantTables
                .AsNoTracking()
                .CountAsync(t => t.IsActive);

            // Today's reservations.
            var reservations = _context.Reservations
                .AsNoTracking()
                .Where(r =>
                    r.ReservationDate >= today &&
                    r.ReservationDate < tomorrow);

            TodayReservations = await reservations.CountAsync();

            PendingReservations = await reservations.CountAsync(r =>
                r.Status == "Pending");

            ConfirmedReservations = await reservations.CountAsync(r =>
                r.Status == "Confirmed");

            CancelledReservations = await reservations.CountAsync(r =>
                r.Status == "Cancelled");

            // Today's orders.
            TodayOrders = await _context.Orders
                .AsNoTracking()
                .CountAsync(o =>
                    o.OrderDate >= today &&
                    o.OrderDate < tomorrow);

            // Today's successfully recorded paid transactions.
            var paidPayments = _context.Payments
                .AsNoTracking()
                .Where(p =>
                    p.Status == "Paid" &&
                    p.TransactionDate.HasValue &&
                    p.TransactionDate.Value >= today &&
                    p.TransactionDate.Value < tomorrow);

            TodaySales = await paidPayments
                .Select(p => (decimal?)p.Amount)
                .SumAsync() ?? 0m;

            // Recent reservations.
            var recentReservations = await _context.Reservations
                .AsNoTracking()
                .Include(r => r.Customer)
                .Include(r => r.Table)
                .OrderByDescending(r => r.CreatedAt)
                .ThenByDescending(r => r.Id)
                .Take(5)
                .ToListAsync();

            RecentReservations = recentReservations
                .Select(r => new RecentReservation
                {
                    Id = r.Id,
                    CustomerName = r.Customer != null
                        ? r.Customer.FullName
                        : "Unknown customer",
                    ReservationDate = r.ReservationDate,
                    StartTime = r.StartTime,
                    PartySize = r.PartySize,
                    TableNumber = r.Table != null
                        ? r.Table.TableNumber
                        : "Unassigned",
                    Status = r.Status
                })
                .ToList();

            // Recent orders.
            var recentOrders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .Include(o => o.Payments)
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.Id)
                .Take(5)
                .ToListAsync();

            RecentOrders = recentOrders
                .Select(o => new RecentOrder
                {
                    Id = o.Id,
                    CustomerName = o.Customer != null
                        ? o.Customer.FullName
                        : "Unknown customer",
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    ItemCount = o.OrderItems.Sum(i => i.Quantity),
                    PaymentStatus = o.Payments
                        .OrderByDescending(p => p.TransactionDate)
                        .ThenByDescending(p => p.Id)
                        .Select(p => p.Status)
                        .FirstOrDefault() ?? "Unpaid"
                })
                .ToList();
        }

        public class RecentReservation
        {
            public int Id { get; set; }
            public string CustomerName { get; set; } = string.Empty;
            public DateTime ReservationDate { get; set; }
            public TimeSpan StartTime { get; set; }
            public int PartySize { get; set; }
            public string TableNumber { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
        }

        public class RecentOrder
        {
            public int Id { get; set; }
            public string CustomerName { get; set; } = string.Empty;
            public DateTime OrderDate { get; set; }
            public string Status { get; set; } = string.Empty;
            public decimal TotalAmount { get; set; }
            public int ItemCount { get; set; }
            public string PaymentStatus { get; set; } = string.Empty;
        }
    }
}
