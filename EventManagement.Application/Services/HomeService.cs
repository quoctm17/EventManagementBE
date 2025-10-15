using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EventManagement.Application.DTOs.Responses;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Application.Interfaces.Services;

namespace EventManagement.Application.Services
{
    public class HomeService : IHomeService
    {
        private readonly IEventRepository _eventRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public HomeService(IEventRepository eventRepository, ICategoryRepository categoryRepository, IMapper mapper)
        {
            _eventRepository = eventRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<HomeResponseDTO> GetHomeAsync()
        {
            var upcoming = await _eventRepository.GetUpcomingPublishedAsync(6);
            var venues = await _eventRepository.GetPopularDestinationsAsync(4);

            var response = new HomeResponseDTO
            {
                RecommendedEvents = upcoming.Select(e => _mapper.Map<RecommendedEventDTO>(e)).ToList(),
                Destinations = venues.Select(v => _mapper.Map<DestinationDTO>(v)).ToList(),
                Categories = (await _categoryRepository.GetAllAsync()).Select(c => c.CategoryName).ToList()
            };

            // Populate starting prices
            var eventIds = response.RecommendedEvents.Select(r => r.EventId).ToList();
            var priceMap = await _eventRepository.GetStartingPricesAsync(eventIds);
            foreach (var dto in response.RecommendedEvents)
            {
                if (priceMap.TryGetValue(dto.EventId, out var p)) dto.StartingPrice = p;
            }

            return response;
        }
    }
}
