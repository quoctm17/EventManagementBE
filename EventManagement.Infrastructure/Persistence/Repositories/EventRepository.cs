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
        public EventRepository(EventManagementDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Event>> GetUpcomingPublishedAsync(int limit = 6)
        {
            // Return published events ordered by EventDate then EventStartTime — do not exclude past dates so UI will always have items
            return await _context.Events
                .Include(e => e.EventImages)
                .Include(e => e.Venue)
                .Include(e => e.Categories)
                .Where(e => e.IsPublished == true)
                .OrderBy(e => e.EventDate)
                .ThenBy(e => e.EventStartTime)
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

        public async Task<(IEnumerable<Event> Items, int Total)> GetPagedEventsAsync(int page, int pageSize, string? search = null, string? province = null, IEnumerable<Guid>? categoryIds = null, DateTime? date = null, decimal? priceMin = null, decimal? priceMax = null, string? sortBy = null)
        {
            var query = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventImages)
                .Include(e => e.Categories)
                .AsQueryable()
                .Where(e => e.IsPublished == true);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(e => e.EventName.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(province))
            {
                var p = province.Trim().ToLower();
                query = query.Where(e => e.Venue != null && e.Venue.Province.ToLower() == p);
            }

            if (categoryIds != null && categoryIds.Any())
            {
                // AND semantics: event must have all requested category ids
                var idsList = categoryIds.ToList();
                var reqCount = idsList.Count;
                query = query.Where(e => e.Categories.Count(c => idsList.Contains(c.CategoryId)) == reqCount);
            }

            if (date.HasValue)
            {
                var d = DateOnly.FromDateTime(date.Value);
                query = query.Where(e => e.EventDate == d);
            }

            // Filter by price: find events that have at least one seat mapping within the range
            if (priceMin.HasValue || priceMax.HasValue)
            {
                var pm = priceMin ?? decimal.MinValue;
                var pM = priceMax ?? decimal.MaxValue;
                var eventIds = await _context.EventSeatMappings
                    .Where(m => m.IsAvailable != false && m.Price >= pm && m.Price <= pM)
                    .Select(m => m.EventId)
                    .Distinct()
                    .ToListAsync();

                query = query.Where(e => eventIds.Contains(e.EventId));
            }

            // Sorting (simple)
            query = sortBy switch
            {
                "date_desc" => query.OrderByDescending(e => e.EventDate).ThenByDescending(e => e.EventStartTime),
                "price_asc" => query.OrderBy(e => e.EventSeatMappings.Min(m => m.Price)),
                "price_desc" => query.OrderByDescending(e => e.EventSeatMappings.Min(m => m.Price)),
                _ => query.OrderBy(e => e.EventDate).ThenBy(e => e.EventStartTime),
            };

            var total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (items, total);
        }

        public async Task<Event?> GetEventWithDetailsAsync(Guid eventId)
        {
            return await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventImages)
                .Include(e => e.Categories)
                .Include(e => e.EventSeatMappings)
                .Include(e => e.Organizer)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EventId == eventId && e.IsPublished == true);
        }

        public async Task<List<string>> GetAllProvincesAsync()
        {
            return await _context.Venues
                .Where(v => !string.IsNullOrEmpty(v.Province))
                .Select(v => v.Province.Trim())
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();
        }

        public async Task<int> CountByOrganizerAsync(Guid organizerId)
        {
            return await _context.Events
                .Where(e => e.OrganizerId == organizerId)
                .CountAsync();
        }
    }
}
