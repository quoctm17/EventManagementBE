namespace EventManagement.Application.DTOs.Responses
{
    public class PaymentInitResultResponseDTO<T>
    {
        public string RedirectUrl { get; set; } = string.Empty;
        public string? TransactionRef { get; set; }
        public T? ProviderResponse { get; set; }
    }
}
