using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Domain.Models;
using EventManagement.Infrastructure.Persistence;

namespace EventManagement.Infrastructure.Persistence.Repositories
{
    public class OrganizerRequestRepository : RepositoryBase<OrganizerRequest>, IOrganizerRequestRepository
    {
        public OrganizerRequestRepository(EventManagementDbContext context) : base(context)
        {
        }
    }
}
