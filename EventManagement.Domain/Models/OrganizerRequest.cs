using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class OrganizerRequest
{
    public Guid RequestId { get; set; }

    public Guid UserId { get; set; }

    public string? Reason { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? RequestedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public Guid? ProcessedBy { get; set; }

    public string? Additional { get; set; }

    public virtual User? ProcessedByNavigation { get; set; }

    public virtual User User { get; set; } = null!;
}
