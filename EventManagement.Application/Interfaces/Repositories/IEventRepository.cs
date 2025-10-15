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
        Task<Dictionary<Guid, decimal?>> GetStartingPricesAsync(IEnumerable<Guid> eventIds);
    }
}
