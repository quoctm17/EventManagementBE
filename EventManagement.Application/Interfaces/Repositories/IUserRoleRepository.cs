using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManagement.Application.Interfaces.Repositories
{
    public interface IUserRoleRepository
    {
        Task<IEnumerable<string>> GetRolesByUserIdAsync(Guid userId);
    }
}