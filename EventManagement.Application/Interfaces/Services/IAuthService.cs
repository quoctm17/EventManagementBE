using System.Threading.Tasks;
using EventManagement.Application.DTOs.Requests;
using EventManagement.Application.DTOs.Responses;

namespace EventManagement.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> LoginAsync(UserLoginRequestDTO request);
        Task<bool> RegisterAsync(UserRegisterRequestDTO request);
        Task<bool> ValidateTokenAsync(string token);
    }
}