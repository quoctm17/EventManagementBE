using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class UserBankAccount
{
    public Guid BankAccountId { get; set; }

    public Guid UserId { get; set; }

    public string BankName { get; set; } = null!;

    public string AccountNumber { get; set; } = null!;

    public string AccountHolderName { get; set; } = null!;

    public bool? IsDefault { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Additional { get; set; }

    public virtual ICollection<RefundRequest> RefundRequests { get; set; } = new List<RefundRequest>();

    public virtual User User { get; set; } = null!;
}
