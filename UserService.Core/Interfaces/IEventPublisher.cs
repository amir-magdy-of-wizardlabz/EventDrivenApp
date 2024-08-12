using SharedEvents.Events;

namespace UserService.Core.Interfaces
{
    public interface IEventPublisher
    {
        void Publish<T>(string exchangeName, string routingKey, T userEvent) where T : EventBase;
    }
}
