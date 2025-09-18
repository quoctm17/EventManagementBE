using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class Venue
{
    public Guid VenueId { get; set; }

    public string VenueName { get; set; } = null!;

    public string Address { get; set; } = null!;

    public int? TotalSeats { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Additional { get; set; }

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
}
