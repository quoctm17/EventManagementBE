using System;

namespace EventManagement.Application.DTOs.Requests.Tests
{
    public class PaymentTestRequestDTO
    {
        public Guid? OrderId { get; set; }
        public decimal Amount { get; set; }
        public string? OrderInfo { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
