using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventManagement.Domain.Models;

namespace EventManagement.Application.Interfaces.Repositories
{
    public interface IUserBankAccountRepository : IRepository<UserBankAccount>
    {
        Task<UserBankAccount?> GetDefaultForUserAsync(Guid userId);
        Task<IEnumerable<UserBankAccount>> GetByUserAsync(Guid userId);
    }
}
