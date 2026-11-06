using Microsoft.EntityFrameworkCore;
using restaurant.Data;
using restaurant.Models.Shared;

namespace restaurant.Services
{
    public class OperatingHoursService : IOperatingHoursService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<OperatingHoursService> _logger;
        private readonly TimeZoneInfo _timeZone;
        private readonly bool _blockOrdersWhenClosed;

        public OperatingHoursService(
            ApplicationDbContext db,
            IConfiguration configuration,
            ILogger<OperatingHoursService> logger)
        {
            _db = db;
            _logger = logger;
            _timeZone = ResolveTimeZone(configuration["Restaurant:TimeZoneId"], logger);
            _blockOrdersWhenClosed = configuration.GetValue("Restaurant:BlockOrdersWhenClosed", true);
        }

        // "Now" in the restaurant's own time zone. A hosted server (e.g. Azure) usually
        // runs in UTC, which would make "open now" wrong by several hours otherwise.
        private DateTime Now => TimeZoneInfo.ConvertTime(DateTime.UtcNow, _timeZone);

        public async Task<OperatingHoursSnapshot> GetSnapshotAsync()
        {
            var week = await LoadWeekAsync();
            var now = Now;
            return new OperatingHoursSnapshot(week, BuildStatus(week, now), now.DayOfWeek);
        }

        public async Task<string?> ValidateReservationSlotAsync(
            DateTime reservationDate, TimeSpan startTime, int durationMinutes)
        {
            var week = await LoadWeekAsync();
            var row = week.FirstOrDefault(h => h.Day == reservationDate.DayOfWeek);

            // Hours not configured for this day -> don't block the booking.
            if (row == null)
                return null;

            var day = reservationDate.DayOfWeek;

            if (row.IsClosed)
                return $"We are closed on {day}s. Please choose another day.";

            if (!IsOpenDay(row))
                return null; // not closed, but no times saved - nothing sensible to enforce

            var open = row.OpeningTime!.Value;
            var close = row.ClosingTime!.Value;
            var sitting = TimeSpan.FromMinutes(durationMinutes);

            if (startTime < open)
                return $"We open at {Fmt(open)} on {day}s. Please choose {Fmt(open)} or later.";

            // The whole sitting has to finish by closing time.
            if (startTime + sitting > close)
            {
                var lastStart = close - sitting;
                return lastStart >= open
                    ? $"Our last booking on {day}s is at {Fmt(lastStart)} (we close at {Fmt(close)})."
                    : $"Reservations must finish by our closing time of {Fmt(close)} on {day}s.";
            }

            return null;
        }

        public async Task<string?> GetOrderingBlockReasonAsync()
        {
            if (!_blockOrdersWhenClosed)
                return null;

            var week = await LoadWeekAsync();
            var status = BuildStatus(week, Now);

            if (!status.IsConfigured || status.IsOpen)
                return null;

            return string.IsNullOrEmpty(status.Detail)
                ? "We're closed right now, so new orders can't be placed."
                : $"We're closed right now, so new orders can't be placed. {status.Detail}.";
        }

        // ---------------------------------------------------------------- helpers

        private async Task<List<OperatingHour>> LoadWeekAsync()
        {
            try
            {
                var rows = await _db.OperatingHours.AsNoTracking().ToListAsync();

                // Monday -> Sunday, same order the Admin page uses.
                return rows
                    .OrderBy(o => o.Day == DayOfWeek.Sunday ? 7 : (int)o.Day)
                    .ToList();
            }
            catch (Exception ex)
            {
                // Don't break the customer site if the OperatingHours table
                // doesn't exist yet in this database.
                _logger.LogWarning(ex, "Could not load operating hours.");
                return new List<OperatingHour>();
            }
        }

        private static OpenStatus BuildStatus(List<OperatingHour> week, DateTime now)
        {
            if (week.Count == 0)
                return new OpenStatus(false, false, string.Empty, null);

            var today = week.FirstOrDefault(h => h.Day == now.DayOfWeek);

            if (today != null && IsOpenDay(today))
            {
                var open = today.OpeningTime!.Value;
                var close = today.ClosingTime!.Value;
                var time = now.TimeOfDay;

                if (time >= open && time < close)
                    return new OpenStatus(true, true, "Open now", $"Closes at {Fmt(close)}");

                if (time < open)
                    return new OpenStatus(true, false, "Closed now", $"Opens today at {Fmt(open)}");
            }

            var next = DescribeNextOpening(week, now.DayOfWeek);
            return new OpenStatus(true, false, "Closed now", next);
        }

        private static string? DescribeNextOpening(List<OperatingHour> week, DayOfWeek from)
        {
            for (var i = 1; i <= 7; i++)
            {
                var day = (DayOfWeek)(((int)from + i) % 7);
                var row = week.FirstOrDefault(h => h.Day == day);
                if (row == null || !IsOpenDay(row))
                    continue;

                var at = Fmt(row.OpeningTime!.Value);
                return i switch
                {
                    1 => $"Opens tomorrow at {at}",
                    7 => $"Opens next {day} at {at}",
                    _ => $"Opens {day} at {at}"
                };
            }

            return null;
        }

        private static bool IsOpenDay(OperatingHour h) =>
            !h.IsClosed && h.OpeningTime.HasValue && h.ClosingTime.HasValue;

        // Same 24-hour hh:mm format the rest of the customer site uses.
        private static string Fmt(TimeSpan t) => t.ToString(@"hh\:mm");

        private static TimeZoneInfo ResolveTimeZone(string? id, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(id))
                return TimeZoneInfo.Local;

            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (Exception ex) when (ex is TimeZoneNotFoundException || ex is InvalidTimeZoneException)
            {
                logger.LogWarning(ex,
                    "Time zone '{TimeZoneId}' was not found; using the server's local time zone.", id);
                return TimeZoneInfo.Local;
            }
        }
    }
}
