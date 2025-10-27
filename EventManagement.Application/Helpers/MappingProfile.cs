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

            // Role -> RoleResponseDTO
            CreateMap<Role, RoleResponseDTO>();

            // Event -> RecommendedEventDto: most properties map by name automatically.
            CreateMap<Event, RecommendedEventDTO>()
                // EventDate is DateOnly in domain and DateOnly in DTO — AutoMapper will map if types match.
                .ForMember(dest => dest.VenueName, opt => opt.MapFrom(src => src.Venue.VenueName))
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories.Select(c => c.CategoryName)))
                .ForMember(dest => dest.CoverImageUrl, opt => opt.MapFrom(src => src.CoverImageUrl))
                .ForMember(dest => dest.StartingPrice, opt => opt.Ignore());

            CreateMap<Event, EventListItemDTO>()
                .ForMember(dest => dest.VenueName, opt => opt.MapFrom(src => src.Venue.VenueName))
                .ForMember(dest => dest.VenueProvince, opt => opt.MapFrom(src => src.Venue.Province))
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories.Select(c => c.CategoryName)))
                .ForMember(dest => dest.CoverImageUrl, opt => opt.MapFrom(src => src.CoverImageUrl))
                .ForMember(dest => dest.StartingPrice, opt => opt.Ignore());

            CreateMap<Event, EventDetailDTO>()
                .ForMember(dest => dest.VenueName, opt => opt.MapFrom(src => src.Venue.VenueName))
                .ForMember(dest => dest.ImageUrls, opt => opt.Ignore())
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories.Select(c => c.CategoryName)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Organizer, opt => opt.MapFrom(src => src.Organizer))
                .ForMember(dest => dest.TicketTiers, opt => opt.Ignore());

            // Venue -> DestinationDto: map Province -> City and choose main image if any
            CreateMap<Venue, DestinationDTO>()
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Province))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.VenueImages.Where(i => i.IsMain == true).Select(i => i.ImageUrl).FirstOrDefault()));

            // EventSeatMapping -> EventSeatDTO
            CreateMap<EventSeatMapping, EventSeatResponseDTO>()
                .ForMember(dest => dest.SeatId, opt => opt.MapFrom(src => src.Seat.SeatId))
                .ForMember(dest => dest.RowLabel, opt => opt.MapFrom(src => src.Seat.RowLabel))
                .ForMember(dest => dest.SeatNumber, opt => opt.MapFrom(src => src.Seat.SeatNumber))
                .ForMember(dest => dest.TicketCategory, opt => opt.MapFrom(src => src.TicketCategory))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsAvailable != false))
                .ForMember(dest => dest.IsHeld, opt => opt.Ignore());
        }
    }
}
