using System;
using System.Collections.Generic;

namespace EventManagement.Application.DTOs.Responses
{
    public class EventDetailDTO
    {
        public Guid EventId { get; set; }
        public string EventName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateOnly EventDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string VenueName { get; set; } = null!;
        public string VenueAddress { get; set; } = null!;
        public string VenueProvince { get; set; } = null!;
        public List<string> ImageUrls { get; set; } = new List<string>();
        public List<string> Categories { get; set; } = new List<string>();
    }
}
