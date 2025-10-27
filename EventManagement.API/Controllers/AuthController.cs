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
        public async Task<ActionResult<HTTPResponseValue<AuthResponseDTO>>> Login([FromBody] UserLoginRequestDTO request)
        {
            try
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
            catch
            {
                var error = new HTTPResponseValue<string>(null, StatusResponse.Error, MessageResponse.Error);
                return StatusCode(500, error);
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<HTTPResponseValue<bool>>> Register([FromBody] UserRegisterRequestDTO request)
        {
            try
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
            catch
            {
                var error = new HTTPResponseValue<string>(null, StatusResponse.Error, MessageResponse.Error);
                return StatusCode(500, error);
            }
        }

        [HttpPost("validate-token")]
        public async Task<ActionResult<HTTPResponseValue<bool>>> ValidateToken([FromBody] TokenValidationRequestDTO request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Token))
            {
                var bad = new HTTPResponseValue<string>(null, StatusResponse.BadRequest, MessageResponse.BadRequest);
                return BadRequest(bad);
            }

            try
            {
                var isValid = await _authService.ValidateTokenAsync(request.Token);
                var resp = new HTTPResponseValue<bool>(isValid, StatusResponse.Success, MessageResponse.Success);
                return Ok(resp);
            }
            catch
            {
                var error = new HTTPResponseValue<string>(null, StatusResponse.Error, MessageResponse.Error);
                return StatusCode(500, error);
            }
        }
    }
}