using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EventManagement.Application.DTOs.Responses;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Application.Interfaces.Services;

namespace EventManagement.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;

        public RoleService(IRoleRepository roleRepository, IMapper mapper)
        {
            _roleRepository = roleRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RoleResponseDTO>> GetRolesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return roles.Select(r => _mapper.Map<RoleResponseDTO>(r)).ToList();
        }
    }
}
