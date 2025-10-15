using System.Collections.Generic;

namespace EventManagement.Application.DTOs.Responses
{
    public class HomeResponseDTO
    {
        public string HeroTitle { get; set; } = "What event are you looking for today?";
        public string HeroSubtitle { get; set; } = "Find concerts, festivals, conferences and special shows nearby — book tickets in seconds.";
        public IEnumerable<RecommendedEventDTO> RecommendedEvents { get; set; } = new List<RecommendedEventDTO>();
        public IEnumerable<DestinationDTO> Destinations { get; set; } = new List<DestinationDTO>();
        public IEnumerable<string> Categories { get; set; } = new List<string>();
    }
}
