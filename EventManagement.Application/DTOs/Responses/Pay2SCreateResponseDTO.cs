using System.Text.Json.Serialization;

namespace EventManagement.Application.DTOs.Responses
{
    // Typed model for Pay2S create payment API response
    public class Pay2SCreateResponseDTO
    {
        [JsonPropertyName("partnerCode")] public string? PartnerCode { get; set; }
        [JsonPropertyName("requestId")] public string? RequestId { get; set; }
        [JsonPropertyName("orderId")] public string? OrderId { get; set; }
        [JsonPropertyName("amount")] public long? Amount { get; set; }
        [JsonPropertyName("message")] public string? Message { get; set; }
        [JsonPropertyName("resultCode")] public int? ResultCode { get; set; }
        [JsonPropertyName("payUrl")] public string? PayUrl { get; set; }
    }
}
