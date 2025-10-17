using EventManagement.Domain.Models;
using EventManagement.Application.DTOs.Responses;

namespace EventManagement.Application.Interfaces.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<List<CategoryOptionDTO>> GetAllCategoryOptionsAsync();
    }
}
