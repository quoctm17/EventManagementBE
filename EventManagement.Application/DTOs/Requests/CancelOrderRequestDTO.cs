using System;

namespace EventManagement.Application.DTOs.Requests
{
    public class CancelOrderRequestDTO
    {
        public Guid OrderId { get; set; }
        public string? AccessKey { get; set; }
        public string? Amount { get; set; }
        public string? Message { get; set; }
        public string? OrderInfo { get; set; }
        public string? OrderType { get; set; }
        public string? PartnerCode { get; set; }
        public string? PayType { get; set; }
        public string? RequestId { get; set; }
        public string? ResponseTime { get; set; }
        public string? ResultCode { get; set; }
        public string? M2Signature { get; set; }
    }
}
