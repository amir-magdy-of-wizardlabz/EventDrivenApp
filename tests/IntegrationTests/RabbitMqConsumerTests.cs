using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using SharedEvents;
using SharedEvents.Events;
using SharedMessaging;

namespace IntegrationTests
{
    public class RabbitMqConsumerTests : IClassFixture<RabbitMqFixture>, IAsyncLifetime
    {
        private const string RoutingKey = "user.created";
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);

        private readonly RabbitMqFixture _rabbitMq;
        private readonly string _exchangeName = $"test-exchange-{Guid.NewGuid():N}";
        private readonly string _queueName = $"test-queue-{Guid.NewGuid():N}";
        private readonly HandlerProbe _probe = new();
        private IHost? _host;

        public RabbitMqConsumerTests(RabbitMqFixture rabbitMq)
        {
            _rabbitMq = rabbitMq;
        }

        private string DeadLetterQueueName => $"{_queueName}.dead-letter";

        // Runs the real publisher and consumer, wired the same way the services wire them.
        public async ValueTask InitializeAsync()
        {
            var builder = Host.CreateApplicationBuilder();
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RabbitMQ:HostName"] = _rabbitMq.HostName,
                ["RabbitMQ:Port"] = _rabbitMq.Port.ToString(),
                ["RabbitMQ:UserName"] = RabbitMqFixture.UserName,
                ["RabbitMQ:Password"] = RabbitMqFixture.Password
            });
            builder.Services.AddSingleton(_probe);
            builder.Services.AddRabbitMq(builder.Configuration);
            builder.Services.AddRabbitMqConsumer(_queueName, consumer => consumer
                .Subscribe<UserCreatedEvent, ProbeHandler>(_exchangeName, RoutingKey));

            _host = builder.Build();
            await _host.StartAsync();
            await WaitUntilAsync(ConsumerIsListening, "the consumer to start listening");
        }

        public async ValueTask DisposeAsync()
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
        }

        [Fact]
        public async Task A_published_event_reaches_the_handler()
        {
            Publish(new UserCreatedEvent { Id = 1, Name = "Ann", Email = "ann@example.com", Version = "1.0" });

            await WaitUntilAsync(() => _probe.Handled.Count == 1, "the event to be handled");

            var handled = Assert.Single(_probe.Handled);
            Assert.Equal(1, handled.Id);
            Assert.Equal("ann@example.com", handled.Email);
            Assert.Equal(0u, MessageCount(DeadLetterQueueName));
        }

        [Fact]
        public async Task A_handler_that_fails_once_is_retried_and_the_event_is_handled()
        {
            _probe.FailuresBeforeSuccess = 1;

            Publish(new UserCreatedEvent { Id = 1, Name = "Ann", Email = "ann@example.com", Version = "1.0" });

            await WaitUntilAsync(() => _probe.Handled.Count == 1, "the retried event to be handled");

            Assert.Equal(2, _probe.Attempts);
            Assert.Equal(0u, MessageCount(DeadLetterQueueName));
        }

        [Fact]
        public async Task A_handler_that_keeps_failing_gets_one_retry_and_then_the_message_is_dead_lettered()
        {
            _probe.FailuresBeforeSuccess = int.MaxValue;

            Publish(new UserCreatedEvent { Id = 1, Name = "Ann", Email = "ann@example.com", Version = "1.0" });

            await WaitUntilAsync(() => MessageCount(DeadLetterQueueName) == 1, "the message to be dead-lettered");

            Assert.Equal(2, _probe.Attempts);
            Assert.Empty(_probe.Handled);
        }

        [Fact]
        public async Task A_message_that_is_not_valid_json_is_dead_lettered_without_calling_the_handler()
        {
            PublishRaw("this is not json");

            await WaitUntilAsync(() => MessageCount(DeadLetterQueueName) == 1, "the message to be dead-lettered");

            Assert.Equal(0, _probe.Attempts);
        }

        [Fact]
        public async Task An_event_with_an_unsupported_version_is_dead_lettered_without_calling_the_handler()
        {
            Publish(new UserCreatedEvent { Id = 1, Name = "Ann", Email = "ann@example.com", Version = "99.0" });

            await WaitUntilAsync(() => MessageCount(DeadLetterQueueName) == 1, "the message to be dead-lettered");

            Assert.Equal(0, _probe.Attempts);
        }

        [Fact]
        public async Task The_consumer_keeps_working_after_a_bad_message()
        {
            PublishRaw("this is not json");
            Publish(new UserCreatedEvent { Id = 2, Name = "Bob", Email = "bob@example.com", Version = "1.0" });

            await WaitUntilAsync(() => _probe.Handled.Count == 1, "the good event to be handled");

            Assert.Equal(2, Assert.Single(_probe.Handled).Id);
        }

        private void Publish(UserCreatedEvent @event)
        {
            _host!.Services.GetRequiredService<IEventPublisher>().Publish(_exchangeName, RoutingKey, @event);
        }

        private void PublishRaw(string body)
        {
            using var channel = _rabbitMq.Connection.CreateModel();
            channel.BasicPublish(exchange: _exchangeName, routingKey: RoutingKey, basicProperties: null, body: Encoding.UTF8.GetBytes(body));
        }

        private uint MessageCount(string queueName)
        {
            using var channel = _rabbitMq.Connection.CreateModel();
            return channel.QueueDeclarePassive(queueName).MessageCount;
        }

        private bool ConsumerIsListening()
        {
            try
            {
                using var channel = _rabbitMq.Connection.CreateModel();
                return channel.QueueDeclarePassive(_queueName).ConsumerCount == 1;
            }
            catch (OperationInterruptedException)
            {
                // The queue does not exist yet.
                return false;
            }
        }

        private static async Task WaitUntilAsync(Func<bool> condition, string description)
        {
            var deadline = DateTime.UtcNow + Timeout;
            while (!condition())
            {
                Assert.True(DateTime.UtcNow < deadline, $"Timed out waiting for {description}.");
                await Task.Delay(100);
            }
        }

        public class HandlerProbe
        {
            private int _attempts;

            public int FailuresBeforeSuccess { get; set; }
            public int Attempts => _attempts;
            public ConcurrentQueue<UserCreatedEvent> Handled { get; } = new();

            public void Handle(UserCreatedEvent @event)
            {
                if (Interlocked.Increment(ref _attempts) <= FailuresBeforeSuccess)
                {
                    throw new InvalidOperationException("Simulated handler failure.");
                }

                Handled.Enqueue(@event);
            }
        }

        public class ProbeHandler : IEventHandler<UserCreatedEvent>
        {
            private readonly HandlerProbe _probe;

            public ProbeHandler(HandlerProbe probe)
            {
                _probe = probe;
            }

            public Task HandleAsync(UserCreatedEvent @event, CancellationToken cancellationToken)
            {
                _probe.Handle(@event);
                return Task.CompletedTask;
            }
        }
    }
}
