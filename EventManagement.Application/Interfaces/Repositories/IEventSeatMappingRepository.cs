using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventManagement.Domain.Models;

namespace EventManagement.Application.Interfaces.Repositories
{
    public interface IEventSeatMappingRepository : IRepository<EventSeatMapping>
    {
        Task<Dictionary<Guid, decimal?>> GetStartingPricesAsync(IEnumerable<Guid> eventIds);
    }
}
