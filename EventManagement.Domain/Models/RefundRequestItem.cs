using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class RefundRequestItem
{
    public Guid RefundRequestItemId { get; set; }

    public Guid RefundRequestId { get; set; }

    public Guid TicketId { get; set; }

    public Guid? RefundRuleId { get; set; }

    public decimal? RefundPercentApplied { get; set; }

    public decimal? RefundAmount { get; set; }

    public virtual RefundRequest RefundRequest { get; set; } = null!;

    public virtual RefundPolicyRule? RefundRule { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;
}
