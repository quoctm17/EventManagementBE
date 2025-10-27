using System.Threading.Tasks;
using EventManagement.Application.DTOs.Requests;
using EventManagement.Application.DTOs.Responses;

namespace EventManagement.Application.Interfaces.Services
{
    public interface ICheckoutService
    {
        Task<CheckoutPrepareResponseDTO> PrepareAsync(string authHeader, CheckoutPrepareRequestDTO request);
    }
}
