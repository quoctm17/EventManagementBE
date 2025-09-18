using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class Settlement
{
    public Guid SettlementId { get; set; }

    public Guid OrganizerId { get; set; }

    public Guid EventId { get; set; }

    public decimal TotalRevenue { get; set; }

    public decimal CommissionFee { get; set; }

    public decimal NetAmount { get; set; }

    public string? Status { get; set; }

    public DateTime? SettlementDate { get; set; }

    public Guid? ProcessedBy { get; set; }

    public string? Note { get; set; }

    public string? Additional { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual User Organizer { get; set; } = null!;
}
