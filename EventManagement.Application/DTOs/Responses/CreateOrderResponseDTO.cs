using System;

namespace EventManagement.Application.DTOs.Responses
{
    public class CreateOrderResponseDTO
    {
        public Guid OrderId { get; set; }
        public Guid PaymentMethodId { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string? PaymentRedirectUrl { get; set; }
        public Guid PaymentId { get; set; }
        public string? TransactionRef { get; set; }
    }
}
