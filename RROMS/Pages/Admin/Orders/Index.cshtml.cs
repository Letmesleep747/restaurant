using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RROMS.Data;
using RROMS.Models;

namespace RROMS.Pages.Admin.Orders
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public DateTime? OrderDate { get; set; }

        public int TotalOrders { get; private set; }
        public int PlacedOrders { get; private set; }
        public int CancelledOrders { get; private set; }
        public decimal TotalOrderValue { get; private set; }

        public IList<Order> OrderRecords { get; private set; }
            = new List<Order>();

        public async Task OnGetAsync()
        {
            var allOrders = _context.Orders.AsNoTracking();

            TotalOrders = await allOrders.CountAsync();

            PlacedOrders = await allOrders.CountAsync(
                o => o.Status == "Placed");

            CancelledOrders = await allOrders.CountAsync(
                o => o.Status == "Cancelled");

            // Sum actual recorded order totals, not payment totals.
            TotalOrderValue = await allOrders
                .Select(o => (decimal?)o.TotalAmount)
                .SumAsync() ?? 0m;

            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Reservation)
                .Include(o => o.OrderItems)
                .Include(o => o.Payments)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var term = SearchTerm.Trim();

                // Accept both "1" and "#1".
                var normalizedTerm = term.TrimStart('#').Trim();

                var hasOrderId = int.TryParse(
                    normalizedTerm,
                    out var orderId);

                query = query.Where(o =>
                    (hasOrderId && o.Id == orderId)
                    || o.CustomerId.Contains(term)
                    || (o.Customer != null &&
                        (
                            o.Customer.FullName.Contains(term)
                            || (o.Customer.Email != null &&
                                o.Customer.Email.Contains(term))
                        )));
            }

            if (OrderDate.HasValue)
            {
                var startDate = OrderDate.Value.Date;
                var endDate = startDate.AddDays(1);

                query = query.Where(o =>
                    o.OrderDate >= startDate &&
                    o.OrderDate < endDate);
            }

            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                query = query.Where(o => o.Status == StatusFilter);
            }

            OrderRecords = await query
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.Id)
                .ToListAsync();
        }
    }
}