using System;
using System.Collections.Generic;

namespace EventManagement.Application.DTOs.Responses
{
    // Grouped tickets under an event for better UI rendering
    public class TicketsByEventDTO
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
        public string? CoverImageUrl { get; set; }
        public string VenueName { get; set; } = null!;
        public string VenueProvince { get; set; } = null!;

        public List<TicketResponseDTO> Tickets { get; set; } = new List<TicketResponseDTO>();
    }
}
