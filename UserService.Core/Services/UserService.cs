using UserService.Core.Entities;
using UserService.Core.Interfaces;
using SharedEvents.Events;
namespace UserService.Core.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEventPublisher _eventPublisher;

        private const string EVENT_VERSION = "1.0";        

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
            _eventPublisher.Publish("user.created", new UserCreatedEvent { Id = user.Id, Email = user.Email, Name = user.Name , Version=EVENT_VERSION });
        }

        public async Task UpdateUser(User user)
        {
            await _userRepository.UpdateUserAsync(user);

            // Publish an event to RabbitMQ after the user is successfully updated
            _eventPublisher.Publish("user.updated", new UserUpdatedEvent { Id = user.Id, Email = user.Email, Name = user.Name , Version=EVENT_VERSION });
        }

        public async Task DeleteUser(int id)
        {
            await _userRepository.DeleteUserAsync(id);

            // Publish an event to RabbitMQ after the user is successfully deleted
            var user = new UserDeletedEvent { Id = id, Version=EVENT_VERSION };
            _eventPublisher.Publish("user.deleted", user);
        }
    }
}
