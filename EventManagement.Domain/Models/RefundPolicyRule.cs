using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class RefundPolicyRule
{
    public Guid RefundPolicyRuleId { get; set; }

    public Guid RefundPolicyId { get; set; }

    public int CutoffMinutesBeforeStart { get; set; }

    public decimal RefundPercent { get; set; }

    public decimal? FlatFee { get; set; }

    public int RuleOrder { get; set; }

    public virtual RefundPolicy RefundPolicy { get; set; } = null!;

    public virtual ICollection<RefundRequestItem> RefundRequestItems { get; set; } = new List<RefundRequestItem>();
}
