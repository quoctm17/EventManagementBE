using System.Threading.Tasks;
using EventManagement.Application.DTOs.Responses;
using EventManagement.Application.Interfaces.Services;
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
            var data = await _homeService.GetHomeAsync();
            var response = new HTTPResponseValue<HomeResponseDTO>
            {
                Status = "Success",
                Message = "Home data retrieved",
                Content = data
            };

            return Ok(response);
        }
    }
}
