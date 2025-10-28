using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using EventManagement.Application.DTOs.Requests;
using EventManagement.Application.DTOs.Responses;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using EventManagement.Domain.Models;

namespace EventManagement.Application.Services
{
    public class CheckoutService : ICheckoutService
    {
        private readonly IEventRepository _eventRepo;
        private readonly IEventSeatMappingRepository _seatMappingRepo;
        private readonly ISeatHoldRepository _seatHoldRepo;
        private readonly IPaymentService _paymentService;
        private readonly IUserService _userService;
        private readonly ISeatRealtimeService _realtime;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _config;

        public CheckoutService(
            IEventRepository eventRepo,
            IEventSeatMappingRepository seatMappingRepo,
            ISeatHoldRepository seatHoldRepo,
            IPaymentService paymentService,
            IUserService userService,
            ISeatRealtimeService realtime,
            IMapper mapper,
            IUnitOfWork uow,
            IConfiguration config)
        {
            _eventRepo = eventRepo;
            _seatMappingRepo = seatMappingRepo;
            _seatHoldRepo = seatHoldRepo;
            _paymentService = paymentService;
            _userService = userService;
            _realtime = realtime;
            _mapper = mapper;
            _uow = uow;
            _config = config;
        }

        public async Task<CheckoutPrepareResponseDTO> PrepareAsync(string authHeader, CheckoutPrepareRequestDTO request)
        {
            if (request == null || request.UserId == Guid.Empty || request.EventId == Guid.Empty || request.SeatIds == null || request.SeatIds.Count == 0)
                throw new InvalidOperationException("Invalid request");

            // Extract user id from JWT (required for SeatHold)
            if (string.IsNullOrEmpty(authHeader)) throw new InvalidOperationException("Unauthorized");
            var token = authHeader.StartsWith("Bearer ") ? authHeader.Substring("Bearer ".Length).Trim() : authHeader;
            System.IdentityModel.Tokens.Jwt.JwtSecurityToken jwtToken;
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                jwtToken = handler.ReadJwtToken(token);
            }
            catch
            {
                throw new InvalidOperationException("Unauthorized");
            }
            var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            if (subClaim == null || !Guid.TryParse(subClaim.Value, out var userGuid)) throw new InvalidOperationException("Unauthorized");

            // Validate the provided userId matches token subject to prevent spoofing
            if (request.UserId != userGuid)
                throw new InvalidOperationException("Unauthorized");

            // Load user info to include in response
            var userDto = await _userService.GetCurrentUserAsync(authHeader);
            if (userDto == null || userDto.UserId == Guid.Empty)
                throw new InvalidOperationException("Unauthorized");

            // Load event details
            var ev = await _eventRepo.GetEventWithDetailsAsync(request.EventId);
            if (ev == null) throw new InvalidOperationException("Event not found");
            var evDto = _mapper.Map<EventDetailDTO>(ev);

            // Load selected seat mappings for event WITH Seat navigation to populate SeatId/RowLabel/SeatNumber in DTO
            var seatIds = request.SeatIds.Distinct().ToList();
            var allMappingsWithSeat = await _seatMappingRepo.GetByEventIdWithSeatAsync(request.EventId);
            var mappingList = allMappingsWithSeat.Where(m => seatIds.Contains(m.SeatId)).ToList();
            if (mappingList.Count != seatIds.Count)
            {
                throw new InvalidOperationException("Some seats do not exist for this event");
            }

            // Check availability + holds
            var nowUtc = DateTime.UtcNow;
            var activeHeldSeatIds = await _seatHoldRepo.GetActiveHeldSeatIdsAsync(request.EventId, nowUtc);
            var invalid = new List<Guid>();

            foreach (var m in mappingList)
            {
                var isAvailable = m.IsAvailable != false;
                var isHeld = isAvailable && activeHeldSeatIds.Contains(m.SeatId);
                if (!isAvailable || isHeld)
                {
                    invalid.Add(m.SeatId);
                }
            }

            if (invalid.Count > 0)
            {
                throw new InvalidOperationException($"Some seats are unavailable or held: {string.Join(",", invalid)}");
            }

            // Build selected seats DTOs
            var seatDtos = mappingList
                .Select(m => _mapper.Map<EventSeatResponseDTO>(m))
                .ToList();
            foreach (var s in seatDtos) s.IsHeld = false;

            // Total amount
            var total = mappingList.Sum(m => m.Price);

            // Create SeatHolds with configured TTL
            var seatHoldMinutes = 10;
            var raw = _config["Reservation:SeatHoldExpiresMinutes"]; 
            if (int.TryParse(raw, out var confMinutes) && confMinutes > 0) 
            {
                seatHoldMinutes = confMinutes;
            }

            await _uow.BeginTransactionAsync();
            try
            {
                foreach (var m in mappingList)
                {
                    var hold = new SeatHold
                    {
                        HoldId = Guid.NewGuid(),
                        EventId = request.EventId,
                        SeatId = m.SeatId,
                        UserId = request.UserId,
                        HoldExpiresAt = nowUtc.AddMinutes(seatHoldMinutes),
                        OrderId = null
                    };
                    await _seatHoldRepo.AddAsync(hold);
                }

                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw;
            }   

            // Mark seats as held in response (they are now reserved temporarily)
            foreach (var s in seatDtos)
            {
                s.IsHeld = true;
            }

            // Broadcast realtime notification (non-blocking)
            try
            {
                await _realtime.SeatsHeld(request.EventId, seatIds, nowUtc.AddMinutes(seatHoldMinutes));
            }
            catch { }

            // Payment methods
            var paymentMethods = (await _paymentService.GetPaymentMethodsAsync(true)).ToList();

            return new CheckoutPrepareResponseDTO
            {
                User = userDto,
                Event = evDto,
                SelectedSeats = seatDtos,
                TotalAmount = total,
                PaymentMethods = paymentMethods
            };
        }
    }
}
