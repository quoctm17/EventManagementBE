using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Domain.Models;
using EventManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Infrastructure.Persistence.Repositories
{
    public class OrderRepository : RepositoryBase<Order>, IOrderRepository
    {
        public OrderRepository(EventManagementDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Order>> GetOrdersWithDetailsByUserIdAsync(Guid userId)
        {
            return await _context.Orders
                .Include(o => o.Tickets)
                    .ThenInclude(t => t.EventSeatMapping)
                .Include(o => o.Payments)
                .Where(o => o.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Order?> GetOrderWithDetailsAsync(Guid orderId)
        {
            return await _context.Orders
                .Include(o => o.Tickets)
                    .ThenInclude(t => t.EventSeatMapping)
                .Include(o => o.Payments)
                .AsNoTracking()
                .SingleOrDefaultAsync(o => o.OrderId == orderId);
        }
    }
}
