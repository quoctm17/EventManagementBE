using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.DTOs.Requests;
using EventManagement.Application.DTOs.Requests.Tests;
using EventManagement.Application.DTOs.Responses;
using EventManagement.Application.Interfaces.Repositories;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.Security.Cryptography;
using EventManagement.Application.DTOs.Requests.Webhooks;
using EventManagement.Application.Interfaces;
using EventManagement.Domain.Enums;

namespace EventManagement.Application.Services
{
    // Simplified payment service: supports Pay2S (PayOS-like) and Napas stubs
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _http; // reserved for real API calls
        private readonly IPaymentMethodRepository _paymentMethodRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly IUnitOfWork _uow;

        public PaymentService(
            IConfiguration config,
            IPaymentMethodRepository paymentMethodRepo,
            IPaymentRepository paymentRepo,
            IUnitOfWork uow)
        {
            _config = config;
            _http = new HttpClient();
            _paymentMethodRepo = paymentMethodRepo;
            _paymentRepo = paymentRepo;
            _uow = uow;
        }

        // Test-only method: returns typed Pay2S response directly
        public async Task<Pay2SCreateResponseDTO> InitiatePay2SPaymentTestAsync(PaymentTestRequestDTO dto)
        {
            var endpoint = _config["Pay2S:Sandbox:Endpoint"] ?? "https://sandbox-payment.pay2s.vn/v1/gateway/api/create";
            var partnerCode = _config["Pay2S:PartnerCode"] ?? string.Empty;
            var accessKey = _config["Pay2S:AccessKey"] ?? string.Empty;
            var secretKey = _config["Pay2S:SecretKey"] ?? string.Empty;
            var partnerName = _config["Pay2S:PartnerName"] ?? "EventManagement";
            var ipnUrl = _config["Pay2S:IpnUrl"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(partnerCode) || string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey))
                throw new InvalidOperationException("Pay2S credentials are not configured");

            var orderId = (dto.OrderId ?? Guid.NewGuid());
            var amountLong = (long)Math.Round(dto.Amount);
            var requestId = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            // Prepare orderInfo: 10-32 alphanumeric
            var orderInfo = dto.OrderInfo;
            if (string.IsNullOrWhiteSpace(orderInfo))
            {
                var raw = ($"TT{orderId:N}").Substring(0, Math.Min(16, $"TT{orderId:N}".Length));
                orderInfo = raw.ToUpperInvariant();
            }
            orderInfo = new string(orderInfo.Where(char.IsLetterOrDigit).ToArray());
            if (orderInfo.Length < 10) orderInfo = orderInfo.PadRight(10, 'X');
            else if (orderInfo.Length > 32) orderInfo = orderInfo.Substring(0, 32);

            // Load bank accounts
            var bankAccountsSection = _config.GetSection("Pay2S:BankAccounts");
            var bankAccounts = bankAccountsSection.Exists() ? bankAccountsSection.GetChildren().Select(ch => new
            {
                account_number = ch["account_number"],
                bank_id = ch["bank_id"]
            }).ToArray() : Array.Empty<object>();

            var sigData = $"accessKey={accessKey}&amount={amountLong}&bankAccounts=Array&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={dto.ReturnUrl}&requestId={requestId}&requestType=pay2s";
            var signature = GenerateSignatureCode(sigData, secretKey);

            var payload = new
            {
                accessKey = accessKey,
                partnerCode = partnerCode,
                partnerName = partnerName,
                requestId = requestId,
                amount = amountLong,
                orderId = orderId.ToString(),
                orderInfo = orderInfo,
                orderType = "pay2s",
                bankAccounts = bankAccounts,
                redirectUrl = dto.ReturnUrl,
                ipnUrl = ipnUrl,
                requestType = "pay2s",
                signature = signature
            };

            var json = JsonSerializer.Serialize(payload);
            using var msg = new HttpRequestMessage(HttpMethod.Post, endpoint);
            msg.Headers.Accept.Clear();
            msg.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var resp = await _http.SendAsync(msg);
            var respBody = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Pay2S create failed: {(int)resp.StatusCode} - {respBody}");
            }

            Pay2SCreateResponseDTO? typed;
            try
            {
                typed = JsonSerializer.Deserialize<Pay2SCreateResponseDTO>(respBody);
            }
            catch
            {
                typed = null;
            }

            if (typed == null)
            {
                typed = new Pay2SCreateResponseDTO
                {
                    PartnerCode = partnerCode,
                    RequestId = requestId,
                    OrderId = orderId.ToString(),
                    Amount = amountLong,
                    Message = "Parsed from minimal mapping",
                    ResultCode = 0,
                    PayUrl = null
                };
            }

            return typed;
        }

        public async Task<PaymentInitResultResponseDTO<Pay2SCreateResponseDTO>> InitiatePay2SPaymentAsync(PaymentInitRequest request)
        {
            // Build Pay2S create payment request (sandbox)
            var endpoint = _config["Pay2S:Sandbox:Endpoint"] ?? "https://sandbox-payment.pay2s.vn/v1/gateway/api/create";
            var partnerCode = _config["Pay2S:PartnerCode"] ?? string.Empty;
            var accessKey = _config["Pay2S:AccessKey"] ?? string.Empty;
            var secretKey = _config["Pay2S:SecretKey"] ?? string.Empty;
            var partnerName = _config["Pay2S:PartnerName"] ?? "EventManagement";
            var ipnUrl = _config["Pay2S:IpnUrl"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(partnerCode) || string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey))
                throw new InvalidOperationException("Pay2S credentials are not configured");

            // requestId: unique per request
            var requestId = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var orderId = request.OrderId;
            var amountLong = (long)Math.Round(request.Amount);

            // Generate deterministic orderInfo/transactionRef: 10-32 uppercase alphanumeric
            var gatewayCode = !string.IsNullOrWhiteSpace(request.GatewayKey)
                ? request.GatewayKey
                : (_config["Payments:DefaultGatewayKey"]?.Trim()?.ToUpperInvariant() ?? "PAY2S");
            string orderInfo = GenerateOrderInfo(orderId, gatewayCode);

            // Load bank accounts from config
            var bankAccountsSection = _config.GetSection("Pay2S:BankAccounts");
            var bankAccounts = bankAccountsSection.Exists() ? bankAccountsSection.GetChildren().Select(ch => new
            {
                account_number = ch["account_number"],
                bank_id = ch["bank_id"]
            }).ToArray() : Array.Empty<object>();

            // Build signature string (use 'Array' literal for bankAccounts per docs)
            var sigData = $"accessKey={accessKey}&amount={amountLong}&bankAccounts=Array&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={request.ReturnUrl}&requestId={requestId}&requestType=pay2s";
            var signature = GenerateSignatureCode(sigData, secretKey);

            var payload = new
            {
                accessKey = accessKey,
                partnerCode = partnerCode,
                partnerName = partnerName,
                requestId = requestId,
                amount = amountLong,
                orderId = orderId.ToString(),
                orderInfo = orderInfo,
                orderType = "pay2s",
                bankAccounts = bankAccounts,
                redirectUrl = request.ReturnUrl,
                ipnUrl = ipnUrl,
                requestType = "pay2s",
                signature = signature
            };

            var json = JsonSerializer.Serialize(payload);
            using var msg = new HttpRequestMessage(HttpMethod.Post, endpoint);
            msg.Headers.Accept.Clear();
            msg.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var resp = await _http.SendAsync(msg);
            var respBody = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Pay2S create failed: {(int)resp.StatusCode} - {respBody}");
            }

            // Deserialize typed provider response
            Pay2SCreateResponseDTO? provider = null;
            try { provider = JsonSerializer.Deserialize<Pay2SCreateResponseDTO>(respBody); } catch { }
            var redirectUrl = provider?.PayUrl ?? request.ReturnUrl ?? string.Empty;

            return new PaymentInitResultResponseDTO<Pay2SCreateResponseDTO>
            {
                RedirectUrl = redirectUrl,
                TransactionRef = orderInfo,
                ProviderResponse = provider
            };
        }

        public Task<PaymentInitResultResponseDTO<object>> InitiateNapasPaymentAsync(PaymentInitRequest request)
        {
            // Stub for NAPAS; to be implemented later.
            // For now, mirror Pay2S-style to keep FE flow unblocked (different base path)
            var baseUrl = _config["Napas:SandboxBaseUrl"] ?? "https://sandbox.napas.vn/checkout";
            var amount = request.Amount.ToString("0.##");
            var redirect = $"{baseUrl}?orderId={request.OrderId}&amount={amount}" +
                           (string.IsNullOrEmpty(request.ReturnUrl) ? string.Empty : $"&returnUrl={Uri.EscapeDataString(request.ReturnUrl)}") +
                           (string.IsNullOrEmpty(request.CancelUrl) ? string.Empty : $"&cancelUrl={Uri.EscapeDataString(request.CancelUrl)}");

            var txnRef = $"NAPAS-{request.OrderId}";
            return Task.FromResult(new PaymentInitResultResponseDTO<object>
            {
                RedirectUrl = redirect,
                TransactionRef = txnRef
            });
        }

        public async Task<IEnumerable<PaymentMethodDTO>> GetPaymentMethodsAsync(bool onlyActive = true)
        {
            var methods = await _paymentMethodRepo.GetAllAsync();
            var list = methods
                .Where(m => !onlyActive || m.IsActive != false)
                .Select(m => new PaymentMethodDTO
                {
                    PaymentMethodId = m.PaymentMethodId,
                    MethodName = m.MethodName,
                    Provider = m.Provider,
                    IsActive = m.IsActive != false
                })
                .ToList();
            return list;
        }

        private static string GenerateSignatureCode(string data, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash) sb.AppendFormat("{0:x2}", b);
            return sb.ToString();
        }

        // Public verify used by controller to validate IPN signature
        public bool VerifyPay2SIpnSignature(Pay2SIpnRequestDTO payload, string providedSignature)
        {
            if (payload == null || string.IsNullOrWhiteSpace(providedSignature)) return false;
            var secret = _config["Pay2S:SecretKey"];
            if (string.IsNullOrWhiteSpace(secret)) return false;

            // Alphabetically sort all non-null fields except signature fields
            var dict = new Dictionary<string, string?>
            {
                ["accessKey"] = payload.AccessKey,
                ["amount"] = payload.Amount.ToString(),
                ["extraData"] = payload.ExtraData,
                ["message"] = payload.Message,
                ["orderId"] = payload.OrderId,
                ["orderInfo"] = payload.OrderInfo,
                ["orderType"] = payload.OrderType,
                ["partnerCode"] = payload.PartnerCode,
                ["payType"] = payload.PayType,
                ["requestId"] = payload.RequestId,
                ["responseTime"] = payload.ResponseTime > 0 ? payload.ResponseTime.ToString() : null,
                ["resultCode"] = payload.ResultCode.ToString(),
                ["transId"] = payload.TransId
            };
            var kv = dict
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .OrderBy(kv => kv.Key, StringComparer.Ordinal)
                .Select(kv => $"{kv.Key}={kv.Value}");
            var raw = string.Join('&', kv);
            var calc = GenerateSignatureCode(raw, secret);
            return calc.Equals(providedSignature, StringComparison.OrdinalIgnoreCase);
        }

        private static string GenerateOrderInfo(Guid orderId, string gatewayCode)
        {
            // Base: EM + gatewayCode (e.g., P2S) + first 15 of GUID (N format)
            var baseId = orderId.ToString("N").ToUpperInvariant();
            var prefix = "EM" + new string(gatewayCode.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
            var core = baseId.Substring(0, Math.Min(15, baseId.Length));
            var orderInfo = (prefix + core).ToUpperInvariant();
            // Ensure 10-32 length by padding or trimming (should be ~20 already)
            if (orderInfo.Length < 10) orderInfo = orderInfo.PadRight(10, 'X');
            if (orderInfo.Length > 32) orderInfo = orderInfo.Substring(0, 32);
            return orderInfo;
        }

        // Webhook handling removed; now handled by OrderService to avoid circular dependency

    }
}
