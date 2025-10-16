using System;

namespace EventManagement.Application.DTOs.Responses
{
    public class RecommendedEventDTO
    {
        public Guid EventId { get; set; }
        public string EventName { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        // The date the event occurs (day) — maps from Event.EventDate (DateOnly)
        public DateOnly EventDate { get; set; }
        public string? CoverImageUrl { get; set; }
        public string VenueName { get; set; } = null!;
        public string VenueAddress { get; set; } = null!;
        public string VenueProvince { get; set; } = null!;
        public List<string> Categories { get; set; } = new List<string>();
        public decimal? StartingPrice { get; set; }
    }
}
