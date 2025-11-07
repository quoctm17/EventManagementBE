using System;

namespace EventManagement.Application.DTOs.Responses
{
    public class RefundRequestItemDTO
    {
        public Guid RefundRequestItemId { get; set; }
        public Guid TicketId { get; set; }
        public Guid? RefundRuleId { get; set; }
        public decimal? RefundPercentApplied { get; set; }
        public decimal? RefundAmount { get; set; }
    }
}
