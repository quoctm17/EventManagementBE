using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Domain.Models;
using EventManagement.Infrastructure.Persistence;

namespace EventManagement.Infrastructure.Persistence.Repositories
{
    public class EventImageRepository : RepositoryBase<EventImage>, IEventImageRepository
    {
        public EventImageRepository(EventManagementDbContext context) : base(context)
        {
        }
    }
}
