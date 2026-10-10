using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using SharedEvents;
using SharedEvents.Events;

namespace SharedMessaging
{
    internal sealed class RabbitMqEventPublisher : IEventPublisher
    {
        private static readonly TimeSpan ConfirmTimeout = TimeSpan.FromSeconds(5);

        private readonly IServiceProvider _serviceProvider;

        public RabbitMqEventPublisher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Publish<T>(string exchangeName, string routingKey, T @event) where T : EventBase
        {
            // The connection is long-lived and shared; channels are cheap but not thread-safe,
            // so each publish gets its own.
            var connection = _serviceProvider.GetRequiredService<IConnection>();
            using var channel = connection.CreateModel();

            channel.ConfirmSelect();
            channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct, durable: true);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Type = @event.GetType().Name;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            channel.BasicPublish(exchange: exchangeName,
                                 routingKey: routingKey,
                                 basicProperties: properties,
                                 body: JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType()));

            channel.WaitForConfirmsOrDie(ConfirmTimeout);
        }
    }
}
