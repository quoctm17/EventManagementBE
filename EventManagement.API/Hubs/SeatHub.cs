using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace EventManagement.API.Hubs
{
    public class SeatHub : Hub
    {
        private static string Group(Guid eventId) => $"event:{eventId:N}";

        public Task JoinEvent(Guid eventId) =>
            Groups.AddToGroupAsync(Context.ConnectionId, Group(eventId));

        public Task LeaveEvent(Guid eventId) =>
            Groups.RemoveFromGroupAsync(Context.ConnectionId, Group(eventId));
    }
}
