using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Domain.Models;
using EventManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Infrastructure.Persistence.Repositories
{
    public class EventRepository : RepositoryBase<Event>, IEventRepository
    {
        private readonly EventManagementDbContext _context;

        public EventRepository(EventManagementDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Event>> GetUpcomingPublishedAsync(int limit = 6)
        {
            // Return published events ordered by EventDate then StartTime — do not exclude past dates so UI will always have items
            return await _context.Events
                .Include(e => e.EventImages)
                .Include(e => e.Venue)
                .Include(e => e.Categories)
                .Where(e => e.IsPublished == true)
                .OrderBy(e => e.EventDate)
                .ThenBy(e => e.StartTime)
                .Take(limit)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Venue>> GetPopularDestinationsAsync(int limit = 4)
        {
            // Simple heuristic: destinations with most upcoming published events
            // Rank venues by total number of published events (no time filter) so destinations are always returned
            var query = await _context.Events
                .Where(e => e.IsPublished == true)
                .GroupBy(e => e.VenueId)
                .Select(g => new { VenueId = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(limit)
                .ToListAsync();

            var venueIds = query.Select(q => q.VenueId).ToList();

            return await _context.Venues
                .Include(v => v.VenueImages)
                .Where(v => venueIds.Contains(v.VenueId))
                .AsNoTracking()
                .ToListAsync();
        }

        // Return minimum price per event for the provided event ids
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
