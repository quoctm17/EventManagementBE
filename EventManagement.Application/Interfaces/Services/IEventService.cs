using System;
using System.Threading.Tasks;
using EventManagement.Application.DTOs.Requests;
using EventManagement.Application.DTOs.Responses;

namespace EventManagement.Application.Interfaces.Services
{
    public interface IEventService
    {
        Task<PagedResult<EventListItemDTO>> GetEventsAsync(EventQueryRequestDTO query);
        Task<EventDetailDTO?> GetEventByIdAsync(Guid eventId);
        Task<List<EventSeatResponseDTO>> GetEventSeatsAsync(Guid eventId);
    }
}
