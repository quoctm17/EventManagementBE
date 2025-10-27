using System.Collections.Generic;
using System.Threading.Tasks;
using EventManagement.Application.DTOs.Responses;

namespace EventManagement.Application.Interfaces.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleResponseDTO>> GetRolesAsync();
    }
}
