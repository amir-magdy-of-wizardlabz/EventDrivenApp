using NotificationService.Infrastructure.Data;
using SharedEvents;
using SharedEvents.Events;

namespace NotificationService.Infrastructure.Messaging
{
    public class UserDeletedEventHandler : IEventHandler<UserDeletedEvent>
    {
        private readonly NotificationDbContext _dbContext;

        public UserDeletedEventHandler(NotificationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task HandleAsync(UserDeletedEvent @event, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FindAsync(new object[] { @event.Id }, cancellationToken);
            if (user == null)
            {
                return;
            }

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
