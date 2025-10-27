using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventManagement.Application.DTOs.Responses;
using EventManagement.Application.DTOs.Requests;
using EventManagement.Application.DTOs.Requests.Webhooks;

namespace EventManagement.Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResponseDTO>> GetCurrentUserOrdersAsync(string token);
        Task<OrderResponseDTO?> GetOrderByIdAsync(Guid orderId);
        Task<CreateOrderResponseDTO> CreateOrderAsync(string authHeader, CreateOrderRequestDTO request);
        // Handle provider webhook (moved from PaymentService to avoid circular dependency)
        Task<bool> HandlePay2SWebhookAsync(Pay2SWebhookRequestDTO payload);
        // Handle Pay2S IPN (gateway callback)
        Task<bool> HandlePay2SIpnAsync(Pay2SIpnRequestDTO payload);

        // Manual cancel endpoint: cancel a pending order/payment initiated by FE when user abandons checkout
        Task<bool> CancelPendingOrderAsync(string authHeader, ManualCancelRequestDTO request);
    }
}
