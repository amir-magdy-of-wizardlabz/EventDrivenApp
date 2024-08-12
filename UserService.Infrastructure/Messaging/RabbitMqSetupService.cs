using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace UserService.Infrastructure.Messaging
{
    public class RabbitMqSetupService : IHostedService
    {
        private readonly string _queueName;
        private readonly string _exchangeName;
        private readonly string _routingKey;
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMqSetupService(IConfiguration configuration)
        {
            try
            {
                Console.WriteLine("Initializing RabbitMQ Setup Service...");

                _hostname = configuration["RabbitMQ:HostName"] ?? throw new ArgumentNullException("RabbitMQ:HostName");
                _username = configuration["RabbitMQ:UserName"] ?? throw new ArgumentNullException("RabbitMQ:UserName");
                _password = configuration["RabbitMQ:Password"] ?? throw new ArgumentNullException("RabbitMQ:Password");
                _queueName = configuration["RabbitMQ:UserPublishQueue:Queue"] ?? throw new ArgumentNullException("RabbitMQ:UserPublishQueue:Queue");
                _exchangeName = configuration["RabbitMQ:UserPublishQueue:ExchangeName"] ?? throw new ArgumentNullException("RabbitMQ:UserPublishQueue:ExchangeName");
                _routingKey = configuration["RabbitMQ:UserPublishQueue:RoutingKey"] ?? throw new ArgumentNullException("RabbitMQ:UserPublishQueue:RoutingKey");

                var factory = new ConnectionFactory()
                {
                    HostName = _hostname,
                    UserName = _username,
                    Password = _password
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                Console.WriteLine("RabbitMQ connection and channel established.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed initialization of RabbitMQ: " + ex.Message);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine("RabbitMQ Setup Service is running...");

                // Declare the exchange at startup
                _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Direct);
                Console.WriteLine($"Exchange {_exchangeName} declared successfully.");

                // Declare the queue at startup
                _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                Console.WriteLine($"Queue {_queueName} declared successfully.");

                // Bind the queue to the exchange with the routing key
                _channel.QueueBind(queue: _queueName, exchange: _exchangeName, routingKey: _routingKey);
                Console.WriteLine($"Queue {_queueName} bound to exchange {_exchangeName} with routing key {_routingKey}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during RabbitMQ setup: " + ex.Message);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
            return Task.CompletedTask;
        }
    }
}
