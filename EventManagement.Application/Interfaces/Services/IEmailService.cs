using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManagement.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string htmlBody);
        Task SendWithInlineImagesAsync(string toEmail, string subject, string htmlBody, IDictionary<string, byte[]> inlineImages);
    }
}
