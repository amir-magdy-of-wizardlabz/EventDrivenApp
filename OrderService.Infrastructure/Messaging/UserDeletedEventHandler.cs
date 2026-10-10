using OrderService.Infrastructure.Data;
using SharedEvents;
using SharedEvents.Events;

namespace OrderService.Infrastructure.Messaging
{
    public class UserDeletedEventHandler : IEventHandler<UserDeletedEvent>
    {
        private readonly OrderDbContext _dbContext;

        public UserDeletedEventHandler(OrderDbContext dbContext)
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

            // The foreign key cascades, so the user's orders are removed with them.
            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
