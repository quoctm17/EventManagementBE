using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Domain.Models;
using EventManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventManagement.Infrastructure.Persistence.Repositories
{
    public class EventSeatMappingRepository : RepositoryBase<EventSeatMapping>, IEventSeatMappingRepository
    {
        public EventSeatMappingRepository(EventManagementDbContext context) : base(context)
        {
        }

        public async Task<Dictionary<Guid, decimal?>> GetStartingPricesAsync(IEnumerable<Guid> eventIds)
        {
            var prices = await _context.EventSeatMappings
                .Where(m => eventIds.Contains(m.EventId) && m.IsAvailable != false)
                .GroupBy(m => m.EventId)
                .Select(g => new { EventId = g.Key, MinPrice = g.Min(x => (decimal?)x.Price) })
                .ToListAsync();

            return prices.ToDictionary(p => p.EventId, p => p.MinPrice);
        }
    }
}
