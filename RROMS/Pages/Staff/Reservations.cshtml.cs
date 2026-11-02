
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RROMS.Data;
using RROMS.Models;

namespace RROMS.Pages.Staff
{
    public class ReservationsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservationsModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public void OnGet()
        {
        }

        // GET: /Staff/Reservations?handler=List
        // Loads reservations with their customer and table information.

        public async Task<IActionResult> OnGetListAsync()
        {
            var rows = await _context.Reservations
                .AsNoTracking()
                .Include(r => r.Customer)
                .Include(r => r.RestaurantTable)
                .OrderByDescending(r => r.ReservationDate)
                .ThenBy(r => r.StartTime)
                .ToListAsync();

            var reservations = rows.Select(r => new
            {
                id = r.Id,
                customerId = r.CustomerId,
                customer = r.Customer?.FullName ?? "Unknown customer",
                phone = r.Customer?.PhoneNumber ?? "",
                date = r.ReservationDate.ToString("yyyy-MM-dd"),
                time = r.StartTime.ToString(@"hh\:mm"),
                guests = r.PartySize,
                tableId = r.TableId,
                table = r.RestaurantTable?.TableNumber ?? "Not assigned",
                status = r.Status,
                durationMinutes = r.DurationMinutes,
                specialRequests = r.SpecialRequests
            }).ToList();

            return new JsonResult(new
            {
                success = true,
                data = reservations
            });
        }


        // GET: /Staff/Reservations?handler=Options
        // Only users with the Customer role appear in the customer dropdown.
        public async Task<IActionResult> OnGetOptionsAsync()
        {
            var customers = await _userManager.GetUsersInRoleAsync("Customer");

            var customerOptions = customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.FullName)
                .Select(c => new
                {
                    id = c.Id,
                    name = c.FullName,
                    phone = c.PhoneNumber ?? ""
                })
                .ToList();

            var tableOptions = await _context.RestaurantTables
                .AsNoTracking()
                .Where(t => t.IsActive)
                .OrderBy(t => t.TableNumber)
                .Select(t => new
                {
                    id = t.RestaurantTableId,
                    tableNumber = t.TableNumber,
                    capacity = t.Capacity
                })
                .ToListAsync();

            return new JsonResult(new
            {
                success = true,
                customers = customerOptions,
                tables = tableOptions
            });
        }

        // POST: /Staff/Reservations?handler=Add
        public async Task<IActionResult> OnPostAddAsync(
            [FromBody] ReservationRequest request)
        {
            if (!IsValidRequest(request))
            {
                return BadRequestResult("Please provide valid reservation details.");
            }

            var customer = await FindActiveCustomerAsync(request.CustomerId);

            if (customer == null)
            {
                return BadRequestResult("The selected customer account was not found or is inactive.");
            }

            if (request.TableId.HasValue)
            {
                var table = await _context.RestaurantTables
                    .FirstOrDefaultAsync(t =>
                        t.RestaurantTableId == request.TableId.Value &&
                        t.IsActive);

                if (table == null)
                {
                    return BadRequestResult("The selected table is unavailable.");
                }

                if (table.Capacity < request.PartySize)
                {
                    return BadRequestResult("The selected table does not have enough capacity.");
                }
            }

            var reservation = new Reservation
            {
                CustomerId = customer.Id,
                TableId = request.TableId,
                ReservationDate = request.ReservationDate.Date,
                StartTime = request.StartTime,
                DurationMinutes = request.DurationMinutes > 0
                    ? request.DurationMinutes
                    : 120,
                PartySize = request.PartySize,
                Status = NormalizeStatus(request.Status),
                SpecialRequests = request.SpecialRequests,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true,
                message = "Reservation added successfully.",
                id = reservation.Id
            });
        }

        // POST: /Staff/Reservations?handler=Update
        public async Task<IActionResult> OnPostUpdateAsync(
            [FromBody] ReservationRequest request)
        {
            if (request.Id <= 0 || !IsValidRequest(request))
            {
                return BadRequestResult("Please provide valid reservation details.");
            }

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == request.Id);

            if (reservation == null)
            {
                return NotFoundResult("Reservation not found.");
            }

            var customer = await FindActiveCustomerAsync(request.CustomerId);

            if (customer == null)
            {
                return BadRequestResult("The selected customer account was not found or is inactive.");
            }

            if (request.TableId.HasValue)
            {
                var table = await _context.RestaurantTables
                    .FirstOrDefaultAsync(t =>
                        t.RestaurantTableId == request.TableId.Value &&
                        t.IsActive);

                if (table == null)
                {
                    return BadRequestResult("The selected table is unavailable.");
                }

                if (table.Capacity < request.PartySize)
                {
                    return BadRequestResult("The selected table does not have enough capacity.");
                }
            }

            reservation.CustomerId = customer.Id;
            reservation.TableId = request.TableId;
            reservation.ReservationDate = request.ReservationDate.Date;
            reservation.StartTime = request.StartTime;
            reservation.DurationMinutes = request.DurationMinutes > 0
                ? request.DurationMinutes
                : 120;
            reservation.PartySize = request.PartySize;
            reservation.Status = NormalizeStatus(request.Status);
            reservation.SpecialRequests = request.SpecialRequests;
            reservation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true,
                message = "Reservation updated successfully."
            });
        }

        // POST: /Staff/Reservations?handler=UpdateStatus
        public async Task<IActionResult> OnPostUpdateStatusAsync(
            [FromBody] UpdateReservationStatusRequest request)
        {
            if (request.Id <= 0 || !IsAllowedStatus(request.Status))
            {
                return BadRequestResult("Invalid reservation status.");
            }

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == request.Id);

            if (reservation == null)
            {
                return NotFoundResult("Reservation not found.");
            }

            reservation.Status = NormalizeStatus(request.Status);
            reservation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true,
                message = "Reservation status updated successfully."
            });
        }

        private async Task<ApplicationUser?> FindActiveCustomerAsync(
            string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                return null;
            }

            var customer = await _userManager.FindByIdAsync(customerId);

            if (customer == null || !customer.IsActive)
            {
                return null;
            }

            var isCustomer = await _userManager.IsInRoleAsync(
                customer,
                "Customer");

            return isCustomer ? customer : null;
        }

        private static bool IsValidRequest(ReservationRequest request)
        {
            return request != null
                && !string.IsNullOrWhiteSpace(request.CustomerId)
                && request.ReservationDate != default
                && request.PartySize >= 1
                && request.PartySize <= 20
                && request.StartTime >= TimeSpan.Zero
                && request.StartTime < TimeSpan.FromDays(1)
                && IsAllowedStatus(request.Status);
        }

        private static bool IsAllowedStatus(string? status)
        {
            return status is "Pending" or "Confirmed" or "Cancelled";
        }

        private static string NormalizeStatus(string? status)
        {
            return IsAllowedStatus(status) ? status! : "Pending";
        }

        private BadRequestObjectResult BadRequestResult(string message)
        {
            return BadRequest(new
            {
                success = false,
                message
            });
        }

        private NotFoundObjectResult NotFoundResult(string message)
        {
            return NotFound(new
            {
                success = false,
                message
            });
        }

        public class ReservationRequest
        {
            public int Id { get; set; }

            public string CustomerId { get; set; } = string.Empty;

            public int? TableId { get; set; }

            public DateTime ReservationDate { get; set; }

            public TimeSpan StartTime { get; set; }

            public int PartySize { get; set; }

            public int DurationMinutes { get; set; } = 120;

            public string Status { get; set; } = "Pending";

            public string? SpecialRequests { get; set; }
        }

        public class UpdateReservationStatusRequest
        {
            public int Id { get; set; }

            public string Status { get; set; } = string.Empty;
        }
    }
}
