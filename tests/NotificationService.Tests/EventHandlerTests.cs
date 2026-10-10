using Microsoft.EntityFrameworkCore;
using NotificationService.Core.Interfaces;
using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.Messaging;
using SharedEvents.Events;

namespace NotificationService.Tests
{
    public class EventHandlerTests
    {
        private readonly DbContextOptions<NotificationDbContext> _options =
            new DbContextOptionsBuilder<NotificationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        private readonly RecordingNotificationService _mail = new();

        private static readonly CancellationToken None = CancellationToken.None;

        // Every delivery gets its own scope, and so its own DbContext.
        private NotificationDbContext NewContext() => new(_options);

        [Fact]
        public async Task User_created_stores_the_user_and_sends_a_welcome_email()
        {
            await new UserCreatedEventHandler(NewContext(), _mail).HandleAsync(UserCreated(1, "ann@example.com"), None);

            Assert.Equal("ann@example.com", (await NewContext().Users.SingleAsync(None)).Email);

            var sent = Assert.Single(_mail.Sent);
            Assert.Equal("ann@example.com", sent.ToAddress);
            Assert.Contains("Ann", sent.Body);
        }

        [Fact]
        public async Task User_created_delivered_twice_stores_the_user_once()
        {
            await new UserCreatedEventHandler(NewContext(), _mail).HandleAsync(UserCreated(1, "ann@example.com"), None);
            await new UserCreatedEventHandler(NewContext(), _mail).HandleAsync(UserCreated(1, "ann@example.com"), None);

            Assert.Equal(1, await NewContext().Users.CountAsync(None));
        }

        [Fact]
        public async Task User_created_keeps_the_user_when_the_email_cannot_be_sent()
        {
            _mail.Fail = true;

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                new UserCreatedEventHandler(NewContext(), _mail).HandleAsync(UserCreated(1, "ann@example.com"), None));

            Assert.Equal(1, await NewContext().Users.CountAsync(None));
        }

        [Fact]
        public async Task User_updated_changes_the_stored_email()
        {
            await new UserCreatedEventHandler(NewContext(), _mail).HandleAsync(UserCreated(1, "ann@example.com"), None);

            await new UserUpdatedEventHandler(NewContext()).HandleAsync(UserUpdated(1, "new@example.com"), None);

            Assert.Equal("new@example.com", (await NewContext().Users.SingleAsync(None)).Email);
        }

        [Fact]
        public async Task User_updated_for_an_unknown_user_stores_them()
        {
            await new UserUpdatedEventHandler(NewContext()).HandleAsync(UserUpdated(1, "new@example.com"), None);

            Assert.Equal("new@example.com", (await NewContext().Users.SingleAsync(None)).Email);
        }

        [Fact]
        public async Task User_deleted_removes_the_user_and_tolerates_a_repeat()
        {
            await new UserCreatedEventHandler(NewContext(), _mail).HandleAsync(UserCreated(1, "ann@example.com"), None);

            await new UserDeletedEventHandler(NewContext()).HandleAsync(new UserDeletedEvent { Id = 1, Version = "1.0" }, None);
            await new UserDeletedEventHandler(NewContext()).HandleAsync(new UserDeletedEvent { Id = 1, Version = "1.0" }, None);

            Assert.Empty(await NewContext().Users.ToListAsync(None));
        }

        [Fact]
        public async Task Order_created_emails_the_users_current_address()
        {
            await new UserCreatedEventHandler(NewContext(), _mail).HandleAsync(UserCreated(1, "ann@example.com"), None);
            await new UserUpdatedEventHandler(NewContext()).HandleAsync(UserUpdated(1, "new@example.com"), None);
            _mail.Sent.Clear();

            await new OrderCreatedEventHandler(NewContext(), _mail).HandleAsync(OrderCreated(orderId: 10, userId: 1), None);

            var sent = Assert.Single(_mail.Sent);
            Assert.Equal("new@example.com", sent.ToAddress);
            Assert.Contains("10", sent.Body);
        }

        [Fact]
        public async Task Order_created_for_an_unknown_user_fails_so_the_message_is_not_lost()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                new OrderCreatedEventHandler(NewContext(), _mail).HandleAsync(OrderCreated(orderId: 10, userId: 99), None));

            Assert.Empty(_mail.Sent);
        }

        private static UserCreatedEvent UserCreated(int id, string email) =>
            new() { Id = id, Name = "Ann", Email = email, Version = "1.0" };

        private static UserUpdatedEvent UserUpdated(int id, string email) =>
            new() { Id = id, Name = "Ann", Email = email, Version = "1.0" };

        private static OrderCreatedEvent OrderCreated(int orderId, int userId) =>
            new() { OrderId = orderId, UserId = userId, TotalAmount = 1m, OrderDate = DateTime.UtcNow, Version = "1.0" };

        private record SentEmail(string ToAddress, string Subject, string Body);

        private class RecordingNotificationService : INotificationService
        {
            public List<SentEmail> Sent { get; } = new();
            public bool Fail { get; set; }

            public Task NotifyAsync(string toAddress, string subject, string body, CancellationToken cancellationToken)
            {
                if (Fail)
                {
                    throw new InvalidOperationException("Mail server is down.");
                }

                Sent.Add(new SentEmail(toAddress, subject, body));
                return Task.CompletedTask;
            }
        }
    }
}
