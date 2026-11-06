using restaurant.Models.Shared;

namespace restaurant.Services
{
    // Whether the restaurant is open right now, in a form that is easy to show on a page.
    // IsConfigured is false when the admin has not set up any hours (or the table is
    // unavailable) - in that case nothing should be blocked and nothing should be shown.
    public sealed record OpenStatus(bool IsConfigured, bool IsOpen, string Headline, string? Detail)
    {
        public string Text => string.IsNullOrEmpty(Detail) ? Headline : $"{Headline} \u00B7 {Detail}";
    }

    // The weekly schedule (Monday -> Sunday) plus the current open/closed state.
    public sealed record OperatingHoursSnapshot(
        IReadOnlyList<OperatingHour> Week,
        OpenStatus Status,
        DayOfWeek Today);

    public interface IOperatingHoursService
    {
        // One query: the weekly schedule and whether we are open right now.
        Task<OperatingHoursSnapshot> GetSnapshotAsync();

        // Returns null when the requested sitting fits inside opening hours,
        // otherwise a customer-friendly message explaining why it does not.
        Task<string?> ValidateReservationSlotAsync(
            DateTime reservationDate, TimeSpan startTime, int durationMinutes);

        // Returns null when new orders can be placed right now,
        // otherwise a customer-friendly message explaining why not.
        Task<string?> GetOrderingBlockReasonAsync();
    }
}
