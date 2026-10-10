using Microsoft.EntityFrameworkCore;
using Npgsql;
using UserService.Core.Entities;
using UserService.Core.Exceptions;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _dbContext;

        public UserRepository(UserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _dbContext.Users.FindAsync(id);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _dbContext.Users.AsNoTracking().OrderBy(u => u.Id).ToListAsync();
        }

        public async Task AddUserAsync(User user)
        {
            _dbContext.Users.Add(user);
            await SaveChangesAsync(user);
        }

        public async Task UpdateUserAsync(User user)
        {
            await SaveChangesAsync(user);
        }

        public async Task DeleteUserAsync(User user)
        {
            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
        }

        // The unique index on Email is the only check that holds under concurrent requests,
        // so uniqueness is enforced there rather than with a query beforehand.
        private async Task SaveChangesAsync(User user)
        {
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                throw new DuplicateEmailException(user.Email, ex);
            }
        }
    }
}
