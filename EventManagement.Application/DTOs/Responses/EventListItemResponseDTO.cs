using System;
using System.Collections.Generic;

namespace EventManagement.Application.DTOs.Responses
{
    public class EventListItemDTO
    {
        public Guid EventId { get; set; }
        public string EventName { get; set; } = null!;
        public DateOnly EventDate { get; set; }
        // Sale window
        public DateTime SaleStartTime { get; set; }
        public DateTime SaleEndTime { get; set; }
        // Actual event time window
        public DateTime EventStartTime { get; set; }
        public DateTime EventEndTime { get; set; }
        public DateTime? OrderPendingExpires { get; set; }
        public string? CoverImageUrl { get; set; }
        public string VenueName { get; set; } = null!;
        public string VenueProvince { get; set; } = null!;
        public List<string> Categories { get; set; } = new List<string>();
        public decimal? StartingPrice { get; set; }
    }
}
