using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Domain.Models;
using EventManagement.Infrastructure.Persistence;

namespace EventManagement.Infrastructure.Persistence.Repositories
{
    public class VenueRepository : RepositoryBase<Venue>, IVenueRepository
    {
        public VenueRepository(EventManagementDbContext context) : base(context)
        {
        }
    }
}
