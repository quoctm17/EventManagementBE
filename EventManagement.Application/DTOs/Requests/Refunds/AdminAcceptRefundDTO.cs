using System;

namespace EventManagement.Application.DTOs.Requests.Refunds
{
    public class AdminAcceptRefundDTO
    {
        public Guid RefundRequestId { get; set; }
        public string? AdminNote { get; set; }
    }
}
