using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class EventStaff
{
    public Guid EventId { get; set; }

    public Guid UserId { get; set; }

    public Guid AssignedBy { get; set; }

    public DateTime? AssignedAt { get; set; }

    public bool? IsActive { get; set; }

    public string? Additional { get; set; }

    public virtual User AssignedByNavigation { get; set; } = null!;

    public virtual Event Event { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
