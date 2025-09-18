using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class Contract
{
    public Guid ContractId { get; set; }

    public Guid OrganizerId { get; set; }

    public string ContractType { get; set; } = null!;

    public Guid? EventId { get; set; }

    public string ContractFileUrl { get; set; } = null!;

    public DateTime EffectiveDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public string? Status { get; set; }

    public string? Additional { get; set; }

    public virtual Event? Event { get; set; }

    public virtual User Organizer { get; set; } = null!;
}
