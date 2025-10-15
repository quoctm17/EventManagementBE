using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Domain.Models;
using EventManagement.Infrastructure.Persistence;

namespace EventManagement.Infrastructure.Persistence.Repositories
{
    public class CheckinRepository : RepositoryBase<Checkin>, ICheckinRepository
    {
        public CheckinRepository(EventManagementDbContext context) : base(context)
        {
        }
    }
}
