using System.Collections.Generic;
using System.Threading.Tasks;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Domain.Models;
using EventManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventManagement.Application.DTOs.Responses;

namespace EventManagement.Infrastructure.Persistence.Repositories
{
    public class CategoryRepository : RepositoryBase<Category>, ICategoryRepository
    {
        public CategoryRepository(EventManagementDbContext context) : base(context)
        {
        }

        public async Task<List<CategoryOptionDTO>> GetAllCategoryOptionsAsync()
        {
            return await _context.Categories
                .Where(c => c.CategoryId != Guid.Empty && !string.IsNullOrEmpty(c.CategoryName))
                .Select(c => new CategoryOptionDTO { CategoryId = c.CategoryId, CategoryName = c.CategoryName.Trim() })
                .Distinct()
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }
    }
}
