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
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? CoverImageUrl { get; set; }
        public string VenueName { get; set; } = null!;
        public string VenueProvince { get; set; } = null!;

        public List<TicketResponseDTO> Tickets { get; set; } = new List<TicketResponseDTO>();
    }
}
