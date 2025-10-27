using System.Threading.Tasks;
using System.Collections.Generic;
using EventManagement.Application.DTOs.Requests;
using EventManagement.Application.DTOs.Requests.Tests;
using EventManagement.Application.DTOs.Responses;
using EventManagement.Application.DTOs.Requests.Webhooks;

namespace EventManagement.Application.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<PaymentInitResultResponseDTO<Pay2SCreateResponseDTO>> InitiatePay2SPaymentAsync(PaymentInitRequest request);
        Task<PaymentInitResultResponseDTO<object>> InitiateNapasPaymentAsync(PaymentInitRequest request);
        Task<IEnumerable<PaymentMethodDTO>> GetPaymentMethodsAsync(bool onlyActive = true);
        Task<Pay2SCreateResponseDTO> InitiatePay2SPaymentTestAsync(PaymentTestRequestDTO request);
        // Verify Pay2S IPN signature using HMAC-SHA256 with alphabetically sorted key=value pairs
        bool VerifyPay2SIpnSignature(Pay2SIpnRequestDTO payload, string providedSignature);
    }
}
