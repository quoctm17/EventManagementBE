using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.DTOs.Responses;
using EventManagement.Application.Constants;

namespace EventManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<ActionResult<HTTPResponseValue<IEnumerable<RoleResponseDTO>>>> GetRoles()
        {
            try
            {
                var roles = await _roleService.GetRolesAsync();
                var success = new HTTPResponseValue<IEnumerable<RoleResponseDTO>>(roles, StatusResponse.Success, MessageResponse.Success);
                return Ok(success);
            }
            catch
            {
                var error = new HTTPResponseValue<string>(null, StatusResponse.Error, MessageResponse.Error);
                return StatusCode(500, error);
            }
        }
    }
}
