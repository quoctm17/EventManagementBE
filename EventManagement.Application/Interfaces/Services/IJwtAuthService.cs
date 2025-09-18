using EventManagement.Domain.Models;

namespace EventManagement.Application.Interfaces.Services
{
    public interface IJwtAuthService
    {
        string GenerateToken(User user);
        string DecodePayloadToken(string token);
    }
}