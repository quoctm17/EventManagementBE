using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class VenueImage
{
    public Guid ImageId { get; set; }

    public Guid VenueId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? Caption { get; set; }

    public int? DisplayOrder { get; set; }

    public bool? IsMain { get; set; }

    public string? Additional { get; set; }

    public virtual Venue Venue { get; set; } = null!;
}
