using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EventManagement.Application.DTOs.Requests;
using EventManagement.Application.DTOs.Responses;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Application.Interfaces.Services;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace EventManagement.Application.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IEventSeatMappingRepository _seatMappingRepository;
        private readonly IMapper _mapper;
        private readonly ICategoryRepository _categoryRepository;
    private readonly ISeatHoldRepository _seatHoldRepository;
    private readonly IConfiguration _config;

        public EventService(IEventRepository eventRepository, IEventSeatMappingRepository seatMappingRepository, ICategoryRepository categoryRepository, ISeatHoldRepository seatHoldRepository, IMapper mapper, IConfiguration config)
        {
            _eventRepository = eventRepository;
            _seatMappingRepository = seatMappingRepository;
            _categoryRepository = categoryRepository;
            _seatHoldRepository = seatHoldRepository;
            _mapper = mapper;
            _config = config;
        }

        public async Task<PagedResult<EventListItemDTO>> GetEventsAsync(EventQueryRequestDTO query)
        {
            var (items, total) = await _eventRepository.GetPagedEventsAsync(query.Page, query.PageSize, query.Search, query.Province, query.CategoryIds, query.Date, query.PriceMin, query.PriceMax, query.SortBy);

            var dtoItems = items.Select(e => _mapper.Map<EventListItemDTO>(e)).ToList();

            // Compute OrderPendingExpires based on config (Reservation:OrderPendingExpiresMinutes)
            var pendingMinutes = 10;
            var raw = _config["Reservation:OrderPendingExpiresMinutes"];
            if (int.TryParse(raw, out var conf) && conf > 0) pendingMinutes = conf;
            var nowUtc = DateTime.UtcNow;
            foreach (var d in dtoItems)
            {
                d.OrderPendingExpires = nowUtc.AddMinutes(pendingMinutes);
            }

            // Populate starting prices
            var ids = dtoItems.Select(d => d.EventId).ToList();
            var priceMap = await _seatMappingRepository.GetStartingPricesAsync(ids);
            foreach (var dto in dtoItems)
            {
                if (priceMap.TryGetValue(dto.EventId, out var p)) dto.StartingPrice = p;
            }

            // Collect available categories from Category repository (all categories in DB)
            var categories = await _categoryRepository.GetAllCategoryOptionsAsync();

            // Collect available provinces using an optimized repository method
            var provinces = await _eventRepository.GetAllProvincesAsync();

            return new PagedResult<EventListItemDTO>
            {
                Items = dtoItems,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling((double)total / query.PageSize),
                AvailableCategories = categories,
                AvailableProvinces = provinces
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
            // Organizer mapping is handled by AutoMapper. Ensure Organizer is not null when available.
            if (e.Organizer != null)
            {
                // Organizer already mapped into dto. No-op here, but keep for clarity.
            }

            // Build aggregated ticket tiers from EventSeatMappings
            if (e.EventSeatMappings != null && e.EventSeatMappings.Count > 0)
            {
                var tiers = e.EventSeatMappings
                    .Where(m => m != null)
                    .GroupBy(m => m.TicketCategory)
                    .Select(g => new TicketTierDTO
                    {
                        TicketCategory = g.Key,
                        Price = g.Min(x => x.Price),
                        AvailableSeats = g.Count(x => x.IsAvailable != false)
                    })
                    .OrderBy(t => t.Price)
                    .ToList();

                dto.TicketTiers = tiers;
            }

            // Compute OrderPendingExpires for detail
            var pendingMinutes = 10;
            var raw = _config["Reservation:OrderPendingExpiresMinutes"];
            if (int.TryParse(raw, out var conf) && conf > 0) pendingMinutes = conf;
            dto.OrderPendingExpires = DateTime.UtcNow.AddMinutes(pendingMinutes);

            return dto;
        }

        public async Task<List<EventSeatResponseDTO>> GetEventSeatsAsync(Guid eventId)
        {
            var mappings = await _seatMappingRepository.GetByEventIdWithSeatAsync(eventId);
            var seats = mappings
                .Where(m => m.Seat != null)
                .Select(m => _mapper.Map<EventSeatResponseDTO>(m))
                .OrderBy(s => s.RowLabel)
                .ThenBy(s => s.SeatNumber)
                .ToList();

            if (seats.Count == 0) return seats;

            // Compute IsHeld only for seats that are available
            var nowUtc = DateTime.UtcNow;
            var activeHeldSeatIds = await _seatHoldRepository.GetActiveHeldSeatIdsAsync(eventId, nowUtc);

            foreach (var seat in seats)
            {
                seat.IsHeld = seat.IsAvailable && activeHeldSeatIds.Contains(seat.SeatId);
            }
            return seats;
        }
    }
}
