using System;

namespace EventManagement.Application.DTOs.Responses
{
    public class PaymentResponseDTO
    {
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public Guid PaymentMethodId { get; set; }
        public string? MethodName { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? TransactionDate { get; set; }
        public string? TransactionRef { get; set; }
        public string? Additional { get; set; }
    }
}
