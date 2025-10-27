using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using EventManagement.Application.DTOs.Responses;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Application.Interfaces.Services;

namespace EventManagement.Application.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepo;

        public TicketService(ITicketRepository ticketRepo)
        {
            _ticketRepo = ticketRepo;
        }

        public async Task<IEnumerable<TicketsByEventDTO>> GetCurrentUserTicketsAsync(string token)
        {
            if (string.IsNullOrEmpty(token)) return Enumerable.Empty<TicketsByEventDTO>();
            if (token.StartsWith("Bearer ")) token = token.Substring("Bearer ".Length).Trim();

            System.IdentityModel.Tokens.Jwt.JwtSecurityToken jwtToken;
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                jwtToken = handler.ReadJwtToken(token);
            }
            catch
            {
                return Enumerable.Empty<TicketsByEventDTO>();
            }

            var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            if (subClaim == null || !Guid.TryParse(subClaim.Value, out var userId)) return Enumerable.Empty<TicketsByEventDTO>();

            var tickets = await _ticketRepo.GetTicketsByUserIdAsync(userId);
            // group strictly by EventId to avoid duplicate groups from different Event instances
            var grouped = tickets
                .Where(t => t.EventSeatMapping?.Event != null)
                .GroupBy(t => t.EventId)
                .Select(g =>
                {
                    var sample = g.First();
                    var ev = sample.EventSeatMapping!.Event;
                    return new TicketsByEventDTO
                    {
                        EventId = g.Key,
                        EventName = ev.EventName,
                        EventDate = ev.EventDate,
                        StartTime = ev.StartTime,
                        EndTime = ev.EndTime,
                        CoverImageUrl = ev.CoverImageUrl,
                        VenueName = ev.Venue?.VenueName ?? string.Empty,
                        VenueProvince = ev.Venue?.Province ?? string.Empty,
                        Tickets = g.Select(t => new TicketResponseDTO
                        {
                            TicketId = t.TicketId,
                            OrderId = t.OrderId,
                            EventId = t.EventId,
                            SeatId = t.SeatId,
                            Price = t.Price,
                            AttendeeId = t.AttendeeId,
                            Qrcode = t.Qrcode,
                            PurchaseDate = t.PurchaseDate,
                            Status = t.Status,
                            Additional = t.Additional,
                            TicketCategory = t.EventSeatMapping?.TicketCategory,
                            SeatLabel = (t.EventSeatMapping?.Seat != null) ? ($"{t.EventSeatMapping.Seat.RowLabel}-{t.EventSeatMapping.Seat.SeatNumber}") : null
                        })
                        // optional: order by seat label for stable display
                        .OrderBy(t => t.SeatLabel)
                        .ToList()
                    };
                })
                .OrderBy(e => e.EventDate)
                .ThenBy(e => e.StartTime)
                .ToList();

            return grouped;
        }

        public async Task<IEnumerable<TicketResponseDTO>> GetTicketsByOrderIdAsync(Guid orderId)
        {
            var tickets = await _ticketRepo.GetTicketsByOrderIdAsync(orderId);
            return tickets.Select(t => new TicketResponseDTO
            {
                TicketId = t.TicketId,
                OrderId = t.OrderId,
                EventId = t.EventId,
                SeatId = t.SeatId,
                Price = t.Price,
                AttendeeId = t.AttendeeId,
                Qrcode = t.Qrcode,
                PurchaseDate = t.PurchaseDate,
                Status = t.Status,
                Additional = t.Additional,
                TicketCategory = t.EventSeatMapping?.TicketCategory
            }).ToList();
        }
    }
}
