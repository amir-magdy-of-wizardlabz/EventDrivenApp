using SharedEvents.Events;

namespace SharedEvents
{
    public interface IEventPublisher
    {
        // Returns once the broker has confirmed the message; throws if it could not be delivered.
        void Publish<T>(string exchangeName, string routingKey, T @event) where T : EventBase;
    }
}
