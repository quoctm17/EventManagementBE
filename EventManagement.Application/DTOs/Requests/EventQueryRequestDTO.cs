using System;
using System.Collections.Generic;

namespace EventManagement.Application.DTOs.Requests
{
    public class EventQueryRequestDTO
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public string? Search { get; set; }
        public string? Province { get; set; }
        public List<Guid>? CategoryIds { get; set; }
        public DateTime? Date { get; set; }
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
        public string? SortBy { get; set; }
    }
}
