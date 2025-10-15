using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Domain.Models;
using EventManagement.Infrastructure.Persistence;

namespace EventManagement.Infrastructure.Persistence.Repositories
{
    public class ContractRepository : RepositoryBase<Contract>, IContractRepository
    {
        public ContractRepository(EventManagementDbContext context) : base(context)
        {
        }
    }
}
