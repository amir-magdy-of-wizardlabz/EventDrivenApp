using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using SharedEvents.Events;

namespace SharedMessaging
{
    internal sealed class RabbitMqConsumer : BackgroundService
    {
        private const ushort PrefetchCount = 10;
        private static readonly TimeSpan ConnectRetryDelay = TimeSpan.FromSeconds(5);

        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitMqConsumerOptions _options;
        private readonly ILogger<RabbitMqConsumer> _logger;
        private IModel? _channel;

        public RabbitMqConsumer(IServiceProvider serviceProvider, RabbitMqConsumerOptions options, ILogger<RabbitMqConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _options = options;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Connecting blocks, so get off the host's startup path first.
            await Task.Yield();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _channel = StartConsuming(stoppingToken);
                    _logger.LogInformation("Consuming from queue {Queue}", _options.QueueName);
                    return;
                }
                catch (BrokerUnreachableException ex)
                {
                    _logger.LogWarning("RabbitMQ is unreachable ({Reason}); retrying in {Delay}", ex.Message, ConnectRetryDelay);
                    await Task.Delay(ConnectRetryDelay, stoppingToken);
                }
            }
        }

        private IModel StartConsuming(CancellationToken stoppingToken)
        {
            var channel = _serviceProvider.GetRequiredService<IConnection>().CreateModel();
            try
            {
                var queueName = _options.QueueName;
                var deadLetterQueueName = $"{queueName}.dead-letter";

                // Rejected messages are routed through the default exchange to a parking queue
                // so they can be inspected and replayed instead of being lost.
                channel.QueueDeclare(queue: deadLetterQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false,
                    arguments: new Dictionary<string, object>
                    {
                        ["x-dead-letter-exchange"] = string.Empty,
                        ["x-dead-letter-routing-key"] = deadLetterQueueName
                    });

                foreach (var subscription in _options.Subscriptions)
                {
                    channel.ExchangeDeclare(exchange: subscription.ExchangeName, type: ExchangeType.Direct, durable: true);
                    channel.QueueBind(queue: queueName, exchange: subscription.ExchangeName, routingKey: subscription.RoutingKey);
                }

                channel.BasicQos(prefetchSize: 0, prefetchCount: PrefetchCount, global: false);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += (_, ea) => OnReceivedAsync(channel, ea, stoppingToken);
                channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

                return channel;
            }
            catch
            {
                channel.Dispose();
                throw;
            }
        }

        private async Task OnReceivedAsync(IModel channel, BasicDeliverEventArgs ea, CancellationToken stoppingToken)
        {
            var subscription = _options.Subscriptions.FirstOrDefault(s =>
                s.ExchangeName == ea.Exchange && s.RoutingKey == ea.RoutingKey);

            var @event = subscription is null ? null : Deserialize(ea, subscription.EventType);
            if (subscription is null || @event is null || !@event.IsValidVersion())
            {
                // Retrying cannot fix a message we do not understand.
                _logger.LogError("Dead-lettering unreadable or unsupported message {MessageId} ({RoutingKey})",
                    ea.BasicProperties.MessageId, ea.RoutingKey);
                channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            try
            {
                await using var scope = _serviceProvider.CreateAsyncScope();
                await subscription.Dispatch(scope.ServiceProvider, @event, stoppingToken);
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                // Retry once in case the failure was transient, then dead-letter.
                var requeue = !ea.Redelivered;
                _logger.LogError(ex, "Handling message {MessageId} ({RoutingKey}) failed; {Outcome}",
                    ea.BasicProperties.MessageId, ea.RoutingKey, requeue ? "requeueing" : "dead-lettering");
                channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: requeue);
            }
        }

        private EventBase? Deserialize(BasicDeliverEventArgs ea, Type eventType)
        {
            try
            {
                return (EventBase?)JsonSerializer.Deserialize(ea.Body.Span, eventType);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Message {MessageId} is not a valid {EventType}", ea.BasicProperties.MessageId, eventType.Name);
                return null;
            }
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            base.Dispose();
        }
    }
}
