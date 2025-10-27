using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.DTOs.Responses;
using EventManagement.Application.Constants;

namespace EventManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<HTTPResponseValue<IEnumerable<TicketsByEventDTO>>>> GetCurrentUserTickets()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader))
            {
                var resp = new HTTPResponseValue<string>(null, StatusResponse.Unauthorized, MessageResponse.Unauthorized);
                return Unauthorized(resp);
            }

            try
            {
                var tickets = await _ticketService.GetCurrentUserTicketsAsync(authHeader);
                var success = new HTTPResponseValue<IEnumerable<TicketsByEventDTO>>(tickets, StatusResponse.Success, MessageResponse.Success);
                return Ok(success);
            }
            catch
            {
                var error = new HTTPResponseValue<string>(null, StatusResponse.Error, MessageResponse.Error);
                return StatusCode(500, error);
            }
        }

        [HttpGet("by-order/{orderId}")]
        [Authorize]
        public async Task<ActionResult<HTTPResponseValue<IEnumerable<TicketResponseDTO>>>> GetTicketsByOrderId(Guid orderId)
        {
            try
            {
                var tickets = await _ticketService.GetTicketsByOrderIdAsync(orderId);
                var success = new HTTPResponseValue<IEnumerable<TicketResponseDTO>>(tickets, StatusResponse.Success, MessageResponse.Success);
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
