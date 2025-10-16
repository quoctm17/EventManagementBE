using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EventManagement.Application.DTOs.Requests;
using EventManagement.Application.DTOs.Responses;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Application.Interfaces.Services;

namespace EventManagement.Application.Services
{
    public class EventService : IEventService
    {
    private readonly IEventRepository _eventRepository;
    private readonly IEventSeatMappingRepository _seatMappingRepository;
    private readonly IMapper _mapper;

        public EventService(IEventRepository eventRepository, IEventSeatMappingRepository seatMappingRepository, IMapper mapper)
        {
            _eventRepository = eventRepository;
            _seatMappingRepository = seatMappingRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<EventListItemDTO>> GetEventsAsync(EventQueryRequestDTO query)
        {
            var (items, total) = await _eventRepository.GetPagedEventsAsync(query.Page, query.PageSize, query.Search, query.Province, query.CategoryId, query.Date, query.PriceMin, query.PriceMax, query.SortBy);

            var dtoItems = items.Select(e => _mapper.Map<EventListItemDTO>(e)).ToList();

            // Populate starting prices
            var ids = dtoItems.Select(d => d.EventId).ToList();
            var priceMap = await _seatMappingRepository.GetStartingPricesAsync(ids);
            foreach (var dto in dtoItems)
            {
                if (priceMap.TryGetValue(dto.EventId, out var p)) dto.StartingPrice = p;
            }

            return new PagedResult<EventListItemDTO>
            {
                Items = dtoItems,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling((double)total / query.PageSize)
            };
        }

        public async Task<EventDetailDTO?> GetEventByIdAsync(Guid eventId)
        {
            var e = await _eventRepository.GetEventWithDetailsAsync(eventId);
            if (e == null) return null;

            var dto = _mapper.Map<EventDetailDTO>(e);
            // fill image urls
            dto.ImageUrls = e.EventImages?.Where(i => !string.IsNullOrEmpty(i.ImageUrl)).Select(i => i.ImageUrl!).ToList() ?? new List<string>();
            dto.Categories = e.Categories?.Select(c => c.CategoryName).ToList() ?? new List<string>();

            return dto;
        }
    }
}
