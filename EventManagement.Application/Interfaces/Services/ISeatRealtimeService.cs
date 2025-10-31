using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManagement.Application.Interfaces.Services
{
    public interface ISeatRealtimeService
    {
        // Notify that seats are soft-held for a limited time
        Task SeatsHeld(Guid eventId, IEnumerable<Guid> seatIds, DateTime expiresAtUtc);
        // Notify that seats become unavailable due to an order pending window
        Task SeatsUnavailable(Guid eventId, IEnumerable<Guid> seatIds, DateTime? pendingExpiresUtc);
        // Notify that seats are released back to available
        Task SeatsReleased(Guid eventId, IEnumerable<Guid> seatIds);
    }
}
