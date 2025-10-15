using EventManagement.Domain.Models;

namespace EventManagement.Application.Interfaces.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        // Use base IRepository<T> for standard CRUD. Add custom methods here if needed.
    }
}
