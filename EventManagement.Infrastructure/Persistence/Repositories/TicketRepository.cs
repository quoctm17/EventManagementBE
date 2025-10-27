using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Domain.Models;
using EventManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using EventManagement.Domain.Enums;

namespace EventManagement.Infrastructure.Persistence.Repositories
{
    public class TicketRepository : RepositoryBase<Ticket>, ITicketRepository
    {
        public TicketRepository(EventManagementDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByUserIdAsync(Guid userId)
        {
            return await _context.Tickets
                .Include(t => t.EventSeatMapping)
                    .ThenInclude(esm => esm.Event)
                        .ThenInclude(e => e.Venue)
                .Include(t => t.EventSeatMapping)
                    .ThenInclude(esm => esm.Seat)
                .Include(t => t.Order)
                .Where(t => t.AttendeeId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByOrderIdAsync(Guid orderId)
        {
            return await _context.Tickets
                .Include(t => t.EventSeatMapping)
                    .ThenInclude(esm => esm.Event)
                        .ThenInclude(e => e.Venue)
                .Include(t => t.EventSeatMapping)
                    .ThenInclude(esm => esm.Seat)
                .Include(t => t.Attendee)
                .Where(t => t.OrderId == orderId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> CountByAttendeeAsync(Guid userId)
        {
            return await _context.Tickets
                .Where(t => t.AttendeeId == userId
                            && t.Status != TicketStatus.Cancelled
                            && t.Status != TicketStatus.Refunded)
                .CountAsync();
        }
    }
}
