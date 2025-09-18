using System.Threading.Tasks;
using EventManagement.Application.DTOs.Requests;
using EventManagement.Application.DTOs.Responses;

namespace EventManagement.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(UserLoginRequest request);
        Task<bool> RegisterAsync(UserRegisterRequest request);
    }
}