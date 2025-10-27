using System;

namespace EventManagement.Application.DTOs.Responses
{
    public class PaymentMethodDTO
    {
        public Guid PaymentMethodId { get; set; }
        public string MethodName { get; set; } = null!;
        public string? Provider { get; set; }
        public bool IsActive { get; set; }
    }
}
