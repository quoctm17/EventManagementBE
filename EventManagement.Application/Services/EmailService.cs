using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EventManagement.Application.Interfaces.Services;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace EventManagement.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            await SendWithInlineImagesAsync(toEmail, subject, htmlBody, new Dictionary<string, byte[]>());
        }

        public async Task SendWithInlineImagesAsync(string toEmail, string subject, string htmlBody, IDictionary<string, byte[]> inlineImages)
        {
            var message = new MimeMessage();
            var fromEmail = _config["Smtp:FromEmail"] ?? "no-reply@example.com";
            var fromName = _config["Smtp:FromName"] ?? "EventManagement";
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };

            // Convert inline images to attachments with content-id and reference them in html if placeholders present
            foreach (var kv in inlineImages)
            {
                var img = builder.LinkedResources.Add($"{kv.Key}.png", kv.Value);
                img.ContentId = kv.Key;
            }

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            var host = _config["Smtp:Host"] ?? "localhost";
            var portStr = _config["Smtp:Port"];
            var enableSsl = string.Equals(_config["Smtp:EnableSsl"], "true", StringComparison.OrdinalIgnoreCase);
            var port = int.TryParse(portStr, out var p) ? p : (enableSsl ? 465 : 25);

            // Determine TLS mode: if 587 + EnableSsl => StartTls; if 465 + EnableSsl => SslOnConnect; else Auto
            var security = SecureSocketOptions.Auto;
            if (enableSsl)
            {
                security = port == 587 ? SecureSocketOptions.StartTls : SecureSocketOptions.SslOnConnect;
            }

            var user = _config["Smtp:User"];
            try
            {
                await client.ConnectAsync(host, port, security);

                var pass = _config["Smtp:Password"];
                if (!string.IsNullOrEmpty(user))
                {
                    await client.AuthenticateAsync(user, pass);
                }

                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email: failed to send to {To} via {Host}:{Port} (security={Security})", toEmail, host, port, security);
                throw;
            }
            finally
            {
                try
                {
                    await client.DisconnectAsync(true);
                }
                catch (Exception dex)
                {
                    _logger.LogWarning(dex, "Email: error while disconnecting SMTP client");
                }
            }
        }
    }
}
