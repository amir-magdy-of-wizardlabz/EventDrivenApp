using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using NotificationService.Core.Interfaces;
using NotificationService.Infrastructure.Data;
using SharedEvents.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationService.Infrastructure.Messaging
{
    public class OrderCreatedEventHandler : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IModel _channel;
        private readonly string _queueName;
        private readonly string _exchangeName;
        private readonly string _routingKey;

        public OrderCreatedEventHandler(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            Console.WriteLine("Notification Service: OrderCreatedEventHandler is running and waiting for messages...");

            _serviceProvider = serviceProvider;

            var factory = new ConnectionFactory()
            {
                HostName = configuration["RabbitMQ:HostName"] ?? throw new ArgumentNullException("RabbitMQ:HostName"),
                UserName = configuration["RabbitMQ:UserName"] ?? throw new ArgumentNullException("RabbitMQ:UserName"),
                Password = configuration["RabbitMQ:Password"] ?? throw new ArgumentNullException("RabbitMQ:Password")
            };

            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();
            _queueName = configuration["RabbitMQ:OrderSubscriptionQueue:Queue"] ?? throw new ArgumentNullException("RabbitMQ:OrderSubscriptionQueue:Queue");
            _exchangeName = configuration["RabbitMQ:OrderSubscriptionQueue:ExchangeName"] ?? throw new ArgumentNullException("RabbitMQ:OrderSubscriptionQueue:ExchangeName");
            _routingKey = configuration["RabbitMQ:OrderSubscriptionQueue:RoutingKey"] ?? throw new ArgumentNullException("RabbitMQ:OrderSubscriptionQueue:RoutingKey");

            _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Direct);
            _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: _queueName, exchange: _exchangeName, routingKey: _routingKey);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(message);

                if (orderCreatedEvent != null)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
                        var user = await dbContext.Users.FindAsync(orderCreatedEvent.UserId);

                        if (user != null)
                        {
                            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                            notificationService.Notify(user.Email, "Order Confirmation", $"Your order with ID {orderCreatedEvent.OrderId} has been created successfully!");
                        }
                        else
                        {
                            Console.WriteLine($"User with ID {orderCreatedEvent.UserId} not found.");
                        }
                    }
                }

                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);

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
