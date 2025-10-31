using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventManagement.Domain.Models;

namespace EventManagement.Application.Interfaces.Repositories
{
    public interface IRefundRequestRepository : IRepository<RefundRequest>
    {
        Task<IEnumerable<RefundRequest>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<RefundRequest>> GetByOrderIdAsync(Guid orderId);
    }
}
