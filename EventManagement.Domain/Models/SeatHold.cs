using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class SeatHold
{
    public Guid HoldId { get; set; }

    public Guid EventId { get; set; }

    public Guid SeatId { get; set; }

    public Guid UserId { get; set; }

    public DateTime HoldExpiresAt { get; set; }

    public Guid? OrderId { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual Order? Order { get; set; }

    public virtual Seat Seat { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
