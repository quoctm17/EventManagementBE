using System;
using System.Collections.Generic;

namespace EventManagement.Application.Interfaces.Services
{
    public interface IQRCodeService
    {
        // Build compact signed payload for a ticket QR
        string BuildTicketPayload(Guid ticketId, Guid orderId, Guid eventId, Guid? attendeeId, DateTime? expiresUtc = null);
        // Generate PNG bytes from a string payload
        byte[] GeneratePng(string payload, int pixelsPerModule = 5);
        // Build data URI suitable for embedding in HTML
        string BuildDataUri(string payload, int pixelsPerModule = 5);
    }
}
