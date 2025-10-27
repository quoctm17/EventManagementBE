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
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtAuthService _jwtAuthService;

        public AuthService(IUserRepository userRepo, IUnitOfWork unitOfWork, IJwtAuthService jwtAuthService)
        {
            _userRepo = userRepo;
            _unitOfWork = unitOfWork;
            _jwtAuthService = jwtAuthService;
        }

        public async Task<AuthResponseDTO?> LoginAsync(UserLoginRequestDTO request)
        {
            var user = await _userRepo.SingleOrDefaultAsync(
                us => us.Email == request.UsernameOrEmail || us.FullName == request.UsernameOrEmail
            );

            if (user == null) return null;

            // Sử dụng PasswordHelper
            if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
                return null;

            string token = _jwtAuthService.GenerateToken(user);

            return new AuthResponseDTO
            {
                AccessToken = token
            };
        }

        public async Task<bool> RegisterAsync(UserRegisterRequestDTO request)
        {
            var existingUser = await _userRepo.SingleOrDefaultAsync(
                u => u.Email == request.Email || u.FullName == request.UserName
            );

            if (existingUser != null) return false;

            var user = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = PasswordHelper.HashPassword(request.Password),
                IsActive = true
            };

            await _userRepo.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public Task<bool> ValidateTokenAsync(string token)
        {
            // Delegate to JwtAuthService which knows issuer/audience/signing key
            var isValid = _jwtAuthService.ValidateToken(token);
            return Task.FromResult(isValid);
        }
    }
}