using System;
using System.Collections.Generic;

namespace EventManagement.Application.DTOs.Requests
{
    public class CreateOrderRequestDTO
    {
        public Guid PaymentMethodId { get; set; }
        public Guid EventId { get; set; }
        public List<Guid> SeatIds { get; set; } = new List<Guid>();
        public string? ReturnUrl { get; set; }
        public string? CancelUrl { get; set; }
    }
}
