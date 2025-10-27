using System;
using System.Text.Json.Serialization;
using EventManagement.Application.Helpers;

namespace EventManagement.Application.DTOs.Requests.Webhooks
{
    // Payload shape observed from Pay2S IPN
    public class Pay2SIpnRequestDTO
    {
        // Fields commonly seen in some samples/specs
        public string? AccessKey { get; set; }
        public long Amount { get; set; }
        public string? ExtraData { get; set; }
        public string? Message { get; set; }
        public string? OrderId { get; set; }
        public string? OrderInfo { get; set; }
        public string? OrderType { get; set; }
        public string? PartnerCode { get; set; }
        public string? PayType { get; set; }
        // requestId in sandbox may arrive as number; accept number or string
        [JsonConverter(typeof(StringOrNumberJsonConverter))]
        public string? RequestId { get; set; }
        public long ResponseTime { get; set; }
        public int ResultCode { get; set; }
        public string? TransId { get; set; }
        // Signature fields: provider variations observed as "signature" or "m2signature"
        public string? M2Signature { get; set; }
        public string? Signature { get; set; }
    }
}
