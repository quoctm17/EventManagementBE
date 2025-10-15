using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventManagement.Application.Constants;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.DTOs.Responses;
using EventManagement.Domain.Models;

namespace EventManagement.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UserController : ControllerBase
	{
		private readonly IUserService _userService;
		private readonly IJwtAuthService _jwtAuthService;

		public UserController(IUserService userService, IJwtAuthService jwtAuthService)
		{
			_userService = userService;
			_jwtAuthService = jwtAuthService;
		}

		[HttpGet("me")]
		[Authorize]
		public async Task<IActionResult> GetCurrentUser()
		{
			var authHeader = Request.Headers["Authorization"].FirstOrDefault();
			var response = new HTTPResponseValue<UserResponseDTO>();
			if (string.IsNullOrEmpty(authHeader))
			{
				response.Status = StatusResponse.Unauthorized;
				response.Message = MessageResponse.Unauthorized;
				response.Content = null;
				return Unauthorized(response);
			}

			var user = await _userService.GetCurrentUserAsync(authHeader);
			if (user == null)
			{
				response.Status = StatusResponse.Unauthorized;
				response.Message = MessageResponse.Unauthorized;
				response.Content = null;
				return Unauthorized(response);
			}

			response.Status = StatusResponse.Success;
			response.Message = MessageResponse.Success;
			response.Content = user;
			return Ok(response);
		}
	}
}
