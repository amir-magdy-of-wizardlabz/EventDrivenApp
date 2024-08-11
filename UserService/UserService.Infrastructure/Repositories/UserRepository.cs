using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Exceptions;

namespace UserService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _dbContext;

        public UserRepository(UserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
            {
                throw new UserNotFoundException($"User with ID {id} was not found.");
            }

            return user;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _dbContext.Users.ToListAsync();
        }

        public async Task AddUserAsync(User user)
        {
            if (await EmailExistsAsync(user.Email))
            {
                throw new DuplicateEmailException($"A user with the email '{user.Email}' already exists.");
            }

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            // Check if the user exists
            var existingUser = await _dbContext.Users.FindAsync(user.Id);
            if (existingUser == null)
            {
                throw new UserNotFoundException($"User with ID {user.Id} was not found.");
            }

            // Check if the email is already used by another user
            if (await EmailExistsAsync(user.Email) && existingUser.Email != user.Email)
            {
                throw new DuplicateEmailException($"A user with the email '{user.Email}' already exists.");
            }

            // Detach the existing entity to avoid tracking conflict
            _dbContext.Entry(existingUser).State = EntityState.Detached;

            // Update the user
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
            {
                throw new UserNotFoundException($"User with ID {id} was not found.");
            }

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbContext.Users.AnyAsync(u => u.Email == email);
        }
    }
}
