using SharedEvents.Events;

namespace SharedEvents
{
    // Delivery is at-least-once, so implementations must be idempotent:
    // handling the same event twice has to leave the same state as handling it once.
    public interface IEventHandler<in TEvent> where TEvent : EventBase
    {
        Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
    }
}
