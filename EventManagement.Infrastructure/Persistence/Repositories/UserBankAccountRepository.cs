using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Infrastructure.Persistence.Repositories
{
    public class UserBankAccountRepository : RepositoryBase<UserBankAccount>, IUserBankAccountRepository
    {
        public UserBankAccountRepository(EventManagementDbContext context) : base(context) { }

        public async Task<UserBankAccount?> GetDefaultForUserAsync(Guid userId)
        {
            return await _context.UserBankAccounts.AsNoTracking()
                .Where(b => b.UserId == userId && b.IsActive != false)
                .OrderByDescending(b => b.IsDefault == true)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<UserBankAccount>> GetByUserAsync(Guid userId)
        {
            return await _context.UserBankAccounts.AsNoTracking()
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }
    }
}
