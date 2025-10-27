using System;
using System.Collections.Generic;

namespace EventManagement.Application.DTOs.Requests
{
    public class CheckoutPrepareRequestDTO
    {
        public Guid EventId { get; set; }
        public List<Guid> SeatIds { get; set; } = new List<Guid>();
    }
}
