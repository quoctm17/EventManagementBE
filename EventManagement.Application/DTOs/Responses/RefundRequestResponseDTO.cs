using System;
using System.Collections.Generic;

namespace EventManagement.Application.DTOs.Responses
{
    public class RefundRequestResponseDTO
    {
        public Guid RefundRequestId { get; set; }
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public Guid? BankAccountId { get; set; }
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountHolderName { get; set; }
        public decimal Amount { get; set; }
        public string? Status { get; set; }
        public string? Reason { get; set; }
        public string? AdminNote { get; set; }
        public string? ReceiptImageUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public Guid? ProcessedBy { get; set; }

        public List<RefundRequestItemDTO> Items { get; set; } = new();
    }
}
