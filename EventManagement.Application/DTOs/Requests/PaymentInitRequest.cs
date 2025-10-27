using System;

namespace EventManagement.Application.DTOs.Requests
{
    public class PaymentInitRequest
    {
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string? ReturnUrl { get; set; }
        public string? CancelUrl { get; set; }
        public string Description { get; set; } = string.Empty;
        // Gateway key used to generate a deterministic orderInfo/transactionRef (e.g., PAY2S, NAPAS)
        public string GatewayKey { get; set; } = string.Empty;
    }
}
