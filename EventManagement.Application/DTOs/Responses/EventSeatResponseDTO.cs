using System;

namespace EventManagement.Application.DTOs.Responses
{
    public class EventSeatResponseDTO
    {
        public Guid SeatId { get; set; }
        public string RowLabel { get; set; } = null!;
        public int SeatNumber { get; set; }
        public string TicketCategory { get; set; } = null!;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsHeld { get; set; }
    }
}
