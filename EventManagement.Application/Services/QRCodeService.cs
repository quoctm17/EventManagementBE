using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using EventManagement.Application.Interfaces.Services;
using QRCoder;

namespace EventManagement.Application.Services
{
    public class QRCodeService : IQRCodeService
    {
        private readonly IConfiguration _config;
        public QRCodeService(IConfiguration config)
        {
            _config = config;
        }

        public string BuildTicketPayload(Guid ticketId, Guid orderId, Guid eventId, Guid? attendeeId, DateTime? expiresUtc = null)
        {
            // Schema: EMQR|1|{ticketId:N}|{eventId:N}|{expUnix}|{sigHex}
            var version = 1;
            var exp = expiresUtc ?? DateTime.UtcNow.AddMinutes(GetExpireMinutes());
            var expUnix = new DateTimeOffset(exp).ToUnixTimeSeconds();
            var baseStr = $"{ticketId:N}.{eventId:N}.{expUnix}";
            var secret = _config["Qr:SecretKey"] ?? string.Empty;
            var sig = ComputeHmacHex(secret, baseStr);
            return $"EMQR|{version}|{ticketId:N}|{eventId:N}|{expUnix}|{sig}";
        }

        public byte[] GeneratePng(string payload, int pixelsPerModule = 5)
        {
            using var generator = new QRCodeGenerator();
            var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
            var pngQr = new PngByteQRCode(data);
            return pngQr.GetGraphic(pixelsPerModule);
        }

        public string BuildDataUri(string payload, int pixelsPerModule = 5)
        {
            var png = GeneratePng(payload, pixelsPerModule);
            var b64 = Convert.ToBase64String(png);
            return $"data:image/png;base64,{b64}";
        }

        private int GetExpireMinutes()
        {
            var raw = _config["Qr:TokenExpiresMinutes"]; // optional override
            if (int.TryParse(raw, out var v) && v > 0) return v;
            return 60 * 24; // default 24h
        }

        private static string ComputeHmacHex(string secret, string data)
        {
            if (string.IsNullOrEmpty(secret)) return string.Empty;
            using var h = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var bytes = h.ComputeHash(Encoding.UTF8.GetBytes(data));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
