using System;
using System.Threading.Tasks;
using EventManagement.Domain.Models;
using EventManagement.Domain.Enums;

namespace EventManagement.Application.Interfaces.Repositories
{
    public interface IRefundPolicyRepository
    {
        Task<RefundPolicy?> GetApplicablePolicyAsync(Guid? eventId, string? category);
    }
}