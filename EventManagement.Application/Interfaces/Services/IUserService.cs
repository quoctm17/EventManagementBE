using System.Threading.Tasks;
using EventManagement.Application.DTOs.Requests;
using EventManagement.Application.DTOs.Responses;
using EventManagement.Domain.Models;

namespace EventManagement.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(Guid id);
        Task<UserResponseDTO?> GetCurrentUserAsync(string token);
    }
}