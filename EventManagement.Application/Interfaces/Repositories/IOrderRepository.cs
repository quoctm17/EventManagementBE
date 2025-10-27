using EventManagement.Domain.Models;

namespace EventManagement.Application.Interfaces.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersWithDetailsByUserIdAsync(Guid userId);
        Task<Order?> GetOrderWithDetailsAsync(Guid orderId);
    }
}
