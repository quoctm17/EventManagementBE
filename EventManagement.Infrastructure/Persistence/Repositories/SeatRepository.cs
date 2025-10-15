using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Domain.Models;
using EventManagement.Infrastructure.Persistence;

namespace EventManagement.Infrastructure.Persistence.Repositories
{
    public class SeatRepository : RepositoryBase<Seat>, ISeatRepository
    {
        public SeatRepository(EventManagementDbContext context) : base(context)
        {
        }
    }
}
