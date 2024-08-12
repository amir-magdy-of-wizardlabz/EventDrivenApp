using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using OrderService.Core.Entities;
using OrderService.Core.Events;
using OrderService.Infrastructure.Data;
using System.Text;
using System.Text.Json;

namespace OrderService.Infrastructure.Messaging
{
    public class UserCreatedEventHandler : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IModel _channel;
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;
        private readonly string _queueName;

        public UserCreatedEventHandler(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;

            _hostname = configuration["RabbitMQ:HostName"] ?? throw new ArgumentNullException(nameof(_hostname), "RabbitMQ HostName configuration is missing");
            _username = configuration["RabbitMQ:UserName"] ?? throw new ArgumentNullException(nameof(_username), "RabbitMQ UserName configuration is missing");
            _password = configuration["RabbitMQ:Password"] ?? throw new ArgumentNullException(nameof(_password), "RabbitMQ Password configuration is missing");
            _queueName = configuration["RabbitMQ:QueueNames:UserCreatedQueue"]?? throw new ArgumentNullException(nameof(_password), "RabbitMQ QueueName configuration is missing");


            var factory = new ConnectionFactory()
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password
            };

            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();
            _channel.QueueDeclare(queue: _queueName,
                                 durable: false,
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
                        var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

                        var user = new User { Id = userCreatedEvent.UserId };
                        dbContext.Users.Add(user);
                        await dbContext.SaveChangesAsync();
                    }
                }
            };

            _channel.BasicConsume(queue: _queueName,
                                  autoAck: true,
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
