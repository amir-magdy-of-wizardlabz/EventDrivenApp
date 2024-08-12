using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using NotificationService.Core.Interfaces;
using NotificationService.Core.Entities;
using NotificationService.Infrastructure.Data;
using SharedEvents.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationService.Infrastructure.Messaging
{
    public class UserCreatedEventHandler : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IModel _channel;
        private readonly string _queueName;
        private readonly string _exchangeName;
        private readonly string _routingKey;

        public UserCreatedEventHandler(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            Console.WriteLine("Notification User Consumer is running and waiting for messages...");

            _serviceProvider = serviceProvider;

            var factory = new ConnectionFactory()
            {
                HostName = configuration["RabbitMQ:HostName"],
                UserName = configuration["RabbitMQ:UserName"],
                Password = configuration["RabbitMQ:Password"]
            };

            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();
            _queueName = configuration["RabbitMQ:QueueNames:UserCreatedQueue"] ?? "notificationservice.user.created";
            _exchangeName = configuration["RabbitMQ:ExchangeName"] ?? "UserExchange";
            _routingKey = configuration["RabbitMQ:RoutingKey"] ?? "user.created";

            // Declare the exchange
            Console.WriteLine($"Declaring exchange {_exchangeName}");
            _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Direct);

            // Declare the queue
            Console.WriteLine($"Declaring queue {_queueName}");
            _channel.QueueDeclare(queue: _queueName,
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);

            // Bind the queue to the exchange with the routing key
            Console.WriteLine($"Binding queue {_queueName} to exchange {_exchangeName} with routing key {_routingKey}");
            try
            {
                _channel.QueueBind(queue: _queueName,
                                   exchange: _exchangeName,
                                   routingKey: _routingKey);
                Console.WriteLine("Queue successfully bound to exchange");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to bind queue to exchange: {ex.Message}");
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
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
                        // Save user information to the database
                        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
                        var user = new User
                        {
                            Id = userCreatedEvent.Id,
                            Email = userCreatedEvent.Email
                        };
                        dbContext.Users.Add(user);
                        await dbContext.SaveChangesAsync();

                        // Notify the user via email
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                        notificationService.Notify(userCreatedEvent.Email,
                                            "Welcome to Our Service",
                                             $"Hi {userCreatedEvent.Name}, thank you for creating an account!");
                    }
                }

                // Acknowledge that the message has been processed
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
