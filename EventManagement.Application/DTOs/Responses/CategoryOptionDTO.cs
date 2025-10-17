using System;

namespace EventManagement.Application.DTOs.Responses
{
    public class CategoryOptionDTO
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}
