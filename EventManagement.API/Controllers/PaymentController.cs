using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.Constants;
// using EventManagement.Application.DTOs.Requests; // already imported above
using EventManagement.Application.DTOs.Responses;
using EventManagement.Application.DTOs.Requests.Tests;
using Microsoft.Extensions.Configuration;
using EventManagement.Application.DTOs.Requests.Webhooks;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using EventManagement.Application.DTOs.Requests;

namespace EventManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IConfiguration _config;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, IOrderService orderService, IConfiguration config, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _orderService = orderService;
            _config = config;
            _logger = logger;
        }

        [HttpGet("payment-methods")]
        public async Task<ActionResult<HTTPResponseValue<IEnumerable<PaymentMethodDTO>>>> GetPaymentMethods()
        {
            try
            {
                var result = await _paymentService.GetPaymentMethodsAsync(true);
                var success = new HTTPResponseValue<IEnumerable<PaymentMethodDTO>>(result, StatusResponse.Success, MessageResponse.Success);
                return Ok(success);
            }
            catch
            {
                var error = new HTTPResponseValue<string>(null, StatusResponse.Error, MessageResponse.Error);
                return StatusCode(500, error);
            }
        }

        [HttpPost("pay2s/test")]
        public async Task<ActionResult<HTTPResponseValue<Pay2SCreateResponseDTO>>> CreatePay2STest([FromBody] PaymentTestRequestDTO dto)
        {
            try
            {
                var typed = await _paymentService.InitiatePay2SPaymentTestAsync(dto);
                var success = new HTTPResponseValue<Pay2SCreateResponseDTO>(typed, StatusResponse.Success, MessageResponse.Success);
                return Ok(success);
            }
            catch (InvalidOperationException ex)
            {
                var bad = new HTTPResponseValue<string>(null, StatusResponse.BadRequest, ex.Message);
                return BadRequest(bad);
            }
            catch (Exception ex)
            {
                var error = new HTTPResponseValue<string>(ex.Message, StatusResponse.Error, MessageResponse.Error);
                return StatusCode(500, error);
            }
        }

        // Pay2S IPN endpoint: provider posts payment result here.
        // Configure Pay2S:IpnUrl to this route.
        [AllowAnonymous]
        [HttpPost("ipn/pay2s")]
        public async Task<IActionResult> Pay2SIpn([FromBody] Pay2SIpnRequestDTO payload)
        {
            try
            {
                // Optional signature enforcement via config flag
                var verifyToggle = _config["Pay2S:VerifyIpnSignature"]; // "true" to enforce
                var doVerify = string.Equals(verifyToggle, "true", StringComparison.OrdinalIgnoreCase);
                if (doVerify)
                {
                    var providedSig = (payload.Signature ?? payload.M2Signature ?? string.Empty).Trim();
                    if (string.IsNullOrEmpty(providedSig) || !_paymentService.VerifyPay2SIpnSignature(payload, providedSig))
                    {
                        return StatusCode(500, new { success = false });
                    }
                }

                var ok = await _orderService.HandlePay2SIpnAsync(payload);
                if (!ok) return StatusCode(500, new { success = false });
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                // Return 200 with success=false could still lead to retries depending on provider; 500 is safer for retry semantics.
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        // Pay2S Webhook endpoint: must return { "success": true } to stop retries
        // Header: Authorization: Bearer <WebhookToken>
        [HttpPost("webhook/pay2s")]
        public async Task<IActionResult> Pay2SWebhook([FromBody] Pay2SWebhookRequestDTO payload)
        {
            // Verify token
            var auth = Request.Headers["Authorization"].FirstOrDefault() ?? string.Empty;
            var expected = _config["Pay2S:WebhookToken"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(expected))
            {
                // Misconfiguration: don't expose details
                return Unauthorized(new { success = false });
            }

            if (!auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { success = false });
            }
            var provided = auth.Substring("Bearer ".Length).Trim();
            if (!string.Equals(provided, expected, StringComparison.Ordinal))
            {
                return Unauthorized(new { success = false });
            }

            try
            {
                var ok = await _orderService.HandlePay2SWebhookAsync(payload);
                if (!ok) return StatusCode(500, new { success = false });
                return Ok(new { success = true });
            }
            catch
            {
                // For webhook: on failure, still return 200 with success=false would cause retries; but docs require HTTP 200 AND success=true to stop retries
                // So return 500 to trigger retry
                return StatusCode(500, new { success = false });
            }
        }

        // Manual cancel endpoint removed in favor of gateway-return cancel handler

        // Gateway return cancel/failure handler: FE posts params captured from return URL
        [AllowAnonymous]
        [HttpPost("cancel/return")]
        public async Task<IActionResult> GatewayReturnCancel([FromBody] CancelOrderRequestDTO dto)
        {
            try
            {
                _logger.LogInformation("GatewayReturnCancel received: orderId={OrderId}, resultCode={ResultCode}, orderInfo={OrderInfo}", dto.OrderId, dto.ResultCode, dto.OrderInfo);
                var ok = await _orderService.HandleGatewayReturnCancelAsync(dto);
                _logger.LogInformation("GatewayReturnCancel processed: orderId={OrderId}, success={Success}", dto.OrderId, ok);
                return Ok(new { success = ok });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "GatewayReturnCancel invalid operation for orderId={OrderId}", dto?.OrderId);
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch
            {
                _logger.LogError("GatewayReturnCancel failed for orderId={OrderId}", dto?.OrderId);
                return StatusCode(500, new { success = false });
            }
        }

    }
}
