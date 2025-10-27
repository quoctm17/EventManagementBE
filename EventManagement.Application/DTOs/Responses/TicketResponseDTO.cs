using System;

namespace EventManagement.Application.DTOs.Responses
{
    public class TicketResponseDTO
    {
        public Guid TicketId { get; set; }
        public Guid OrderId { get; set; }
        public Guid EventId { get; set; }
        public Guid SeatId { get; set; }
        public decimal Price { get; set; }
        public Guid? AttendeeId { get; set; }
        public string? Qrcode { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string Status { get; set; } = null!;
        public string? Additional { get; set; }
        public string? TicketCategory { get; set; }
        public string? SeatLabel { get; set; }
    }
}
