using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using NotificationService.Core.Interfaces;
using SharedEvents.Events;

namespace NotificationService.Infrastructure.Messaging
{
    public class UserCreatedEventHandler : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IModel _channel;
        private readonly string _queueName;

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
            _queueName = configuration["RabbitMQ:QueueNames:UserCreatedQueue"];
            
            _channel.QueueDeclare(queue: _queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
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
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                        notificationService.Notify(userCreatedEvent.Email,
                                            "Welcome to Our Service",
                                             $"Hi {userCreatedEvent.Name} Thank you for creating an account!");
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
