using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RROMS.Data;
using RROMS.Models;

namespace RROMS.Pages.Staff
{
    public class OrderModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public OrderModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
        }

        // Treat Customer module's "Placed" status as "Pending"
        // for Staff-side order management.
        private static bool IsPendingStatus(string? status)
        {
            return string.Equals(
                       status,
                       "Pending",
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   string.Equals(
                       status,
                       "Placed",
                       StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsTerminalStatus(string? status)
        {
            return string.Equals(
                       status,
                       "Completed",
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   string.Equals(
                       status,
                       "Cancelled",
                       StringComparison.OrdinalIgnoreCase);
        }

        // GET: /Staff/Order?handler=MenuItems
        public async Task<IActionResult> OnGetMenuItemsAsync()
        {
            var menuItems = await _context.MenuItems
                .AsNoTracking()
                .Where(m => m.IsAvailable)
                .OrderBy(m => m.Name)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    price = m.Price
                })
                .ToListAsync();

            return new JsonResult(new
            {
                success = true,
                menuItems
            });
        }

        // GET: /Staff/Order?handler=List
        public async Task<IActionResult> OnGetListAsync()
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Reservation)
                    .ThenInclude(r => r!.RestaurantTable)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.MenuItem)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var result = orders.Select(o => new
            {
                id = o.Id,
                customer = o.Customer?.FullName ?? "Unknown Customer",
                reservationId = o.ReservationId,
                table = o.ReservationId == null
                    ? "Walk-in"
                    : o.Reservation?.RestaurantTable?.TableNumber
                        ?? "Not Assigned",

                // Staff sees "Placed" as "Pending".
                // The actual database value remains unchanged.
                status = IsPendingStatus(o.Status)
                    ? "Pending"
                    : o.Status,

                orderDate = o.OrderDate,
                total = o.TotalAmount,
                items = o.OrderItems.Select(i => new
                {
                    name = i.MenuItem?.Name ?? "Unknown Item",
                    quantity = i.Quantity,
                    price = i.UnitPrice
                }).ToList()
            }).ToList();

            return new JsonResult(new
            {
                success = true,
                orders = result
            });
        }

        // GET: /Staff/Order?handler=Reservations
        public async Task<IActionResult> OnGetReservationsAsync()
        {
            var records = await _context.Reservations
                .AsNoTracking()
                .Include(r => r.Customer)
                .Include(r => r.RestaurantTable)
                .Where(r =>
                    r.ReservationDate >= DateTime.Today &&
                    r.Status != "Cancelled")
                .OrderBy(r => r.ReservationDate)
                .ThenBy(r => r.StartTime)
                .ToListAsync();

            var reservations = records.Select(r => new
            {
                id = r.Id,
                customer = r.Customer?.FullName ?? "Unknown Customer",
                table = r.RestaurantTable?.TableNumber ?? "Not Assigned",
                date = r.ReservationDate,
                time = r.StartTime.ToString(@"hh\:mm"),
                status = r.Status
            }).ToList();

            return new JsonResult(new
            {
                success = true,
                reservations
            });
        }

        // POST: /Staff/Order?handler=Save
        public async Task<IActionResult> OnPostSaveAsync(
            [FromBody] OrderRequest request)
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid order information."
                });
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Please add at least one food item."
                });
            }

            // Staff-created and Staff-edited orders use Pending.
            // Customer-created Placed orders are treated as Pending
            // when Staff edits them.
            const string status = "Pending";

            // Find reservation, if this is a reservation order.
            Reservation? reservation = null;

            if (request.ReservationId.HasValue)
            {
                reservation = await _context.Reservations
                    .Include(r => r.Customer)
                    .FirstOrDefaultAsync(r =>
                        r.Id == request.ReservationId.Value);

                if (reservation == null ||
                    reservation.Status == "Cancelled")
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "The selected reservation does not exist or has been cancelled."
                    });
                }
            }

            // Resolve CustomerId.
            string customerId;

            if (reservation != null)
            {
                customerId = reservation.CustomerId;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.CustomerName))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Please enter a registered customer's name."
                    });
                }

                var matchingCustomers = await _context.Users
                    .Where(u =>
                        u.IsActive &&
                        u.FullName == request.CustomerName.Trim())
                    .Take(2)
                    .ToListAsync();

                if (matchingCustomers.Count == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No registered customer was found with this name. Please select an existing customer."
                    });
                }

                if (matchingCustomers.Count > 1)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "More than one customer has this name. Please use a customer selection that identifies the correct account."
                    });
                }

                customerId = matchingCustomers[0].Id;
            }

            // Retrieve actual menu items from the database.
            var requestedNames = request.Items
                .Where(i => !string.IsNullOrWhiteSpace(i.Name))
                .Select(i => i.Name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var menuRecords = await _context.MenuItems
                .AsNoTracking()
                .Where(m =>
                    requestedNames.Contains(m.Name) &&
                    m.IsAvailable)
                .ToListAsync();

            var menuLookup = menuRecords
                .GroupBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.First(),
                    StringComparer.OrdinalIgnoreCase);

            var validatedItems = new List<OrderItem>();
            decimal totalAmount = 0;

            foreach (var item in request.Items)
            {
                if (item.Quantity < 1 ||
                    item.Quantity > 100 ||
                    string.IsNullOrWhiteSpace(item.Name))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "One or more order items are invalid."
                    });
                }

                var itemName = item.Name.Trim();

                if (!menuLookup.TryGetValue(itemName, out var menuItem))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"The menu item '{itemName}' does not exist or is currently unavailable."
                    });
                }

                validatedItems.Add(new OrderItem
                {
                    MenuItemId = menuItem.Id,
                    Quantity = item.Quantity,
                    UnitPrice = menuItem.Price
                });

                totalAmount += menuItem.Price * item.Quantity;
            }

            Order? order = null;
            bool isNewOrder = !request.OrderId.HasValue;

            if (!isNewOrder)
            {
                order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o =>
                        o.Id == request.OrderId!.Value);

                if (order == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Order not found."
                    });
                }

                // Accept both Pending and Placed for Staff editing.
                if (!IsPendingStatus(order.Status))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Only pending or placed orders can be edited."
                    });
                }

                var existingPayment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.OrderId == order.Id);

                if (existingPayment?.Status == "Paid")
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "This order has already been paid and cannot be edited."
                    });
                }
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                if (isNewOrder)
                {
                    order = new Order
                    {
                        CustomerId = customerId,
                        OrderDate = DateTime.Now
                    };

                    _context.Orders.Add(order);
                }
                else
                {
                    _context.OrderItems.RemoveRange(order!.OrderItems);
                    order.OrderItems.Clear();

                    order.CustomerId = customerId;
                }

                order!.ReservationId = request.ReservationId;
                order.Status = status;
                order.TotalAmount = totalAmount;

                foreach (var item in validatedItems)
                {
                    order.OrderItems.Add(item);
                }

                await _context.SaveChangesAsync();

                // Create or update the related payment record.
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.OrderId == order.Id);

                if (payment == null)
                {
                    payment = new Payment
                    {
                        OrderId = order.Id,
                        Amount = totalAmount,
                        Method = "Pending",
                        Status = "Unpaid",
                        TransactionDate = null
                    };

                    _context.Payments.Add(payment);
                }
                else
                {
                    payment.Amount = totalAmount;
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new JsonResult(new
                {
                    success = true,
                    message = isNewOrder
                        ? "Order created successfully."
                        : "Order updated successfully.",
                    orderId = order.Id
                });
            }
            catch
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    success = false,
                    message = "Unable to save the order. Please check the database and try again."
                });
            }
        }

        // POST: /Staff/Order?handler=UpdateStatus
        public async Task<IActionResult> OnPostUpdateStatusAsync(
            [FromBody] StatusRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid request."
                });
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == request.OrderId);

            if (order == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Order not found."
                });
            }

            if (IsTerminalStatus(order.Status))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "This order can no longer be changed."
                });
            }

            bool isValidNextStatus =
                IsPendingStatus(order.Status) &&
                string.Equals(
                    request.Status,
                    "Served",
                    StringComparison.OrdinalIgnoreCase);

            bool isCancellation =
                string.Equals(
                    request.Status,
                    "Cancelled",
                    StringComparison.OrdinalIgnoreCase);

            if (!isValidNextStatus && !isCancellation)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid order status transition."
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                order.Status = isCancellation
                    ? "Cancelled"
                    : "Served";

                // Cancel the associated unpaid payment when
                // the order is cancelled.
                if (isCancellation)
                {
                    var payment = await _context.Payments
                        .FirstOrDefaultAsync(p => p.OrderId == order.Id);

                    if (payment != null)
                    {
                        if (payment.Status == "Paid")
                        {
                            await transaction.RollbackAsync();

                            return BadRequest(new
                            {
                                success = false,
                                message = "A paid order cannot be cancelled."
                            });
                        }

                        payment.Status = "Cancelled";
                    }
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new JsonResult(new
                {
                    success = true,
                    message = isCancellation
                        ? "Order and associated payment cancelled successfully."
                        : "Order status updated successfully."
                });
            }
            catch
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    success = false,
                    message = "Unable to update the order status."
                });
            }
        }

        // Request models used by the frontend.
        public class OrderRequest
        {
            public int? OrderId { get; set; }

            public int? ReservationId { get; set; }

            public string CustomerName { get; set; } = "";

            public string Status { get; set; } = "Pending";

            public List<OrderItemRequest> Items { get; set; } = new();
        }

        public class OrderItemRequest
        {
            public string Name { get; set; } = "";

            public int Quantity { get; set; }
        }

        public class StatusRequest
        {
            public int OrderId { get; set; }

            public string Status { get; set; } = "";
        }
    }
}