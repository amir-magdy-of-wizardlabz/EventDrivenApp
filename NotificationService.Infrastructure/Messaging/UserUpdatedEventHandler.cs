using NotificationService.Core.Entities;
using NotificationService.Infrastructure.Data;
using SharedEvents;
using SharedEvents.Events;

namespace NotificationService.Infrastructure.Messaging
{
    public class UserUpdatedEventHandler : IEventHandler<UserUpdatedEvent>
    {
        private readonly NotificationDbContext _dbContext;

        public UserUpdatedEventHandler(NotificationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task HandleAsync(UserUpdatedEvent @event, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FindAsync(new object[] { @event.Id }, cancellationToken);
            if (user == null)
            {
                _dbContext.Users.Add(new User { Id = @event.Id, Email = @event.Email });
            }
            else
            {
                user.Email = @event.Email;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
