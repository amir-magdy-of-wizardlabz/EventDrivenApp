using Microsoft.Extensions.DependencyInjection;
using SharedEvents;
using SharedEvents.Events;

namespace SharedMessaging
{
    internal sealed record Subscription(
        string ExchangeName,
        string RoutingKey,
        Type EventType,
        Func<IServiceProvider, EventBase, CancellationToken, Task> Dispatch);

    internal sealed record RabbitMqConsumerOptions(string QueueName, IReadOnlyList<Subscription> Subscriptions);

    public sealed class RabbitMqConsumerBuilder
    {
        private readonly IServiceCollection _services;

        internal RabbitMqConsumerBuilder(IServiceCollection services)
        {
            _services = services;
        }

        internal List<Subscription> Subscriptions { get; } = new();

        public RabbitMqConsumerBuilder Subscribe<TEvent, THandler>(string exchangeName, string routingKey)
            where TEvent : EventBase
            where THandler : class, IEventHandler<TEvent>
        {
            _services.AddScoped<IEventHandler<TEvent>, THandler>();

            Subscriptions.Add(new Subscription(
                exchangeName,
                routingKey,
                typeof(TEvent),
                (provider, @event, cancellationToken) =>
                    provider.GetRequiredService<IEventHandler<TEvent>>().HandleAsync((TEvent)@event, cancellationToken)));

            return this;
        }
    }
}
