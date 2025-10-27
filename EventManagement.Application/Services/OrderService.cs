using AutoMapper;
using EventManagement.Application.DTOs.Responses;
using EventManagement.Application.DTOs.Requests;
using EventManagement.Application.DTOs.Requests.Webhooks;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.Interfaces;
using EventManagement.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventManagement.Domain.Enums;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace EventManagement.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly ITicketRepository _ticketRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly IPaymentMethodRepository _paymentMethodRepo;
        private readonly IEventSeatMappingRepository _seatMappingRepo;
        private readonly ISeatHoldRepository _seatHoldRepo;
        private readonly IPaymentService _paymentService;
        private readonly IUnitOfWork _uow;
        private readonly IQRCodeService _qrService;
        private readonly IEmailService _emailService;
        private readonly IUserRepository _userRepo;
        private readonly IConfiguration _configuration;
    private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepo,
            ITicketRepository ticketRepo,
            IPaymentRepository paymentRepo,
            IPaymentMethodRepository paymentMethodRepo,
            IEventSeatMappingRepository seatMappingRepo,
            ISeatHoldRepository seatHoldRepo,
            IPaymentService paymentService,
            IUnitOfWork uow,
            IConfiguration configuration,
            IQRCodeService qrService,
            IEmailService emailService,
            IUserRepository userRepo,
            ILogger<OrderService> logger)
        {
            _orderRepo = orderRepo;
            _ticketRepo = ticketRepo;
            _paymentRepo = paymentRepo;
            _paymentMethodRepo = paymentMethodRepo;
            _seatMappingRepo = seatMappingRepo;
            _seatHoldRepo = seatHoldRepo;
            _paymentService = paymentService;
            _uow = uow;
            _configuration = configuration;
            _qrService = qrService;
            _emailService = emailService;
            _userRepo = userRepo;
            _logger = logger;
        }

        public async Task<IEnumerable<OrderResponseDTO>> GetCurrentUserOrdersAsync(string token)
        {
            if (string.IsNullOrEmpty(token)) return Enumerable.Empty<OrderResponseDTO>();
            if (token.StartsWith("Bearer ")) token = token.Substring("Bearer ".Length).Trim();

            System.IdentityModel.Tokens.Jwt.JwtSecurityToken jwtToken;
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                jwtToken = handler.ReadJwtToken(token);
            }
            catch
            {
                return Enumerable.Empty<OrderResponseDTO>();
            }

            var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            if (subClaim == null || !Guid.TryParse(subClaim.Value, out var userId)) return Enumerable.Empty<OrderResponseDTO>();

            var orders = await _orderRepo.GetOrdersWithDetailsByUserIdAsync(userId);
            var dtos = orders.Select(o => new OrderResponseDTO
            {
                OrderId = o.OrderId,
                UserId = o.UserId,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                Additional = o.Additional,
                Tickets = o.Tickets?.Select(t => new TicketResponseDTO
                {
                    TicketId = t.TicketId,
                    OrderId = t.OrderId,
                    EventId = t.EventId,
                    SeatId = t.SeatId,
                    Price = t.Price,
                    AttendeeId = t.AttendeeId,
                    Qrcode = t.Qrcode,
                    PurchaseDate = t.PurchaseDate,
                    Status = t.Status,
                    Additional = t.Additional,
                    TicketCategory = t.EventSeatMapping?.TicketCategory
                }).ToList() ?? new List<TicketResponseDTO>(),
                Payments = o.Payments?.Select(p => new { p.PaymentId, p.Amount, p.Status, p.TransactionDate, p.TransactionRef }).ToList<object>() ?? new List<object>()
            }).ToList();

            return dtos;
        }

        public async Task<OrderResponseDTO?> GetOrderByIdAsync(Guid orderId)
        {
            var o = await _orderRepo.GetOrderWithDetailsAsync(orderId);
            if (o == null) return null;

            var dto = new OrderResponseDTO
            {
                OrderId = o.OrderId,
                UserId = o.UserId,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                Additional = o.Additional,
                Tickets = o.Tickets?.Select(t => new TicketResponseDTO
                {
                    TicketId = t.TicketId,
                    OrderId = t.OrderId,
                    EventId = t.EventId,
                    SeatId = t.SeatId,
                    Price = t.Price,
                    AttendeeId = t.AttendeeId,
                    Qrcode = t.Qrcode,
                    PurchaseDate = t.PurchaseDate,
                    Status = t.Status,
                    Additional = t.Additional,
                    TicketCategory = t.EventSeatMapping?.TicketCategory
                }).ToList() ?? new List<TicketResponseDTO>(),
                Payments = o.Payments?.Select(p => new { p.PaymentId, p.Amount, p.Status, p.TransactionDate, p.TransactionRef }).ToList<object>() ?? new List<object>()
            };

            return dto;
        }

        public async Task<CreateOrderResponseDTO> CreateOrderAsync(string authHeader, CreateOrderRequestDTO request)
        {
            // Extract user id from JWT
            if (string.IsNullOrEmpty(authHeader)) throw new InvalidOperationException("Missing auth token");
            var token = authHeader.StartsWith("Bearer ") ? authHeader.Substring("Bearer ".Length).Trim() : authHeader;
            System.IdentityModel.Tokens.Jwt.JwtSecurityToken jwtToken;
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                jwtToken = handler.ReadJwtToken(token);
            }
            catch
            {
                throw new InvalidOperationException("Invalid token");
            }
            var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            if (subClaim == null || !Guid.TryParse(subClaim.Value, out var userGuid)) throw new InvalidOperationException("Invalid token");

            // Validate payment method
            var pm = await _paymentMethodRepo.GetByIdAsync(request.PaymentMethodId);
            if (pm == null) throw new InvalidOperationException("Payment method not found");
            if (pm.IsActive == false) throw new InvalidOperationException("Payment method is inactive");

            // Validate seats
            if (request.SeatIds == null || request.SeatIds.Count == 0)
                throw new InvalidOperationException("No seats selected");

            var mappings = await _seatMappingRepo.WhereAsync(m => m.EventId == request.EventId && request.SeatIds.Contains(m.SeatId));
            var mappingList = mappings.ToList();
            if (mappingList.Count != request.SeatIds.Count)
                throw new InvalidOperationException("Some seats do not exist for this event");

            // Check availability and compute total
            var unavailable = mappingList.Where(m => m.IsAvailable == false).Select(m => m.SeatId).ToList();
            if (unavailable.Any()) throw new InvalidOperationException("Some seats are no longer available");

            var total = mappingList.Sum(m => m.Price);

            await _uow.BeginTransactionAsync();
            try
            {
                // Create Order
                var order = new Order
                {
                    OrderId = Guid.NewGuid(),
                    UserId = userGuid,
                    TotalAmount = total,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };
                await _orderRepo.AddAsync(order);

                // Create Tickets and mark seats unavailable
                foreach (var m in mappingList)
                {
                    var ticket = new Ticket
                    {
                        TicketId = Guid.NewGuid(),
                        OrderId = order.OrderId,
                        EventId = m.EventId,
                        SeatId = m.SeatId,
                        Price = m.Price,
                        AttendeeId = userGuid,
                        Status = "Reserved"
                    };
                    await _ticketRepo.AddAsync(ticket);

                    // mark seat not available
                    m.IsAvailable = false;
                    await _seatMappingRepo.UpdateAsync(m);
                }

                // Remove soft seat holds for these seats since Order now owns them
                var holds = await _seatHoldRepo.WhereAsync(h => h.EventId == request.EventId && request.SeatIds.Contains(h.SeatId));
                foreach (var hold in holds)
                {
                    await _seatHoldRepo.DeleteAsync(hold.HoldId);
                }

                // Create Payment record (Pending)
                var payment = new Payment
                {
                    PaymentId = Guid.NewGuid(),
                    OrderId = order.OrderId,
                    PaymentMethodId = pm.PaymentMethodId,
                    Amount = total,
                    Status = "Pending",
                    TransactionDate = null
                };
                await _paymentRepo.AddAsync(payment);

                // Resolve gatewayKey strictly from DB (PaymentMethod.GatewayKey)
                string? redirectUrl = null;
                var gatewayKey = pm.GatewayKey?.Trim()?.ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(gatewayKey))
                {
                    throw new InvalidOperationException("Payment method gateway is not configured");
                }

                switch (gatewayKey)
                {
                    case "PAY2S":
                        {
                            var init = await _paymentService.InitiatePay2SPaymentAsync(new PaymentInitRequest
                            {
                                OrderId = order.OrderId,
                                Amount = total,
                                ReturnUrl = request.ReturnUrl,
                                CancelUrl = request.CancelUrl,
                                Description = $"Payment for order {order.OrderId}",
                                GatewayKey = gatewayKey
                            });
                            redirectUrl = init.RedirectUrl;
                            payment.TransactionRef = init.TransactionRef;

                            // Set order pending expiry time based on configuration
                            var pendingMinutes = 10;
                            var rawMinutes = _configuration["Reservation:OrderPendingExpiresMinutes"];
                            if (int.TryParse(rawMinutes, out var confMinutes) && confMinutes > 0)
                            {
                                pendingMinutes = confMinutes;
                            }
                            order.OrderPendingExpires = DateTime.UtcNow.AddMinutes(pendingMinutes);
                            break;
                        }
                    case "BANK_TRANSFER":
                        {
                            // not implemented yet; keep status Pending and no redirect
                            break;
                        }
                    default:
                        throw new InvalidOperationException("Unsupported payment method");
                }

                // Ensure all changes are persisted before commit
                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();

                return new CreateOrderResponseDTO
                {
                    OrderId = order.OrderId,
                    PaymentMethodId = pm.PaymentMethodId,
                    TotalAmount = total,
                    OrderStatus = order.Status,
                    PaymentStatus = payment.Status,
                    PaymentRedirectUrl = redirectUrl,
                    PaymentId = payment.PaymentId,
                    TransactionRef = payment.TransactionRef
                };
            }
            catch (DbUpdateConcurrencyException)
            {
                await _uow.RollbackTransactionAsync();
                throw new InvalidOperationException("One or more selected seats were just reserved by another order. Please refresh and select available seats.");
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw;
            }
        }

        // Handle Pay2S webhook
        public async Task<bool> HandlePay2SWebhookAsync(Pay2SWebhookRequestDTO payload)
        {
            if (payload?.Transactions == null || payload.Transactions.Count == 0)
                return true;

            var regex = new System.Text.RegularExpressions.Regex(@"EM[A-Z0-9]{8,30}", System.Text.RegularExpressions.RegexOptions.Compiled);

            foreach (var tx in payload.Transactions)
            {
                var type = (tx.TransferType ?? string.Empty).Trim().ToUpperInvariant();
                if (type != "IN" && type != "OUT") continue;

                var content = (tx.Content ?? string.Empty).ToUpperInvariant();
                var match = regex.Match(content);
                if (!match.Success) continue;

                var refCode = match.Value;
                var payment = await _paymentRepo.SingleOrDefaultAsync(p => p.TransactionRef == refCode);
                if (payment is null) continue;

                var orderId = payment.OrderId;
                DateTime parsedDate;
                if (!string.IsNullOrWhiteSpace(tx.TransactionDate) && DateTime.TryParse(tx.TransactionDate, out var dt)) parsedDate = dt.ToUniversalTime();
                else parsedDate = DateTime.UtcNow;

                var additionalObj = new
                {
                    provider = "Pay2S",
                    txId = tx.Id,
                    txNumber = tx.TransactionNumber,
                    transferType = tx.TransferType,
                    transferAmount = tx.TransferAmount
                };
                var addJson = System.Text.Json.JsonSerializer.Serialize(additionalObj);

                if (type == "IN")
                {
                    var expected = (long)Math.Round(payment.Amount);
                    if (tx.TransferAmount >= expected)
                    {
                        payment.Status = PaymentStatus.Success;
                        payment.TransactionDate = parsedDate;
                        payment.Additional = addJson;
                        await _paymentRepo.UpdateAsync(payment);

                        // Inline: Mark order paid + issue tickets
                        var order = await _orderRepo.GetByIdAsync(orderId);
                        if (order != null)
                        {
                            order.Status = OrderStatus.Paid;
                            order.OrderPendingExpires = null;
                            await _orderRepo.UpdateAsync(order);

                            var tickets = await _ticketRepo.GetTicketsByOrderIdAsync(orderId);
                            foreach (var t in tickets)
                            {
                                t.Status = TicketStatus.Issued;
                                t.PurchaseDate = parsedDate;
                                var qrPayload = _qrService.BuildTicketPayload(t.TicketId, t.OrderId, t.EventId, t.AttendeeId, null);
                                t.Qrcode = qrPayload;
                                await _ticketRepo.UpdateAsync(t);
                            }
                            // Persist changes before sending email so query sees Issued tickets
                            await _uow.SaveChangesAsync();
                            await TrySendTicketsEmailAsync(orderId);
                        }
                    }
                }
                else if (type == "OUT")
                {
                    payment.Status = PaymentStatus.Failed;
                    payment.TransactionDate = parsedDate;
                    payment.Additional = addJson;
                    await _paymentRepo.UpdateAsync(payment);

                    // Inline: Mark order cancelled + cancel tickets
                    var order = await _orderRepo.GetByIdAsync(orderId);
                    if (order != null)
                    {
                        order.Status = OrderStatus.Cancelled;
                        order.OrderPendingExpires = null;
                        await _orderRepo.UpdateAsync(order);

                        var tickets = await _ticketRepo.GetTicketsByOrderIdAsync(orderId);
                        foreach (var t in tickets)
                        {
                            t.Status = TicketStatus.Cancelled;
                            await _ticketRepo.UpdateAsync(t);
                        }
                    }
                }

                await _uow.SaveChangesAsync();
            }

            return true;
        }

        // Handle Pay2S IPN (gateway callback)
        public async Task<bool> HandlePay2SIpnAsync(Pay2SIpnRequestDTO payload)
        {
            if (payload == null) return true; // nothing to do

            var refCode = (payload.OrderInfo ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(refCode)) return true;

            var payment = await _paymentRepo.SingleOrDefaultAsync(p => p.TransactionRef == refCode);
            if (payment is null) return true; // unknown ref

            // Idempotency: if already final, skip
            if (payment.Status == PaymentStatus.Success || payment.Status == PaymentStatus.Failed)
                return true;

            // Use current time as transaction time if provider doesn't send a timestamp
            DateTime txTimeUtc = DateTime.UtcNow;

            var additionalObj = new
            {
                provider = "Pay2S",
                txId = payload.TransId,
                requestId = payload.RequestId,
                message = payload.Message,
                resultCode = payload.ResultCode,
                partnerCode = payload.PartnerCode,
                orderId = payload.OrderId,
                payType = payload.PayType
            };
            var addJson = JsonSerializer.Serialize(additionalObj);

            var orderId = payment.OrderId;
            if (payload.ResultCode == 0)
            {
                // Success: strict amount match
                var expectedDec = payment.Amount;
                var actualDec = Convert.ToDecimal(payload.Amount);
                if (actualDec == expectedDec)
                {
                    payment.Status = PaymentStatus.Success;
                    payment.TransactionDate = txTimeUtc;
                    payment.Additional = addJson;
                    await _paymentRepo.UpdateAsync(payment);

                    var order = await _orderRepo.GetByIdAsync(orderId);
                    if (order != null)
                    {
                        order.Status = OrderStatus.Paid;
                        order.OrderPendingExpires = null;
                        await _orderRepo.UpdateAsync(order);

                        var tickets = await _ticketRepo.GetTicketsByOrderIdAsync(orderId);
                        foreach (var t in tickets)
                        {
                            t.Status = TicketStatus.Issued;
                            t.PurchaseDate = txTimeUtc;
                            var qrPayload = _qrService.BuildTicketPayload(t.TicketId, t.OrderId, t.EventId, t.AttendeeId, null);
                            t.Qrcode = qrPayload;
                            await _ticketRepo.UpdateAsync(t);
                        }
                        // Persist changes before sending email so query sees Issued tickets
                        await _uow.SaveChangesAsync();
                        await TrySendTicketsEmailAsync(orderId);
                    }
                }
                else
                {
                    // Amount mismatch -> treat as failed for safety
                    payment.Status = PaymentStatus.Failed;
                    payment.TransactionDate = txTimeUtc;
                    payment.Additional = addJson;
                    await _paymentRepo.UpdateAsync(payment);

                    var order = await _orderRepo.GetByIdAsync(orderId);
                    if (order != null)
                    {
                        order.Status = OrderStatus.Cancelled;
                        order.OrderPendingExpires = null;
                        await _orderRepo.UpdateAsync(order);

                        var tickets = await _ticketRepo.GetTicketsByOrderIdAsync(orderId);
                        foreach (var t in tickets)
                        {
                            t.Status = TicketStatus.Cancelled;
                            await _ticketRepo.UpdateAsync(t);
                        }
                    }
                }
            }
            else if (payload.ResultCode == 9000)
            {
                // Authorization successful: keep payment/order Pending; record additional info
                payment.Status = PaymentStatus.Pending;
                payment.TransactionDate = txTimeUtc;
                payment.Additional = addJson;
                await _paymentRepo.UpdateAsync(payment);
                // Do NOT change order or tickets yet; await capture notification
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                payment.TransactionDate = txTimeUtc;
                payment.Additional = addJson;
                await _paymentRepo.UpdateAsync(payment);

                var order = await _orderRepo.GetByIdAsync(orderId);
                if (order != null)
                {
                    order.Status = OrderStatus.Cancelled;
                    order.OrderPendingExpires = null;
                    await _orderRepo.UpdateAsync(order);

                    var tickets = await _ticketRepo.GetTicketsByOrderIdAsync(orderId);
                    foreach (var t in tickets)
                    {
                        t.Status = TicketStatus.Cancelled;
                        await _ticketRepo.UpdateAsync(t);
                    }
                }
            }

            await _uow.SaveChangesAsync();
            return true;
        }

        // Manual cancel: mark payment/order/tickets as failed/cancelled for a user's pending order
        public async Task<bool> CancelPendingOrderAsync(string authHeader, ManualCancelRequestDTO request)
        {
            if (request == null || (request.OrderId == null && request.PaymentId == null))
                throw new InvalidOperationException("OrderId or PaymentId is required");

            // Extract user id from JWT
            if (string.IsNullOrEmpty(authHeader)) throw new InvalidOperationException("Missing auth token");
            var token = authHeader.StartsWith("Bearer ") ? authHeader.Substring("Bearer ".Length).Trim() : authHeader;
            System.IdentityModel.Tokens.Jwt.JwtSecurityToken jwtToken;
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                jwtToken = handler.ReadJwtToken(token);
            }
            catch
            {
                throw new InvalidOperationException("Invalid token");
            }
            var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            if (subClaim == null || !Guid.TryParse(subClaim.Value, out var userGuid)) throw new InvalidOperationException("Invalid token");

            // Locate order
            Order? order = null;
            Payment? payment = null;
            if (request.OrderId.HasValue)
            {
                order = await _orderRepo.GetByIdAsync(request.OrderId.Value);
            }
            else if (request.PaymentId.HasValue)
            {
                payment = await _paymentRepo.GetByIdAsync(request.PaymentId.Value);
                if (payment != null)
                {
                    order = await _orderRepo.GetByIdAsync(payment.OrderId);
                }
            }
            if (order == null) return false;

            // Ensure the order belongs to the caller
            if (order.UserId != userGuid)
                throw new InvalidOperationException("You are not allowed to cancel this order");

            // If already final, treat as success (idempotent)
            if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Paid)
                return true;

            await _uow.BeginTransactionAsync();
            try
            {
                // Mark all pending payments of this order as Failed
                var payments = await _paymentRepo.WhereAsync(p => p.OrderId == order.OrderId && p.Status == PaymentStatus.Pending);
                var now = DateTime.UtcNow;
                var addObj = new { reason = request.Reason ?? "ManualCancel", source = "Client", at = now };
                var addJson = JsonSerializer.Serialize(addObj);

                foreach (var pm in payments)
                {
                    pm.Status = PaymentStatus.Failed;
                    pm.TransactionDate = now;
                    pm.Additional = addJson;
                    await _paymentRepo.UpdateAsync(pm);
                }

                // Cancel order and tickets
                order.Status = OrderStatus.Cancelled;
                order.OrderPendingExpires = null;
                await _orderRepo.UpdateAsync(order);

                var tickets = await _ticketRepo.GetTicketsByOrderIdAsync(order.OrderId);
                foreach (var t in tickets)
                {
                    t.Status = TicketStatus.Cancelled;
                    await _ticketRepo.UpdateAsync(t);
                    // Release seat availability immediately (best effort)
                    try
                    {
                        var mappings = await _seatMappingRepo.WhereAsync(m => m.EventId == t.EventId && m.SeatId == t.SeatId);
                        foreach (var m in mappings)
                        {
                            m.IsAvailable = true;
                            await _seatMappingRepo.UpdateAsync(m);
                        }
                    }
                    catch { /* ignore seat release failures */ }
                }

                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw;
            }
        }

        private async Task TrySendTicketsEmailAsync(Guid orderId)
        {
            try
            {
                // Get order and user
                var order = await _orderRepo.GetByIdAsync(orderId);
                if (order == null) return;
                if (order.Status != OrderStatus.Paid) return;

                var user = await _userRepo.GetByIdAsync(order.UserId);
                if (user == null || string.IsNullOrWhiteSpace(user.Email)) return;

                // Load tickets for this order
                var tickets = await _ticketRepo.GetTicketsByOrderIdAsync(orderId);
                if (tickets == null) return;

                var issuedCount = tickets.Count(t => t.Status == TicketStatus.Issued);
                if (issuedCount == 0) return;

                // Gather event and venue info (assume same event per order)
                var firstIssued = tickets.First(t => t.Status == TicketStatus.Issued);
                var esm = firstIssued.EventSeatMapping;
                var ev = esm?.Event;
                var venue = ev?.Venue;

                // Payment info
                var payments = await _paymentRepo.WhereAsync(p => p.OrderId == orderId);
                var paid = payments?.Where(p => p.Status == PaymentStatus.Success).ToList() ?? new List<Payment>();
                string methodName = "Thanh toán";
                var firstPaid = paid.FirstOrDefault();
                if (firstPaid != null)
                {
                    // Ensure PaymentMethod is loaded; repository uses AsNoTracking without Include
                    if (firstPaid.PaymentMethod != null)
                    {
                        methodName = firstPaid.PaymentMethod.MethodName;
                    }
                    else
                    {
                        var pmEntity = await _paymentMethodRepo.GetByIdAsync(firstPaid.PaymentMethodId);
                        if (pmEntity != null && !string.IsNullOrWhiteSpace(pmEntity.MethodName))
                            methodName = pmEntity.MethodName;
                    }
                }
                var totalPaid = paid.Sum(p => p.Amount);

                // Build email HTML with inline QR images (cid)
                var sb = new System.Text.StringBuilder();
                var inlineImages = new Dictionary<string, byte[]>();
                var brand = _configuration["Smtp:FromName"] ?? "Event Ticketing";
                var brandColor = _configuration["Email:BrandColor"] ?? "#2563eb"; // optional brand color
                var vi = new System.Globalization.CultureInfo("vi-VN");
                string Money(decimal v) => string.Format(vi, "{0:C0}", v);

                sb.Append("<div style='font-family:Segoe UI,Roboto,Helvetica,Arial,sans-serif;color:#222'>");
                sb.Append($"<div style='height:4px;background:{brandColor};border-radius:6px 6px 0 0'></div>");
                sb.Append($"<div style='font-size:20px;font-weight:700;margin-top:12px;color:{brandColor}'>{System.Net.WebUtility.HtmlEncode(brand)}</div>");
                sb.Append("<h2 style='margin:16px 0 8px'>Quý khách đã thanh toán thành công đơn hàng</h2>");
                sb.Append($"<div style='margin:6px 0'><b>Mã đơn hàng:</b> {orderId:N}</div>");
                if (ev != null)
                {
                    sb.Append($"<div style='margin:6px 0'><b>Sự kiện:</b> {System.Net.WebUtility.HtmlEncode(ev.EventName)}</div>");
                    var dtStr = ev.StartTime.ToLocalTime().ToString("HH:mm dd/MM/yyyy");
                    sb.Append($"<div style='margin:6px 0'><b>Thời gian:</b> {dtStr}</div>");
                }
                if (venue != null)
                {
                    sb.Append($"<div style='margin:6px 0'><b>Địa điểm:</b> {System.Net.WebUtility.HtmlEncode(venue.VenueName)} - {System.Net.WebUtility.HtmlEncode(venue.Address)}</div>");
                }
                sb.Append($"<div style='margin:6px 0'><b>Phương thức thanh toán:</b> {System.Net.WebUtility.HtmlEncode(methodName)}</div>");

                // Tickets table with QR per row
                sb.Append("<div style='margin:16px 0'>");
                sb.Append("<table role='presentation' width='100%' cellspacing='0' cellpadding='8' style='border-collapse:collapse;border:1px solid #e5e7eb'>");
                sb.Append("<thead><tr style='background:#f0f7ff'>");
                sb.Append("<th align='center' style='border:1px solid #e5e7eb'>STT</th><th align='left' style='border:1px solid #e5e7eb'>Mã vé</th><th align='left' style='border:1px solid #e5e7eb'>Khu vực/ghế</th><th align='left' style='border:1px solid #e5e7eb'>Hạng vé</th><th align='right' style='border:1px solid #e5e7eb'>Giá</th><th align='center' style='border:1px solid #e5e7eb'>QR</th>");
                sb.Append("</tr></thead><tbody>");
                var idx = 0;
                foreach (var t in tickets.Where(x => x.Status == TicketStatus.Issued))
                {
                    idx++;
                    var payload = t.Qrcode ?? _qrService.BuildTicketPayload(t.TicketId, t.OrderId, t.EventId, t.AttendeeId, null);
                    if (t.Qrcode == null)
                    {
                        t.Qrcode = payload;
                        await _ticketRepo.UpdateAsync(t);
                    }
                    var key = $"qr_{t.TicketId:N}";
                    var png = _qrService.GeneratePng(payload, 6);
                    inlineImages[key] = png;

                    var seat = t.EventSeatMapping?.Seat;
                    var seatLabel = seat != null ? $"{seat.RowLabel}-{seat.SeatNumber}" : "-";
                    var tier = t.EventSeatMapping?.TicketCategory ?? "-";
                    var price = t.Price > 0 ? t.Price : (t.EventSeatMapping?.Price ?? 0);

                    sb.Append("<tr>");
                    sb.Append($"<td align='center' style='border:1px solid #e5e7eb'>{idx}</td>");
                    sb.Append($"<td style='border:1px solid #e5e7eb'>#{t.TicketId:N}</td>");
                    sb.Append($"<td style='border:1px solid #e5e7eb'>{System.Net.WebUtility.HtmlEncode(seatLabel)}</td>");
                    sb.Append($"<td style='border:1px solid #e5e7eb'>{System.Net.WebUtility.HtmlEncode(tier)}</td>");
                    sb.Append($"<td align='right' style='border:1px solid #e5e7eb'>{Money(price)}</td>");
                    sb.Append($"<td align='center' style='border:1px solid #e5e7eb'><img alt='Ticket QR' src='cid:{key}' style='width:140px;height:140px;border:1px solid #eee;border-radius:8px' /></td>");
                    sb.Append("</tr>");
                }
                sb.Append("</tbody>");
                var grand = totalPaid > 0 ? totalPaid : order.TotalAmount;
                sb.Append($"<tfoot><tr><td colspan='4' align='right' style='font-weight:600;padding:12px;border-top:1px solid #e5e7eb'>Tổng cộng</td><td align='right' style='font-weight:600;border-top:1px solid #e5e7eb'>{Money(grand)}</td><td style='border-top:1px solid #e5e7eb'></td></tr></tfoot>");
                sb.Append("</table></div>");

                sb.Append("<div style='font-size:12px;color:#666;margin-top:16px'>");
                sb.Append("Lưu ý: Mỗi mã QR chỉ sử dụng một lần tại cổng vào. Vui lòng không chia sẻ mã QR với người khác.");
                sb.Append("</div>");
                sb.Append("</div>");

                var subject = ev != null ? $"Xác nhận thanh toán - {ev.EventName}" : "Xác nhận thanh toán";
                await _emailService.SendWithInlineImagesAsync(user.Email, subject, sb.ToString(), inlineImages);
            }
            catch
            {
                // swallow errors to avoid breaking payment flow, but log for diagnostics
                try
                {
                    _logger.LogError("Order {OrderId}: failed to send tickets email", orderId);
                }
                catch { }
            }
        }


    }
}
