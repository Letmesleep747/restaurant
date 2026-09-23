using Microsoft.EntityFrameworkCore;
using restaurant.Data;
using restaurant.Models;
using restaurant.Models.Enums;
using restaurant.Models.Shared;

namespace restaurant.Services
{
    public class ReservationAvailabilityService : IReservationAvailabilityService
    {
        private readonly ApplicationDbContext _db;

        public ReservationAvailabilityService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<DiningTable?> FindAvailableTableAsync(
            DateTime reservationDate, TimeSpan startTime, int durationMinutes,
            int partySize, int? excludeReservationId = null)
        {
            var requestedStart = reservationDate.Date + startTime;
            var requestedEnd = requestedStart.AddMinutes(durationMinutes);

            var candidateTables = await _db.DiningTables
                .Where(t => t.IsActive && t.Capacity >= partySize)
                .OrderBy(t => t.Capacity) // smallest table that still fits the party
                .ToListAsync();

            if (candidateTables.Count == 0)
                return null;

            // Only active (not cancelled) reservations can conflict.
            var activeStatuses = new[] { ReservationStatus.Pending, ReservationStatus.Confirmed };

            var sameDayReservations = await _db.Reservations
                .Where(r => r.ReservationDate.Date == reservationDate.Date
                            && activeStatuses.Contains(r.Status)
                            && (excludeReservationId == null || r.Id != excludeReservationId))
                .ToListAsync();

            foreach (var table in candidateTables)
            {
                var overlaps = sameDayReservations
                    .Where(r => r.TableId == table.Id)
                    .Any(r =>
                    {
                        var existingStart = r.ReservationDate.Date + r.StartTime;
                        var existingEnd = existingStart.AddMinutes(r.DurationMinutes);
                        // Two ranges overlap unless one ends before the other starts.
                        return requestedStart < existingEnd && existingStart < requestedEnd;
                    });

                if (!overlaps)
                    return table;
            }

            return null; // every suitable table is booked for that slot
        }
    }
}
