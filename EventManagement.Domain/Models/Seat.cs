using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class Seat
{
    public Guid SeatId { get; set; }

    public Guid VenueId { get; set; }

    public string RowLabel { get; set; } = null!;

    public int SeatNumber { get; set; }

    public string? Additional { get; set; }

    public virtual ICollection<EventSeatMapping> EventSeatMappings { get; set; } = new List<EventSeatMapping>();

    public virtual ICollection<SeatHold> SeatHolds { get; set; } = new List<SeatHold>();

    public virtual Venue Venue { get; set; } = null!;
}
