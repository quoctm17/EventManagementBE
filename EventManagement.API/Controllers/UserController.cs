using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
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
			if (string.IsNullOrEmpty(authHeader))
			{
				var resp = new HTTPResponseValue<string>(null, StatusResponse.Unauthorized, MessageResponse.Unauthorized);
				return Unauthorized(resp);
			}

			try
			{
				var user = await _userService.GetCurrentUserAsync(authHeader);
				if (user == null)
				{
					var resp = new HTTPResponseValue<string>(null, StatusResponse.Unauthorized, MessageResponse.Unauthorized);
					return Unauthorized(resp);
				}

				var success = new HTTPResponseValue<UserResponseDTO>(user, StatusResponse.Success, MessageResponse.Success);
				return Ok(success);
			}
			catch (System.Exception)
			{
				var error = new HTTPResponseValue<string>(null, StatusResponse.Error, MessageResponse.Error);
				return StatusCode(500, error);
			}
		}
	}
}
