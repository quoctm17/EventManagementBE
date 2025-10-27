using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventManagement.Domain.Models;

namespace EventManagement.Application.Interfaces.Repositories
{
    public interface ISeatHoldRepository : IRepository<SeatHold>
    {
        Task<HashSet<Guid>> GetActiveHeldSeatIdsAsync(Guid eventId, DateTime nowUtc);
    }
}
