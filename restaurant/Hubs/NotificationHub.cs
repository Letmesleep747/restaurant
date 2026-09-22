using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace restaurant.Hubs
{
    // Every signed-in user automatically joins a group named after their own 
    // user Id, so we can push a notification to exactly one person.
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnConnectedAsync();
        }
    }
}
