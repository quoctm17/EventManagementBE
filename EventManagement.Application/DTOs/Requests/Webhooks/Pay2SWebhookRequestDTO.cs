using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventManagement.Application.DTOs.Requests.Webhooks
{
    public class Pay2STransactionWebhookDTO
    {
        // Accept both number or string for id/transactionNumber via JsonElement (no custom converter needed)
        [JsonPropertyName("id")]
        public JsonElement Id { get; set; }
        public string? Gateway { get; set; }
        public string? TransactionDate { get; set; }
        [JsonPropertyName("transactionNumber")]
        public JsonElement TransactionNumber { get; set; }
        public string? AccountNumber { get; set; }
        public string? Content { get; set; }
        public string? TransferType { get; set; } // IN / OUT
        public long TransferAmount { get; set; }
        public string? Checksum { get; set; }
    }

    public class Pay2SWebhookRequestDTO
    {
        public List<Pay2STransactionWebhookDTO> Transactions { get; set; } = new List<Pay2STransactionWebhookDTO>();
    }
}
