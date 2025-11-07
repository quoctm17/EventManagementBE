using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.DTOs.Requests.Refunds;
using EventManagement.Application.DTOs.Responses;
using EventManagement.Application.Constants;

namespace EventManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RefundController : ControllerBase
    {
        private readonly IRefundService _refundService;

        public RefundController(IRefundService refundService)
        {
            _refundService = refundService;
        }

        // Create a refund request for current user
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<HTTPResponseValue<RefundRequestResponseDTO>>> CreateRefund([FromBody] CreateRefundRequestDTO request)
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader))
            {
                var resp = new HTTPResponseValue<string>(null, StatusResponse.Unauthorized, MessageResponse.Unauthorized);
                return Unauthorized(resp);
            }

            if (request == null)
            {
                var bad = new HTTPResponseValue<string>(null, StatusResponse.BadRequest, MessageResponse.BadRequest);
                return BadRequest(bad);
            }

            try
            {
                var result = await _refundService.CreateRefundRequestAsync(authHeader, request);
                var success = new HTTPResponseValue<RefundRequestResponseDTO>(result, StatusResponse.Success, MessageResponse.Success);
                return Ok(success);
            }
            catch (InvalidOperationException ex)
            {
                var bad = new HTTPResponseValue<string>(null, StatusResponse.BadRequest, ex.Message);
                return BadRequest(bad);
            }
            catch
            {
                var error = new HTTPResponseValue<string>(null, StatusResponse.Error, MessageResponse.Error);
                return StatusCode(500, error);
            }
        }

        // Admin/manual: mark refund as paid and attach receipt
        [HttpPost("{refundRequestId:guid}/complete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CompleteRefund(Guid refundRequestId, [FromBody] AdminCompleteRefundDTO request)
        {
            if (request == null)
            {
                return BadRequest(new HTTPResponseValue<string>(null, StatusResponse.BadRequest, MessageResponse.BadRequest));
            }

            // Bind route id into DTO
            request.RefundRequestId = refundRequestId;

            // Extract admin user id from claims (sub or nameidentifier)
            var userIdStr = User?.Claims?.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                         ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var adminUserId))
            {
                return Unauthorized(new HTTPResponseValue<string>(null, StatusResponse.Unauthorized, MessageResponse.Unauthorized));
            }

            try
            {
                var ok = await _refundService.AdminMarkRefundPaidAsync(adminUserId, request);
                if (!ok)
                {
                    return NotFound(new HTTPResponseValue<string>(null, StatusResponse.NotFound, "Refund request not found"));
                }
                return Ok(new HTTPResponseValue<object>(new { success = true }, StatusResponse.Success, MessageResponse.Success));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new HTTPResponseValue<string>(null, StatusResponse.BadRequest, ex.Message));
            }
            catch
            {
                return StatusCode(500, new HTTPResponseValue<string>(null, StatusResponse.Error, MessageResponse.Error));
            }
        }

        // Admin: accept a refund request (transition to PendingRefund and lock involved tickets)
        [HttpPost("{refundRequestId:guid}/accept")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AcceptRefund(Guid refundRequestId, [FromBody] AdminAcceptRefundDTO request)
        {
            if (request == null)
            {
                return BadRequest(new HTTPResponseValue<string>(null, StatusResponse.BadRequest, MessageResponse.BadRequest));
            }

            request.RefundRequestId = refundRequestId;

            var userIdStr = User?.Claims?.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                         ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var adminUserId))
            {
                return Unauthorized(new HTTPResponseValue<string>(null, StatusResponse.Unauthorized, MessageResponse.Unauthorized));
            }

            try
            {
                var ok = await _refundService.AdminAcceptRefundAsync(adminUserId, request);
                if (!ok)
                {
                    return NotFound(new HTTPResponseValue<string>(null, StatusResponse.NotFound, "Refund request not found"));
                }
                return Ok(new HTTPResponseValue<object>(new { success = true }, StatusResponse.Success, MessageResponse.Success));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new HTTPResponseValue<string>(null, StatusResponse.BadRequest, ex.Message));
            }
            catch
            {
                return StatusCode(500, new HTTPResponseValue<string>(null, StatusResponse.Error, MessageResponse.Error));
            }
        }

        // Admin: reject a refund request (Pending or Approved)
        [HttpPost("{refundRequestId:guid}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectRefund(Guid refundRequestId, [FromBody] AdminRejectRefundDTO request)
        {
            if (request == null)
            {
                return BadRequest(new HTTPResponseValue<string>(null, StatusResponse.BadRequest, MessageResponse.BadRequest));
            }

            request.RefundRequestId = refundRequestId;

            var userIdStr = User?.Claims?.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                         ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var adminUserId))
            {
                return Unauthorized(new HTTPResponseValue<string>(null, StatusResponse.Unauthorized, MessageResponse.Unauthorized));
            }

            try
            {
                var ok = await _refundService.AdminRejectRefundAsync(adminUserId, request);
                if (!ok)
                {
                    return NotFound(new HTTPResponseValue<string>(null, StatusResponse.NotFound, "Refund request not found"));
                }
                return Ok(new HTTPResponseValue<object>(new { success = true }, StatusResponse.Success, MessageResponse.Success));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new HTTPResponseValue<string>(null, StatusResponse.BadRequest, ex.Message));
            }
            catch
            {
                return StatusCode(500, new HTTPResponseValue<string>(null, StatusResponse.Error, MessageResponse.Error));
            }
        }
    }
}
