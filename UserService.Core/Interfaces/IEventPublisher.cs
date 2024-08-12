using SharedEvents.Events;

namespace UserService.Core.Interfaces
{
    public interface IEventPublisher
    {
        void Publish(string exchangeName, string routingKey, EventBase userEvent);
    }
}
