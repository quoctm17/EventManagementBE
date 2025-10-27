using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventManagement.Domain.Models;

namespace EventManagement.Application.Interfaces.Repositories
{
    public interface IEventRepository : IRepository<Event>
    {
        Task<IEnumerable<Event>> GetUpcomingPublishedAsync(int limit = 6);
        Task<IEnumerable<Venue>> GetPopularDestinationsAsync(int limit = 4);

        Task<(IEnumerable<Event> Items, int Total)> GetPagedEventsAsync(int page, int pageSize, string? search = null, string? province = null, IEnumerable<Guid>? categoryIds = null, DateTime? date = null, decimal? priceMin = null, decimal? priceMax = null, string? sortBy = null);
        Task<Event?> GetEventWithDetailsAsync(Guid eventId);
        Task<List<string>> GetAllProvincesAsync();
        Task<int> CountByOrganizerAsync(Guid organizerId);
    }
}
