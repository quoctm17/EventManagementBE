using System;

namespace EventManagement.Application.DTOs.Responses
{
    public class DestinationDTO
    {
        public Guid VenueId { get; set; }
        public string VenueName { get; set; } = null!;
        public string City { get; set; } = null!;
        public string? ImageUrl { get; set; }
    }
}
