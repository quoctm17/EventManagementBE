using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class Checkin
{
    public Guid CheckinId { get; set; }

    public Guid TicketId { get; set; }

    public Guid StaffId { get; set; }

    public DateTime? CheckinTime { get; set; }

    public string? Additional { get; set; }

    public virtual User Staff { get; set; } = null!;

    public virtual Ticket Ticket { get; set; } = null!;
}
