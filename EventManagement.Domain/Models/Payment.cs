using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class Payment
{
    public Guid PaymentId { get; set; }

    public Guid OrderId { get; set; }

    public Guid PaymentMethodId { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = null!;

    public string? TransactionRef { get; set; }

    public DateTime? TransactionDate { get; set; }

    public string? Additional { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual PaymentMethod PaymentMethod { get; set; } = null!;
}
