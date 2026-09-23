using Microsoft.AspNetCore.SignalR;
using restaurant.Data;
using restaurant.Hubs;
using restaurant.Models;
using restaurant.Models.Enums;

namespace restaurant.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationService(ApplicationDbContext db, IHubContext<NotificationHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        public async Task NotifyAsync(string userId, string title, string message,
            NotificationType type, string? linkUrl = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                LinkUrl = linkUrl,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();

            // Push live to the customer's browser if they're connected.
            // If they're offline, they'll still see it next time they open
            // /Customer/Notifications, since it's already saved above.
            await _hub.Clients.Group(userId).SendAsync("ReceiveNotification", new
            {
                notification.Id,
                notification.Title,
                notification.Message,
                Type = notification.Type.ToString(),
                notification.LinkUrl,
                CreatedAt = notification.CreatedAt.ToString("o")
            });
        }
    }
}
