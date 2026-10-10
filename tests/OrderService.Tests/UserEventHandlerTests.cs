using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.Messaging;
using SharedEvents.Events;

namespace OrderService.Tests
{
    public class UserEventHandlerTests
    {
        private readonly DbContextOptions<OrderDbContext> _options =
            new DbContextOptionsBuilder<OrderDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        private static readonly CancellationToken None = CancellationToken.None;

        // Every delivery gets its own scope, and so its own DbContext.
        private OrderDbContext NewContext() => new(_options);

        [Fact]
        public async Task User_created_stores_the_user_id()
        {
            await new UserCreatedEventHandler(NewContext()).HandleAsync(UserCreated(5), None);

            Assert.Equal(5, (await NewContext().Users.SingleAsync(None)).Id);
        }

        [Fact]
        public async Task User_created_delivered_twice_stores_the_user_once()
        {
            await new UserCreatedEventHandler(NewContext()).HandleAsync(UserCreated(5), None);
            await new UserCreatedEventHandler(NewContext()).HandleAsync(UserCreated(5), None);

            Assert.Equal(1, await NewContext().Users.CountAsync(None));
        }

        [Fact]
        public async Task User_deleted_removes_the_user()
        {
            await new UserCreatedEventHandler(NewContext()).HandleAsync(UserCreated(5), None);

            await new UserDeletedEventHandler(NewContext()).HandleAsync(UserDeleted(5), None);

            Assert.Empty(await NewContext().Users.ToListAsync(None));
        }

        [Fact]
        public async Task User_deleted_for_an_unknown_user_does_nothing()
        {
            await new UserDeletedEventHandler(NewContext()).HandleAsync(UserDeleted(5), None);

            Assert.Empty(await NewContext().Users.ToListAsync(None));
        }

        private static UserCreatedEvent UserCreated(int id) =>
            new() { Id = id, Name = "Ann", Email = "ann@example.com", Version = "1.0" };

        private static UserDeletedEvent UserDeleted(int id) => new() { Id = id, Version = "1.0" };
    }
}
