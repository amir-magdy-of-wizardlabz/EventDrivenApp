using SharedEvents;
using SharedEvents.Events;
using UserService.Core.Entities;
using UserService.Core.Exceptions;
using UserService.Core.Interfaces;

namespace UserService.Tests
{
    public record PublishedEvent(string ExchangeName, string RoutingKey, EventBase Event);

    public class RecordingEventPublisher : IEventPublisher
    {
        public List<PublishedEvent> Published { get; } = new();

        public void Publish<T>(string exchangeName, string routingKey, T @event) where T : EventBase
        {
            Published.Add(new PublishedEvent(exchangeName, routingKey, @event));
        }
    }

    // Behaves like the real repository: assigns ids and rejects duplicate emails.
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = new();
        private int _nextId = 1;

        public Task<User?> GetUserByIdAsync(int id) => Task.FromResult(_users.SingleOrDefault(u => u.Id == id));

        public Task<IEnumerable<User>> GetAllUsersAsync() => Task.FromResult<IEnumerable<User>>(_users.ToList());

        public Task AddUserAsync(User user)
        {
            ThrowIfEmailTaken(user);
            user.Id = _nextId++;
            _users.Add(user);
            return Task.CompletedTask;
        }

        public Task UpdateUserAsync(User user)
        {
            ThrowIfEmailTaken(user);
            return Task.CompletedTask;
        }

        public Task DeleteUserAsync(User user)
        {
            _users.Remove(user);
            return Task.CompletedTask;
        }

        private void ThrowIfEmailTaken(User user)
        {
            if (_users.Any(u => u.Email == user.Email && !ReferenceEquals(u, user)))
            {
                throw new DuplicateEmailException(user.Email);
            }
        }
    }
}
