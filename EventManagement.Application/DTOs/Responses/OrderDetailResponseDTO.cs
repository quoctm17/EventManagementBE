using System;
using System.Collections.Generic;

namespace EventManagement.Application.DTOs.Responses
{
    public class OrderDetailResponseDTO
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public string? Additional { get; set; }

        public EventBasicDTO? Event { get; set; }
        public List<TicketResponseDTO> Tickets { get; set; } = new();
        public List<PaymentResponseDTO> Payments { get; set; } = new();
        public List<RefundRequestResponseDTO> RefundRequests { get; set; } = new();
    }

    public class EventBasicDTO
    {
        public Guid EventId { get; set; }
        public string EventName { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime SaleStartTime { get; set; }
        public DateTime SaleEndTime { get; set; }
        public DateTime EventStartTime { get; set; }
        public DateTime EventEndTime { get; set; }
        public string VenueName { get; set; } = null!;
        public string VenueAddress { get; set; } = null!;
        public string VenueProvince { get; set; } = null!;
        public string? CoverImageUrl { get; set; }
        public string Status { get; set; } = null!;
    }
}
