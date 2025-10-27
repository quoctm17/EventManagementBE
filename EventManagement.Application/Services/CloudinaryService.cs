using System;
using System.IO;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EventManagement.Application.Interfaces.Services;

namespace EventManagement.Application.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly string _baseFolder;
        private readonly string? _uploadPreset;
        private readonly ILogger<CloudinaryService>? _logger;

        public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService>? logger = null)
        {
            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];
            _baseFolder = configuration["Cloudinary:BaseFolder"] ?? "event-management";
            _uploadPreset = configuration["Cloudinary:UploadPreset"];
            _logger = logger;

            if (!string.IsNullOrWhiteSpace(cloudName) && !string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(apiSecret))
            {
                var account = new Account(cloudName, apiKey, apiSecret);
                _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
            }
            else
            {
                _cloudinary = null!; // will throw when used
            }
        }

        public string BuildTicketFolder(Guid eventId, Guid orderId)
        {
            // Structure: base/tickets/eventId/orderId
            return $"{_baseFolder.TrimEnd('/')}/tickets/{eventId:N}/{orderId:N}";
        }

        public string BuildTicketPublicId(Guid eventId, Guid orderId, Guid ticketId)
        {
            return $"{BuildTicketFolder(eventId, orderId)}/{ticketId:N}";
        }

        public async Task<string?> UploadImageAsync(byte[] imageBytes, string folderPath, string publicId, string contentType = "image/png")
        {
            if (_cloudinary == null)
                throw new InvalidOperationException("Cloudinary is not configured");

            if (imageBytes == null || imageBytes.Length == 0) return null;

            using var stream = new MemoryStream(imageBytes);
            // Ensure we don't duplicate folder path inside PublicId; keep only last segment without extension
            var pureId = Path.GetFileNameWithoutExtension(publicId ?? string.Empty);
            if (string.IsNullOrWhiteSpace(pureId)) pureId = Guid.NewGuid().ToString("N");
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription($"{pureId}.png", stream),
                Folder = folderPath,
                PublicId = pureId,
                Overwrite = true,
                UseFilename = false,
                UniqueFilename = false,
                Format = "png"
            };
            if (!string.IsNullOrWhiteSpace(_uploadPreset))
            {
                uploadParams.UploadPreset = _uploadPreset;
            }

            var result = await _cloudinary.UploadAsync(uploadParams);
            if (result.StatusCode == System.Net.HttpStatusCode.OK || result.StatusCode == System.Net.HttpStatusCode.Created)
            {
                return result.SecureUrl?.ToString() ?? result.Url?.ToString();
            }
            try { _logger?.LogError("Cloudinary upload failed: {Message}", result.Error?.Message ?? result.StatusCode.ToString()); } catch { }
            throw new InvalidOperationException($"Cloudinary upload failed: {result.Error?.Message ?? result.StatusCode.ToString()}");
        }

        public async Task<bool> DeleteImageByPublicIdAsync(string publicId)
        {
            if (_cloudinary == null)
                throw new InvalidOperationException("Cloudinary is not configured");

            if (string.IsNullOrWhiteSpace(publicId)) return true;
            var deletionParams = new DeletionParams(publicId) { Invalidate = true };
            var result = await _cloudinary.DestroyAsync(deletionParams);
            // result.Result values: "ok", "not found", etc.
            return string.Equals(result.Result, "ok", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(result.Result, "not found", StringComparison.OrdinalIgnoreCase);
        }
    }
}
