using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class EventSeatMapping
{
    public Guid EventId { get; set; }

    public Guid SeatId { get; set; }

    public string TicketCategory { get; set; } = null!;

    public decimal Price { get; set; }

    public bool? IsAvailable { get; set; }

    public string? Additional { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual Seat Seat { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
