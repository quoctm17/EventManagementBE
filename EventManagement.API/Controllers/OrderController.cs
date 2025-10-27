using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.DTOs.Responses;
using EventManagement.Application.Constants;
using EventManagement.Application.DTOs.Requests;

namespace EventManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<HTTPResponseValue<IEnumerable<OrderResponseDTO>>>> GetCurrentUserOrders()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader))
            {
                var resp = new HTTPResponseValue<string>(null, StatusResponse.Unauthorized, MessageResponse.Unauthorized);
                return Unauthorized(resp);
            }

            try
            {
                var orders = await _orderService.GetCurrentUserOrdersAsync(authHeader);
                var success = new HTTPResponseValue<IEnumerable<OrderResponseDTO>>(orders, StatusResponse.Success, MessageResponse.Success);
                return Ok(success);
            }
            catch
            {
                var error = new HTTPResponseValue<string>(null, StatusResponse.Error, MessageResponse.Error);
                return StatusCode(500, error);
            }
        }

        [HttpGet("{orderId}")]
        [Authorize]
        public async Task<ActionResult<HTTPResponseValue<OrderResponseDTO>>> GetOrderById(Guid orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null) return NotFound(new HTTPResponseValue<string>(null, StatusResponse.NotFound, "Order not found"));
                var success = new HTTPResponseValue<OrderResponseDTO>(order, StatusResponse.Success, MessageResponse.Success);
                return Ok(success);
            }
            catch
            {
                var error = new HTTPResponseValue<string>(null, StatusResponse.Error, MessageResponse.Error);
                return StatusCode(500, error);
            }
        }


        [HttpPost]
        [Authorize]
        public async Task<ActionResult<HTTPResponseValue<CreateOrderResponseDTO>>> CreateOrder([FromBody] CreateOrderRequestDTO request)
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader))
            {
                var resp = new HTTPResponseValue<string>(null, StatusResponse.Unauthorized, MessageResponse.Unauthorized);
                return Unauthorized(resp);
            }

            // Note: userId extraction should be handled in the service layer; controller passes auth header only.

            if (request == null)
            {
                var bad = new HTTPResponseValue<string>(null, StatusResponse.BadRequest, MessageResponse.BadRequest);
                return BadRequest(bad);
            }

            try
            {
                var result = await _orderService.CreateOrderAsync(authHeader!, request);
                var success = new HTTPResponseValue<CreateOrderResponseDTO>(result, StatusResponse.Success, MessageResponse.Success);
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
    }
}
