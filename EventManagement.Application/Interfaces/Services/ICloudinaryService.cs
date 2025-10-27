using System;
using System.Threading.Tasks;

namespace EventManagement.Application.Interfaces.Services
{
    public interface ICloudinaryService
    {
        /// <summary>
        /// Upload raw image bytes to Cloudinary under the given folder and publicId. Returns the secure URL.
        /// </summary>
        /// <param name="imageBytes">Binary content of the image.</param>
        /// <param name="folderPath">Folder path in Cloudinary (e.g., "event-management/tickets/{eventId}/{orderId}").</param>
        /// <param name="publicId">Public ID (file name without extension), e.g., ticketId.</param>
        /// <param name="contentType">MIME type; default image/png.</param>
        /// <returns>Secure URL to the uploaded asset.</returns>
        Task<string?> UploadImageAsync(byte[] imageBytes, string folderPath, string publicId, string contentType = "image/png");

        /// <summary>
        /// Helper to build folder path for a ticket asset using configured base folder.
        /// </summary>
        /// <param name="eventId">Event Id</param>
        /// <param name="orderId">Order Id</param>
        /// <returns>Folder path string</returns>
        string BuildTicketFolder(Guid eventId, Guid orderId);

        /// <summary>
        /// Build the full public id for a ticket QR image under Cloudinary, including folders.
        /// </summary>
        string BuildTicketPublicId(Guid eventId, Guid orderId, Guid ticketId);

        /// <summary>
        /// Delete an image by its public id. Returns true if deleted or not found.
        /// </summary>
        Task<bool> DeleteImageByPublicIdAsync(string publicId);
    }
}
