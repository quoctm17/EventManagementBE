using System;

namespace EventManagement.Application.DTOs.Responses
{
    // Aggregated ticket tier for display in event detail (one per category)
    public class TicketTierDTO
    {
        public string TicketCategory { get; set; } = null!; // e.g. "VIP", "Standard"
        public decimal Price { get; set; } // representative price (min price for the category)
        public int AvailableSeats { get; set; } // number of available seat mappings in this category
    }
}
