using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class SystemLog
{
    public Guid LogId { get; set; }

    public Guid? UserId { get; set; }

    public string? Action { get; set; }

    public string? Details { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Additional { get; set; }

    public virtual User? User { get; set; }
}
