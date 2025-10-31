using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class Transaction
{
    public Guid TransactionId { get; set; }

    public Guid? UserId { get; set; }

    public Guid? PaymentId { get; set; }

    public Guid? RefundRequestId { get; set; }

    public Guid? SettlementId { get; set; }

    public Guid? OrderId { get; set; }

    public decimal Amount { get; set; }

    public string Direction { get; set; } = null!;

    public string Purpose { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public decimal SystemBalance { get; set; }

    public string? Note { get; set; }

    public string? Additional { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Payment? Payment { get; set; }

    public virtual RefundRequest? RefundRequest { get; set; }

    public virtual User? User { get; set; }
}
