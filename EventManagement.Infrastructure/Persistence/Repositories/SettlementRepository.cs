using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Domain.Models;
using EventManagement.Infrastructure.Persistence;

namespace EventManagement.Infrastructure.Persistence.Repositories
{
    public class SettlementRepository : RepositoryBase<Settlement>, ISettlementRepository
    {
        public SettlementRepository(EventManagementDbContext context) : base(context)
        {
        }
    }
}
