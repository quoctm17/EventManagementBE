using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class PaymentMethod
{
    public Guid PaymentMethodId { get; set; }

    public string MethodName { get; set; } = null!;

    public string? Provider { get; set; }

    public bool? IsActive { get; set; }

    public string? Additional { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
