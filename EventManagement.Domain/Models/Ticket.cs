using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class Ticket
{
    public Guid TicketId { get; set; }

    public Guid OrderId { get; set; }

    public Guid EventId { get; set; }

    public Guid SeatId { get; set; }

    public decimal Price { get; set; }

    public Guid? AttendeeId { get; set; }

    public string? Qrcode { get; set; }

    public DateTime? PurchaseDate { get; set; }

    public string Status { get; set; } = null!;

    public string? Additional { get; set; }

    public virtual User? Attendee { get; set; }

    public virtual ICollection<Checkin> Checkins { get; set; } = new List<Checkin>();

    public virtual EventSeatMapping EventSeatMapping { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
