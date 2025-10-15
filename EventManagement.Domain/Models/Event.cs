using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class Event
{
    public Guid EventId { get; set; }

    public Guid OrganizerId { get; set; }

    public Guid VenueId { get; set; }

    public string EventName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public DateOnly EventDate { get; set; }

    public string? CoverImageUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsPublished { get; set; }

    public string Status { get; set; } = null!;

    public string? Additional { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<EventImage> EventImages { get; set; } = new List<EventImage>();

    public virtual ICollection<EventSeatMapping> EventSeatMappings { get; set; } = new List<EventSeatMapping>();

    public virtual ICollection<EventStaff> EventStaffs { get; set; } = new List<EventStaff>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual User Organizer { get; set; } = null!;

    public virtual ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();

    public virtual Venue Venue { get; set; } = null!;

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
