using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EventManagement.Application.DTOs.Requests;
using EventManagement.Application.DTOs.Responses;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.Constants;
using Microsoft.AspNetCore.Authorization;

namespace EventManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutController : ControllerBase
    {
        private readonly ICheckoutService _checkoutService;

        public CheckoutController(ICheckoutService checkoutService)
        {
            _checkoutService = checkoutService;
        }

        [HttpPost("prepare")]
        [Authorize]
        public async Task<ActionResult<HTTPResponseValue<CheckoutPrepareResponseDTO>>> Prepare([FromBody] CheckoutPrepareRequestDTO request)
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader))
            {
                var resp = new HTTPResponseValue<string>(null, StatusResponse.Unauthorized, MessageResponse.Unauthorized);
                return Unauthorized(resp);
            }
            if (request == null || request.UserId == Guid.Empty || request.EventId == Guid.Empty || request.SeatIds == null || request.SeatIds.Count == 0)
            {
                var bad = new HTTPResponseValue<string>(null, StatusResponse.BadRequest, MessageResponse.BadRequest);
                return BadRequest(bad);
            }

            try
            {
                var data = await _checkoutService.PrepareAsync(authHeader!, request);
                var resp = new HTTPResponseValue<CheckoutPrepareResponseDTO>(data, StatusResponse.Success, MessageResponse.Success);
                return Ok(resp);
            }
            catch (InvalidOperationException ex)
            {
                var msg = ex.Message;
                if (string.Equals(msg, "Unauthorized", StringComparison.OrdinalIgnoreCase))
                {
                    var unauth = new HTTPResponseValue<string>(null, StatusResponse.Unauthorized, MessageResponse.Unauthorized);
                    return Unauthorized(unauth);
                }
                var bad = new HTTPResponseValue<string>(null, StatusResponse.BadRequest, ex.Message);
                return BadRequest(bad);
            }
            catch
            {
                var error = new HTTPResponseValue<string>(null, StatusResponse.Error, MessageResponse.Error);
                return StatusCode(500, error);
            }
        }
    }
}
