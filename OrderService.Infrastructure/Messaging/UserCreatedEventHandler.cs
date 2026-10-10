using Microsoft.EntityFrameworkCore;
using OrderService.Core.Entities;
using OrderService.Infrastructure.Data;
using SharedEvents;
using SharedEvents.Events;

namespace OrderService.Infrastructure.Messaging
{
    public class UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
    {
        private readonly OrderDbContext _dbContext;

        public UserCreatedEventHandler(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task HandleAsync(UserCreatedEvent @event, CancellationToken cancellationToken)
        {
            // A redelivered event finds the user already stored.
            if (await _dbContext.Users.AnyAsync(u => u.Id == @event.Id, cancellationToken))
            {
                return;
            }

            _dbContext.Users.Add(new User { Id = @event.Id });
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
