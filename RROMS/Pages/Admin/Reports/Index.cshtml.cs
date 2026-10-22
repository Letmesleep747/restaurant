
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RROMS.Data;

namespace RROMS.Pages.Admin.Reports
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public int TotalReservations { get; private set; }
        public int PendingReservations { get; private set; }
        public int ConfirmedReservations { get; private set; }
        public int CancelledReservations { get; private set; }

        public int TotalOrders { get; private set; }
        public int CompletedOrders { get; private set; }

        public decimal TotalRevenue { get; private set; }

        public List<PopularMenuItem> PopularItems { get; private set; }
            = new();

        public string? ValidationMessage { get; private set; }

        public async Task OnGetAsync()
        {
            if (StartDate.HasValue &&
                EndDate.HasValue &&
                StartDate.Value.Date > EndDate.Value.Date)
            {
                ValidationMessage =
                    "Start date must be on or before the end date.";
                return;
            }

            var start = StartDate?.Date;
            var endExclusive = EndDate?.Date.AddDays(1);

            // Reservations within the selected period.
            var reservations = _context.Reservations
                .AsNoTracking()
                .AsQueryable();

            if (start.HasValue)
                reservations = reservations.Where(r =>
                    r.ReservationDate >= start.Value);

            if (endExclusive.HasValue)
                reservations = reservations.Where(r =>
                    r.ReservationDate < endExclusive.Value);

            TotalReservations = await reservations.CountAsync();

            PendingReservations = await reservations.CountAsync(r =>
                r.Status == "Pending");

            ConfirmedReservations = await reservations.CountAsync(r =>
                r.Status == "Confirmed");

            CancelledReservations = await reservations.CountAsync(r =>
                r.Status == "Cancelled");

            // Orders within the selected period.
            var orders = _context.Orders
                .AsNoTracking()
                .AsQueryable();

            if (start.HasValue)
                orders = orders.Where(o => o.OrderDate >= start.Value);

            if (endExclusive.HasValue)
                orders = orders.Where(o => o.OrderDate < endExclusive.Value);

            TotalOrders = await orders.CountAsync();

            CompletedOrders = await orders.CountAsync(o =>
                o.Status == "Completed");

            // Revenue counts successful payments, not unpaid orders.
            var payments = _context.Payments
                .AsNoTracking()
                .Where(p => p.Status == "Paid"
                    && p.TransactionDate.HasValue);

            if (start.HasValue)
                payments = payments.Where(p =>
                    p.TransactionDate >= start.Value);

            if (endExclusive.HasValue)
                payments = payments.Where(p =>
                    p.TransactionDate < endExclusive.Value);

            TotalRevenue = await payments
                .Select(p => (decimal?)p.Amount)
                .SumAsync() ?? 0m;

            // Rank menu items by quantity ordered.
            // Cancelled orders are excluded.
            var itemQuery =
                from item in _context.OrderItems.AsNoTracking()
                join order in _context.Orders.AsNoTracking()
                    on item.OrderId equals order.Id
                join menuItem in _context.MenuItems.AsNoTracking()
                    on item.MenuItemId equals menuItem.Id
                where order.Status != "Cancelled"
                select new
                {
                    menuItem.Id,
                    menuItem.Name,
                    item.Quantity,
                    order.OrderDate
                };

            if (start.HasValue)
                itemQuery = itemQuery.Where(i =>
                    i.OrderDate >= start.Value);

            if (endExclusive.HasValue)
                itemQuery = itemQuery.Where(i =>
                    i.OrderDate < endExclusive.Value);

            PopularItems = await itemQuery
                .GroupBy(i => new { i.Id, i.Name })
                .Select(g => new PopularMenuItem
                {
                    MenuItemId = g.Key.Id,
                    Name = g.Key.Name,
                    QuantityOrdered = g.Sum(i => i.Quantity)
                })
                .OrderByDescending(i => i.QuantityOrdered)
                .ThenBy(i => i.Name)
                .Take(5)
                .ToListAsync();
        }

        public class PopularMenuItem
        {
            public int MenuItemId { get; set; }
            public string Name { get; set; } = string.Empty;
            public int QuantityOrdered { get; set; }
        }
    }
}
