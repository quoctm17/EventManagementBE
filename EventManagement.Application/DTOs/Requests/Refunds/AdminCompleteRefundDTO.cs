using System;

namespace EventManagement.Application.DTOs.Requests.Refunds
{
    public class AdminCompleteRefundDTO
    {
        public Guid RefundRequestId { get; set; }
        public string? ReceiptImageUrl { get; set; }
        public string? AdminNote { get; set; }
    }
}
