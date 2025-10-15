using System.Threading.Tasks;
using EventManagement.Application.DTOs.Responses;

namespace EventManagement.Application.Interfaces.Services
{
    public interface IHomeService
    {
        Task<HomeResponseDTO> GetHomeAsync();
    }
}
