using System;
using System.Collections.Generic;

namespace EventManagement.Application.DTOs.Responses
{
    public class OrderResponseDTO
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public string? Additional { get; set; }

        public List<TicketResponseDTO> Tickets { get; set; } = new List<TicketResponseDTO>();
        public List<object> Payments { get; set; } = new List<object>();
    }
}
