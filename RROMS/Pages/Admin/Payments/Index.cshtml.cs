using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RROMS.Data;
using RROMS.Models;

namespace RROMS.Pages.Admin.Payments
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
        public DateTime? PaymentDate { get; set; }

        public int TotalPayments { get; private set; }
        public int PaidPayments { get; private set; }
        public int PendingPayments { get; private set; }
        public int FailedPayments { get; private set; }

        public decimal TotalPaidAmount { get; private set; }

        public IList<Payment> PaymentRecords { get; private set; }
            = new List<Payment>();

        public async Task OnGetAsync()
        {
            var allPayments = _context.Payments.AsNoTracking();

            // Summary statistics use all payment records.
            TotalPayments = await allPayments.CountAsync();

            PaidPayments = await allPayments.CountAsync(
                p => p.Status == "Paid");

            PendingPayments = await allPayments.CountAsync(
                p => p.Status == "Pending");

            FailedPayments = await allPayments.CountAsync(
                p => p.Status == "Failed");

            // Count only successful payments in the paid amount.
            TotalPaidAmount = await allPayments
                .Where(p => p.Status == "Paid")
                .Select(p => (decimal?)p.Amount)
                .SumAsync() ?? 0m;

            var query = _context.Payments
                .AsNoTracking()
                .Include(p => p.Order)
                    .ThenInclude(o => o!.Customer)
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var term = SearchTerm.Trim();

                // Accept both "1" and "#1" for Payment ID.
                var normalizedTerm = term.TrimStart('#').Trim();

                var hasPaymentId = int.TryParse(
                    normalizedTerm,
                    out var paymentId);

                query = query.Where(p =>
                    (hasPaymentId && p.Id == paymentId)
                    || (p.TransactionReference != null
                        && p.TransactionReference.Contains(term))
                    || (p.Order != null
                        && p.Order.CustomerId.Contains(term))
                    || (p.Order != null
                        && p.Order.Customer != null
                        && (
                            (p.Order.Customer.FullName != null
                                && p.Order.Customer.FullName.Contains(term))
                            || (p.Order.Customer.Email != null
                                && p.Order.Customer.Email.Contains(term))
                        ))
                );
            }

            if (PaymentDate.HasValue)
            {
                var startDate = PaymentDate.Value.Date;
                var endDate = startDate.AddDays(1);

                query = query.Where(p =>
                    p.TransactionDate.HasValue
                    && p.TransactionDate.Value >= startDate
                    && p.TransactionDate.Value < endDate);
            }

            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                query = query.Where(p => p.Status == StatusFilter);
            }

            PaymentRecords = await query
                .OrderByDescending(p => p.TransactionDate)
                .ThenByDescending(p => p.Id)
                .ToListAsync();
        }
    }
}