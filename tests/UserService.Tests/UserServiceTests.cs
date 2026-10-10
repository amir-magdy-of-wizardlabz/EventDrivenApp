using SharedEvents;
using SharedEvents.Events;
using UserService.Core.Exceptions;

namespace UserService.Tests
{
    public class UserServiceTests
    {
        private readonly InMemoryUserRepository _repository = new();
        private readonly RecordingEventPublisher _publisher = new();
        private readonly Core.Services.UserService _service;

        public UserServiceTests()
        {
            _service = new Core.Services.UserService(_repository, _publisher);
        }

        [Fact]
        public async Task AddUser_saves_the_user_and_publishes_user_created()
        {
            var user = await _service.AddUser("Ann", "ann@example.com");

            Assert.Same(user, await _repository.GetUserByIdAsync(user.Id));

            var published = Assert.Single(_publisher.Published);
            Assert.Equal(Exchanges.Users, published.ExchangeName);
            Assert.Equal(RoutingKeys.UserCreated, published.RoutingKey);

            var @event = Assert.IsType<UserCreatedEvent>(published.Event);
            Assert.Equal(user.Id, @event.Id);
            Assert.Equal("Ann", @event.Name);
            Assert.Equal("ann@example.com", @event.Email);
            Assert.True(@event.IsValidVersion());
        }

        [Fact]
        public async Task AddUser_with_a_taken_email_fails_and_publishes_nothing()
        {
            await _service.AddUser("Ann", "ann@example.com");
            _publisher.Published.Clear();

            await Assert.ThrowsAsync<DuplicateEmailException>(() => _service.AddUser("Other", "ann@example.com"));

            Assert.Empty(_publisher.Published);
        }

        [Fact]
        public async Task UpdateUser_changes_the_user_and_publishes_user_updated()
        {
            var user = await _service.AddUser("Ann", "ann@example.com");
            _publisher.Published.Clear();

            var updated = await _service.UpdateUser(user.Id, "Ann B", "ann.b@example.com");

            Assert.True(updated);
            Assert.Equal("Ann B", user.Name);

            var published = Assert.Single(_publisher.Published);
            Assert.Equal(RoutingKeys.UserUpdated, published.RoutingKey);
            Assert.Equal("ann.b@example.com", Assert.IsType<UserUpdatedEvent>(published.Event).Email);
        }

        [Fact]
        public async Task UpdateUser_for_a_missing_user_returns_false_and_publishes_nothing()
        {
            Assert.False(await _service.UpdateUser(42, "Nobody", "nobody@example.com"));
            Assert.Empty(_publisher.Published);
        }

        [Fact]
        public async Task DeleteUser_removes_the_user_and_publishes_user_deleted()
        {
            var user = await _service.AddUser("Ann", "ann@example.com");
            _publisher.Published.Clear();

            var deleted = await _service.DeleteUser(user.Id);

            Assert.True(deleted);
            Assert.Null(await _repository.GetUserByIdAsync(user.Id));

            var published = Assert.Single(_publisher.Published);
            Assert.Equal(RoutingKeys.UserDeleted, published.RoutingKey);
            Assert.Equal(user.Id, Assert.IsType<UserDeletedEvent>(published.Event).Id);
        }

        [Fact]
        public async Task DeleteUser_for_a_missing_user_returns_false_and_publishes_nothing()
        {
            Assert.False(await _service.DeleteUser(42));
            Assert.Empty(_publisher.Published);
        }
    }
}
