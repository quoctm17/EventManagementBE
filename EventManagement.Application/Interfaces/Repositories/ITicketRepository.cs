using EventManagement.Domain.Models;

namespace EventManagement.Application.Interfaces.Repositories
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        Task<IEnumerable<Ticket>> GetTicketsByUserIdAsync(Guid userId);
        Task<IEnumerable<Ticket>> GetTicketsByOrderIdAsync(Guid orderId);
        Task<int> CountByAttendeeAsync(Guid userId);
    }
}
