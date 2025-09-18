using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class EventImage
{
    public Guid ImageId { get; set; }

    public Guid EventId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? Caption { get; set; }

    public int? DisplayOrder { get; set; }

    public bool? IsCover { get; set; }

    public string? Additional { get; set; }

    public virtual Event Event { get; set; } = null!;
}
