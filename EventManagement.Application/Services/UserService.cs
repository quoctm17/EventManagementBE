using System.Threading.Tasks;
using EventManagement.Application.Interfaces;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Domain.Models;
using EventManagement.Application.Helpers;
using EventManagement.Application.DTOs.Responses;
using EventManagement.Application.DTOs.Requests;

namespace EventManagement.Application.Services
{
    public class UserService : IUserService
    {
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtAuthService _jwtAuthService;
    private readonly IUserRoleRepository _userRoleRepo;
    private readonly ITicketRepository _ticketRepo;
    private readonly IEventRepository _eventRepo;
    private readonly AutoMapper.IMapper _mapper;

        public UserService(
            IUserRepository userRepo,
            IUnitOfWork unitOfWork,
            IJwtAuthService jwtAuthService,
            IUserRoleRepository userRoleRepo,
            ITicketRepository ticketRepo,
            IEventRepository eventRepo,
            AutoMapper.IMapper mapper)
        {
            _userRepo = userRepo;
            _unitOfWork = unitOfWork;
            _jwtAuthService = jwtAuthService;
            _userRoleRepo = userRoleRepo;
            _ticketRepo = ticketRepo;
            _eventRepo = eventRepo;
            _mapper = mapper;
        }

        public Task<User?> GetByEmailAsync(string email)
        {
            return _userRepo.GetByEmailAsync(email);
        }

        public Task<User?> GetByIdAsync(Guid id)
        {
            return _userRepo.GetByIdAsync(id);
        }

        public async Task<UserResponseDTO?> GetCurrentUserAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            // Remove Bearer prefix if present
            if (token.StartsWith("Bearer "))
                token = token.Substring("Bearer ".Length).Trim();

            System.IdentityModel.Tokens.Jwt.JwtSecurityToken jwtToken;
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                jwtToken = handler.ReadJwtToken(token);
            }
            catch
            {
                return null;
            }

            var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            if (subClaim == null || !Guid.TryParse(subClaim.Value, out var userId))
                return null;

            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return null;

            var userResponse = _mapper.Map<UserResponseDTO>(user);
            var roles = await _userRoleRepo.GetRolesByUserIdAsync(user.UserId);
            userResponse.Roles = roles.ToList();

            // Compute counts
            userResponse.TicketsPurchasedCount = await _ticketRepo.CountByAttendeeAsync(user.UserId);
            userResponse.EventsCreatedCount = await _eventRepo.CountByOrganizerAsync(user.UserId);
            return userResponse;
        }
    }
}