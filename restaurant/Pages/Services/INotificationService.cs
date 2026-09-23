using restaurant.Models.Enums;

namespace restaurant.Services
{
    public interface INotificationService
    {
        // Saves a notification to the database AND pushes it live to the
        // user's browser (if connected) via SignalR.
        Task NotifyAsync(string userId, string title, string message,
            NotificationType type, string? linkUrl = null);
    }
}
