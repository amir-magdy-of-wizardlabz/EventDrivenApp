using NotificationService.Core.Interfaces;
using NotificationService.Infrastructure.Data;
using SharedEvents;
using SharedEvents.Events;

namespace NotificationService.Infrastructure.Messaging
{
    public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
    {
        private readonly NotificationDbContext _dbContext;
        private readonly INotificationService _notificationService;

        public OrderCreatedEventHandler(NotificationDbContext dbContext, INotificationService notificationService)
        {
            _dbContext = dbContext;
            _notificationService = notificationService;
        }

        public async Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FindAsync(new object[] { @event.UserId }, cancellationToken);
            if (user == null)
            {
                // Failing sends the message to the dead-letter queue instead of dropping it silently.
                throw new InvalidOperationException($"User with ID {@event.UserId} not found.");
            }

            await _notificationService.NotifyAsync(
                user.Email,
                "Order Confirmation",
                $"Your order with ID {@event.OrderId} has been created successfully!",
                cancellationToken);
        }
    }
}
