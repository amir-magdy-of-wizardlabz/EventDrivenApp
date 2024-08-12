using SharedEvents.Events;
using UserService.Core.Entities;

namespace UserService.Core.Interfaces
{
    public interface IEventPublisher
    {
        void Publish(string queueName, EventBase user);
    }
}
