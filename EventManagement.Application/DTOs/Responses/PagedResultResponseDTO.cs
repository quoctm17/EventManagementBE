using System.Collections.Generic;

namespace EventManagement.Application.DTOs.Responses
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        // Available filter options collected from the matching events
        public List<CategoryOptionDTO> AvailableCategories { get; set; } = new List<CategoryOptionDTO>();
        public List<string> AvailableProvinces { get; set; } = new List<string>();
    }
}
