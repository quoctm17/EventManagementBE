using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Domain.Models;
using EventManagement.Infrastructure.Persistence;

namespace EventManagement.Infrastructure.Persistence.Repositories
{
    public class EventSeatMappingRepository : RepositoryBase<EventSeatMapping>, IEventSeatMappingRepository
    {
        public EventSeatMappingRepository(EventManagementDbContext context) : base(context)
        {
        }
    }
}
