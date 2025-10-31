using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventManagement.Domain.Models;

namespace EventManagement.Application.Interfaces.Repositories
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> GetByOrderIdAsync(Guid orderId);
        Task<IEnumerable<Transaction>> GetByRefundRequestIdAsync(Guid refundRequestId);
        Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId);
    }
}
