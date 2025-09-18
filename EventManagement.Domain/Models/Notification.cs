using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class Notification
{
    public Guid NotificationId { get; set; }

    public Guid UserId { get; set; }

    public Guid? EventId { get; set; }

    public string NotificationType { get; set; } = null!;

    public string? Subject { get; set; }

    public string? Message { get; set; }

    public DateTime? SentAt { get; set; }

    public string? Status { get; set; }

    public string? Additional { get; set; }

    public virtual Event? Event { get; set; }

    public virtual User User { get; set; } = null!;
}
