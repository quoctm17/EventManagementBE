using System;

namespace EventManagement.Application.DTOs.Requests.Refunds
{
    public class AdminRejectRefundDTO
    {
        public Guid RefundRequestId { get; set; }
        public string? AdminNote { get; set; }
    }
}