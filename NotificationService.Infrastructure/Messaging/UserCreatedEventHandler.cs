using NotificationService.Core.Entities;
using NotificationService.Core.Interfaces;
using NotificationService.Infrastructure.Data;
using SharedEvents;
using SharedEvents.Events;

namespace NotificationService.Infrastructure.Messaging
{
    public class UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
    {
        private readonly NotificationDbContext _dbContext;
        private readonly INotificationService _notificationService;

        public UserCreatedEventHandler(NotificationDbContext dbContext, INotificationService notificationService)
        {
            _dbContext = dbContext;
            _notificationService = notificationService;
        }

        public async Task HandleAsync(UserCreatedEvent @event, CancellationToken cancellationToken)
        {
            // Store the user before sending, so a mail failure cannot lose the address.
            // On redelivery the user is already there and only the email is attempted again.
            var user = await _dbContext.Users.FindAsync(new object[] { @event.Id }, cancellationToken);
            if (user == null)
            {
                _dbContext.Users.Add(new User { Id = @event.Id, Email = @event.Email });
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            await _notificationService.NotifyAsync(
                @event.Email,
                "Welcome to Our Service",
                $"Hi {@event.Name}, thank you for creating an account!",
                cancellationToken);
        }
    }
}
