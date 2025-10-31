using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Infrastructure.Persistence.Repositories
{
    public class TransactionRepository : RepositoryBase<Transaction>, ITransactionRepository
    {
        public TransactionRepository(EventManagementDbContext context) : base(context) { }

        public async Task<IEnumerable<Transaction>> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.Transactions.AsNoTracking()
                .Where(t => t.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByRefundRequestIdAsync(Guid refundRequestId)
        {
            return await _context.Transactions.AsNoTracking()
                .Where(t => t.RefundRequestId == refundRequestId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Transactions.AsNoTracking()
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }
    }
}
