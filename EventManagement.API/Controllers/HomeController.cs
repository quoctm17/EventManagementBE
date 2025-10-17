using System.Threading.Tasks;
using EventManagement.Application.DTOs.Responses;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.Constants;
using Microsoft.AspNetCore.Mvc; 

namespace EventManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        [HttpGet]
        public async Task<ActionResult<HTTPResponseValue<HomeResponseDTO>>> Get()
        {
            try
            {
                var data = await _homeService.GetHomeAsync();
                var response = new HTTPResponseValue<HomeResponseDTO>
                {
                    Status = StatusResponse.Success,
                    Message = MessageResponse.Success,
                    Content = data
                };

                return Ok(response);
            }
            catch (System.Exception)
            {
                var error = new HTTPResponseValue<string>(null, StatusResponse.Error, MessageResponse.Error);
                return StatusCode(500, error);
            }
        }
    }
}
