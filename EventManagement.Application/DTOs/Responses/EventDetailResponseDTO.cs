using System;
using System.Collections.Generic;
using EventManagement.Application.DTOs.Responses;

namespace EventManagement.Application.DTOs.Responses
{
    public class EventDetailDTO
    {
        public Guid EventId { get; set; }
        public string EventName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateOnly EventDate { get; set; }
        // Sale window
        public DateTime SaleStartTime { get; set; }
        public DateTime SaleEndTime { get; set; }
        // Actual event time window
        public DateTime EventStartTime { get; set; }
        public DateTime EventEndTime { get; set; }
        public DateTime? OrderPendingExpires { get; set; }
        public string VenueName { get; set; } = null!;
        public string VenueAddress { get; set; } = null!;
        public string VenueProvince { get; set; } = null!;
        public List<string> ImageUrls { get; set; } = new List<string>();
        public List<string> Categories { get; set; } = new List<string>();
        // New fields required by frontend
        public string Status { get; set; } = null!;
        public UserResponseDTO? Organizer { get; set; }
        public List<TicketTierDTO> TicketTiers { get; set; } = new List<TicketTierDTO>();
    }
}
