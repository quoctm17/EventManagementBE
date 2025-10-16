using Microsoft.AspNetCore.Mvc;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.DTOs.Requests;
using EventManagement.Application.DTOs.Responses;
using EventManagement.Application.Constants;  // ✅ thêm dòng này

namespace EventManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDTO request)
        {
            var authResult = await _authService.LoginAsync(request);

            if (authResult == null)
            {
                var responseFail = new HTTPResponseValue<AuthResponseDTO>(
                    null,
                    StatusResponse.Unauthorized,
                    MessageResponse.Unauthorized
                );
                return Unauthorized(responseFail);
            }

            var responseSuccess = new HTTPResponseValue<AuthResponseDTO>(
                authResult,
                StatusResponse.Success,
                MessageResponse.Success
            );

            return Ok(responseSuccess);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequestDTO request)
        {
            var success = await _authService.RegisterAsync(request);

            if (!success)
            {
                var responseFail = new HTTPResponseValue<bool>(
                    false,
                    StatusResponse.BadRequest,
                    MessageResponse.BadRequest
                );
                return BadRequest(responseFail);
            }

            var responseSuccess = new HTTPResponseValue<bool>(
                true,
                StatusResponse.Success,
                MessageResponse.Success
            );

            return Ok(responseSuccess);
        }
    }
}