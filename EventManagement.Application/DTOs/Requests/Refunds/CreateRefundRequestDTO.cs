using System;

namespace EventManagement.Application.DTOs.Requests.Refunds
{
    public class CreateRefundRequestDTO
    {
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string? Reason { get; set; }

        // If user wants to use a saved bank account
        public Guid? BankAccountId { get; set; }

        // Or provide manual details
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountHolderName { get; set; }

        // Save manual details for later and optionally set as default
        public bool SaveBankAccount { get; set; }
        public bool SetAsDefault { get; set; }
    }
}
