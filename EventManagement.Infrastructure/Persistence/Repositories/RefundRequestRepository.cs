using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Infrastructure.Persistence.Repositories
{
    public class RefundRequestRepository : RepositoryBase<RefundRequest>, IRefundRequestRepository
    {
        public RefundRequestRepository(EventManagementDbContext context) : base(context) { }

        public async Task<IEnumerable<RefundRequest>> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.RefundRequests.AsNoTracking()
                .Where(r => r.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<RefundRequest>> GetByUserIdAsync(Guid userId)
        {
            return await _context.RefundRequests.AsNoTracking()
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }

        public async Task<RefundRequest?> GetWithItemsAsync(Guid refundRequestId)
        {
            return await _context.RefundRequests
                .Include(r => r.RefundRequestItems)
                .SingleOrDefaultAsync(r => r.RefundRequestId == refundRequestId);
        }
    }
}
