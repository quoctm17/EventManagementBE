using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class RefundPolicy
{
    public Guid RefundPolicyId { get; set; }

    public Guid? EventId { get; set; }

    public string? TicketCategory { get; set; }

    public bool? IsEnabled { get; set; }

    public DateTime? EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public string? Note { get; set; }

    public string? Additional { get; set; }

    public virtual Event? Event { get; set; }

    public virtual ICollection<RefundPolicyRule> RefundPolicyRules { get; set; } = new List<RefundPolicyRule>();
}
