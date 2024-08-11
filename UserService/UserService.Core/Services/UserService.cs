using UserService.Core.Entities;
using UserService.Core.Interfaces;

namespace UserService.Core.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEventPublisher _eventPublisher;

        public UserService(IUserRepository userRepository, IEventPublisher eventPublisher)
        {
            _userRepository = userRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<User> GetUserById(int id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task AddUser(User user)
        {
            if (await _userRepository.EmailExistsAsync(user.Email))
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }

            await _userRepository.AddUserAsync(user);

            // Publish an event to RabbitMQ after the user is successfully added
            var message = $"User with ID {user.Id} created.";
            _eventPublisher.Publish("user.created", message);
        }

        public async Task UpdateUser(User user)
        {
            await _userRepository.UpdateUserAsync(user);

            // Publish an event to RabbitMQ after the user is successfully updated
            var message = $"User with ID {user.Id} updated.";
            _eventPublisher.Publish("user.updated", message);
        }

        public async Task DeleteUser(int id)
        {
            await _userRepository.DeleteUserAsync(id);

            // Publish an event to RabbitMQ after the user is successfully deleted
            var message = $"User with ID {id} deleted.";
            _eventPublisher.Publish("user.deleted", message);
        }
    }
}
