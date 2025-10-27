using System;

namespace EventManagement.Application.DTOs.Requests
{
    public class ManualCancelRequestDTO
    {
        public Guid? OrderId { get; set; }
        public Guid? PaymentId { get; set; }
        public string? Reason { get; set; }
    }
}
