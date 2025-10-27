using System;

namespace EventManagement.Application.DTOs.Responses
{
    public class RoleResponseDTO
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}
