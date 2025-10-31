using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventManagement.Application.DTOs.Requests.Refunds;
using EventManagement.Application.DTOs.Responses;

namespace EventManagement.Application.Interfaces.Services
{
    public interface IRefundService
    {
        Task<RefundRequestResponseDTO> CreateRefundRequestAsync(string authHeader, CreateRefundRequestDTO request);
        Task<bool> AdminMarkRefundPaidAsync(Guid adminUserId, AdminCompleteRefundDTO request);
    }
}
