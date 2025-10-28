using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventManagement.API.Hubs;
using EventManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;

namespace EventManagement.API.Realtime
{
    // SignalR-backed implementation of realtime seat notifications
    public class SeatRealtimeHubService : ISeatRealtimeService
    {
        private readonly IHubContext<SeatHub> _hub;
        public SeatRealtimeHubService(IHubContext<SeatHub> hub)
        {
            _hub = hub;
        }

        private static string Group(Guid eventId) => $"event:{eventId:N}";

        public Task SeatsHeld(Guid eventId, IEnumerable<Guid> seatIds, DateTime expiresAtUtc)
        {
            return _hub.Clients.Group(Group(eventId)).SendAsync("seatsHeld", new
            {
                eventId,
                seatIds,
                expiresAtUtc
            });
        }

        public Task SeatsUnavailable(Guid eventId, IEnumerable<Guid> seatIds, DateTime? pendingExpiresUtc)
        {
            return _hub.Clients.Group(Group(eventId)).SendAsync("seatsUnavailable", new
            {
                eventId,
                seatIds,
                pendingExpiresUtc
            });
        }

        public Task SeatsReleased(Guid eventId, IEnumerable<Guid> seatIds)
        {
            return _hub.Clients.Group(Group(eventId)).SendAsync("seatsReleased", new
            {
                eventId,
                seatIds
            });
        }
    }
}
