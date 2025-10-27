using System.Threading.Tasks;
using EventManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public SystemController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public class TestEmailRequest
        {
            public string To { get; set; } = string.Empty;
            public string? Subject { get; set; }
            public string? HtmlBody { get; set; }
        }

        [HttpPost("test-email")]
        public async Task<IActionResult> TestEmail([FromBody] TestEmailRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.To))
            {
                return BadRequest(new { error = "To is required" });
            }

            var subject = string.IsNullOrWhiteSpace(req.Subject) ? "Test Email" : req.Subject!;
            var body = string.IsNullOrWhiteSpace(req.HtmlBody) ? "<p>This is a test email from EventManagement.API</p>" : req.HtmlBody!;

            try
            {
                await _emailService.SendAsync(req.To, subject, body);
                return Ok(new { success = true, to = req.To });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
