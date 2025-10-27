using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventManagement.Application.DTOs.Responses;

namespace EventManagement.Application.Interfaces.Services
{
    public interface ITicketService
    {
        Task<IEnumerable<TicketsByEventDTO>> GetCurrentUserTicketsAsync(string token);
        Task<IEnumerable<TicketResponseDTO>> GetTicketsByOrderIdAsync(Guid orderId);
    }
}
