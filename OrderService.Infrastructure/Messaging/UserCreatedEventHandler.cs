using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using OrderService.Infrastructure.Data;
using OrderService.Core.Entities;
using System.Text;
using System.Text.Json;
using SharedEvents.Events;

namespace OrderService.Infrastructure.Messaging
{
    public class UserCreatedEventHandler : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IModel _channel;
        private readonly string _queueName;
        private readonly string _exchangeName;

        public UserCreatedEventHandler(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;

            var factory = new ConnectionFactory()
            {
                HostName = configuration["RabbitMQ:HostName"],
                UserName = configuration["RabbitMQ:UserName"],
                Password = configuration["RabbitMQ:Password"]
            };

            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();
            _queueName = configuration["RabbitMQ:QueueNames:UserCreatedQueue"] ?? "orderservice.user.created";
            _exchangeName = configuration["RabbitMQ:ExchangeName"] ?? "UserExchange";
            var routingKey = configuration["RabbitMQ:RoutingKey"] ?? "user.created";

            // Declare the exchange
            _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Direct);

            // Declare the queue
            _channel.QueueDeclare(queue: _queueName,
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);

            // Bind the queue to the exchange with the routing key
            _channel.QueueBind(queue: _queueName,
                               exchange: _exchangeName,
                               routingKey: routingKey);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Order User Consumer is running and waiting for messages...");

            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var userCreatedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(message);

                if (userCreatedEvent != null)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

                        var user = new User { Id = userCreatedEvent.Id };
                        dbContext.Users.Add(user);
                        await dbContext.SaveChangesAsync();
                    }
                }

                // Acknowledge the message
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            _channel.BasicConsume(queue: _queueName,
                                  autoAck: false,
                                  consumer: consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            base.Dispose();
        }
    }
}
