using System.Collections.Generic;

namespace EventManagement.Application.DTOs.Responses
{
    public class CheckoutPrepareResponseDTO
    {
        public UserResponseDTO? User { get; set; }
        public EventDetailDTO? Event { get; set; }
        public List<EventSeatResponseDTO> SelectedSeats { get; set; } = new List<EventSeatResponseDTO>();
        public decimal TotalAmount { get; set; }
        public List<PaymentMethodDTO> PaymentMethods { get; set; } = new List<PaymentMethodDTO>();
    }
}
