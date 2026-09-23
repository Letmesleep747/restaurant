using restaurant.Models;
using restaurant.Models.Shared;

namespace restaurant.Services
{
    public interface IReservationAvailabilityService
    {
        // Returns the first free table that seats `partySize` or more and has
        // no overlapping Pending/Confirmed reservation for the requested slot.
        // Returns null if nothing is free.
        Task<DiningTable?> FindAvailableTableAsync(
            DateTime reservationDate, TimeSpan startTime, int durationMinutes,
            int partySize, int? excludeReservationId = null);
    }
}
