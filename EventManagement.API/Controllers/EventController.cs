using System;
using System.Threading.Tasks;
using EventManagement.Application.DTOs.Requests;
using EventManagement.Application.DTOs.Responses;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.Constants;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet]
        public async Task<ActionResult<HTTPResponseValue<PagedResult<EventListItemDTO>>>> Get([FromQuery] EventQueryRequestDTO query)
        {
            if (query == null)
            {
                var bad = new HTTPResponseValue<string>(null, StatusResponse.BadRequest, MessageResponse.BadRequest);
                return BadRequest(bad);
            }

            try
            {
                var data = await _eventService.GetEventsAsync(query);
                var response = new HTTPResponseValue<PagedResult<EventListItemDTO>>(data, StatusResponse.Success, MessageResponse.Success);
                return Ok(response);
            }
            catch (System.Exception)
            {
                var error = new HTTPResponseValue<string>(null, StatusResponse.Error, MessageResponse.Error);
                return StatusCode(500, error);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HTTPResponseValue<EventDetailDTO>>> GetById(Guid id)
        {
            if (id == Guid.Empty)
            {
                var bad = new HTTPResponseValue<string>(null, StatusResponse.BadRequest, MessageResponse.BadRequest);
                return BadRequest(bad);
            }

            try
            {
                var data = await _eventService.GetEventByIdAsync(id);
                if (data == null) return NotFound(new HTTPResponseValue<string>(null, StatusResponse.NotFound, MessageResponse.NotFound));

                var response = new HTTPResponseValue<EventDetailDTO>(data, StatusResponse.Success, MessageResponse.Success);
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                var error = new HTTPResponseValue<string>(null, StatusResponse.Error, MessageResponse.Error);
                return StatusCode(500, error);
            }
        }
    }
}
