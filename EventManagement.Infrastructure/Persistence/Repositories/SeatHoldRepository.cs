using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Infrastructure.Persistence.Repositories
{
    public class SeatHoldRepository : RepositoryBase<SeatHold>, ISeatHoldRepository
    {
        public SeatHoldRepository(EventManagementDbContext context) : base(context)
        {
        }

        public async Task<HashSet<Guid>> GetActiveHeldSeatIdsAsync(Guid eventId, DateTime nowUtc)
        {
            var heldSeatIds = await _context.SeatHolds
                .AsNoTracking()
                .Where(h => h.EventId == eventId && h.HoldExpiresAt > nowUtc)
                .Select(h => h.SeatId)
                .Distinct()
                .ToListAsync();

            return heldSeatIds.ToHashSet();
        }
    }
}
