using SharedEvents;
using SharedEvents.Events;
using UserService.Core.Entities;
using UserService.Core.Interfaces;

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

        public async Task<User?> GetUserById(int id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task<User> AddUser(string name, string email)
        {
            var user = new User { Name = name, Email = email };
            await _userRepository.AddUserAsync(user);

            // The save and the publish are two separate writes: if the broker is down the user
            // exists but the event is lost. Closing that gap needs a transactional outbox.
            _eventPublisher.Publish(Exchanges.Users, RoutingKeys.UserCreated, new UserCreatedEvent
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Version = EVENT_VERSION
            });

            return user;
        }

        public async Task<bool> UpdateUser(int id, string name, string email)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            user.Name = name;
            user.Email = email;
            await _userRepository.UpdateUserAsync(user);

            _eventPublisher.Publish(Exchanges.Users, RoutingKeys.UserUpdated, new UserUpdatedEvent
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Version = EVENT_VERSION
            });

            return true;
        }

        public async Task<bool> DeleteUser(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            await _userRepository.DeleteUserAsync(user);

            _eventPublisher.Publish(Exchanges.Users, RoutingKeys.UserDeleted, new UserDeletedEvent
            {
                Id = id,
                Version = EVENT_VERSION
            });

            return true;
        }
    }
}
