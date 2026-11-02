using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RROMS.Data;
using RROMS.Models;

namespace RROMS.Pages.Staff
{
    public class PaymentModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PaymentModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
        }

        // GET: /Staff/Payment?handler=List
        public async Task<IActionResult> OnGetListAsync()
        {
            var payments = await _context.Payments
                .AsNoTracking()
                .Include(p => p.Order)
                    .ThenInclude(o => o!.Customer)
                .Include(p => p.Order)
                    .ThenInclude(o => o!.Reservation)
                        .ThenInclude(r => r!.RestaurantTable)
                .Include(p => p.Order)
                    .ThenInclude(o => o!.OrderItems)
                        .ThenInclude(i => i.MenuItem)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            var result = payments.Select(p => new
            {
                id = p.Id,
                orderId = p.OrderId,

                customer = p.Order?.Customer?.FullName
                    ?? "Unknown Customer",

                table = p.Order?.ReservationId == null
                    ? "Walk-in"
                    : p.Order.Reservation?.RestaurantTable?.TableNumber
                        ?? "Not Assigned",

                amount = p.Amount,
                status = p.Status,

                method = p.Status == "Paid"
                    ? p.Method
                    : "-",

                transactionDate = p.TransactionDate,

                items = p.Order?.OrderItems.Select(i => new
                {
                    name = i.MenuItem?.Name ?? "Unknown Item",
                    quantity = i.Quantity,
                    price = i.UnitPrice
                }).Cast<object>().ToList() ?? new List<object>()
            }).ToList();

            return new JsonResult(new
            {
                success = true,
                payments = result
            });
        }

        // POST: /Staff/Payment?handler=RecordPayment
        public async Task<IActionResult> OnPostRecordPaymentAsync(
            [FromBody] PaymentRequest request)
        {
            if (request == null || request.PaymentId <= 0)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Invalid payment information."
                });
            }

            var allowedMethods = new[]
            {
                "Cash",
                "Card",
                "E-Wallet"
            };

            if (string.IsNullOrWhiteSpace(request.Method) ||
                !allowedMethods.Contains(request.Method.Trim()))
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Please select a valid payment method."
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var payment = await _context.Payments
                    .Include(p => p.Order)
                    .FirstOrDefaultAsync(p => p.Id == request.PaymentId);

                if (payment == null)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "Payment record was not found."
                    });
                }

                if (payment.Status == "Paid")
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "This bill has already been paid."
                    });
                }

                if (payment.Order == null)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "The related order could not be found."
                    });
                }

                // Update payment information.
                payment.Method = request.Method.Trim();
                payment.Status = "Paid";
                payment.TransactionDate = DateTime.Now;

                // Automatically complete the order after successful payment.
                payment.Order.Status = "Completed";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new JsonResult(new
                {
                    success = true,
                    message = "Payment recorded successfully. The order is now completed.",
                    paymentId = payment.Id,
                    orderId = payment.OrderId,
                    paymentStatus = payment.Status,
                    orderStatus = payment.Order.Status
                });
            }
            catch
            {
                await transaction.RollbackAsync();

                return new JsonResult(new
                {
                    success = false,
                    message = "Unable to record payment. Please try again."
                });
            }
        }

        public class PaymentRequest
        {
            public int PaymentId { get; set; }

            public string Method { get; set; } = "";
        }
    }
}