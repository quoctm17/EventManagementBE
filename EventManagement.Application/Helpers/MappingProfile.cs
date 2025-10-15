using AutoMapper;
using EventManagement.Domain.Models;
using EventManagement.Application.DTOs.Responses;

namespace EventManagement.Application.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserResponseDTO>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore());

            // Event -> RecommendedEventDto: most properties map by name automatically.
            CreateMap<Event, RecommendedEventDTO>()
                // EventDate is DateOnly in domain and DateOnly in DTO — AutoMapper will map if types match.
                .ForMember(dest => dest.VenueName, opt => opt.MapFrom(src => src.Venue.VenueName))
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories.Select(c => c.CategoryName)))
                .ForMember(dest => dest.CoverImageUrl, opt => opt.MapFrom(src => src.CoverImageUrl))
                .ForMember(dest => dest.StartingPrice, opt => opt.Ignore());

            // Venue -> DestinationDto: map Province -> City and choose main image if any
            CreateMap<Venue, DestinationDTO>()
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Province))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.VenueImages.Where(i => i.IsMain == true).Select(i => i.ImageUrl).FirstOrDefault()));
        }
    }
}
