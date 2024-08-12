using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace OrderService.Infrastructure.Messaging
{
    public class OrderRabbitMqSetupService : IHostedService
    {
        private readonly string _queueName;
        private readonly string _exchangeName;
        private readonly string _routingKey;
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;
        private IConnection _connection;
        private IModel _channel;

        public OrderRabbitMqSetupService(IConfiguration configuration)
        {
            try
            {
                Console.WriteLine("Initializing RabbitMQ Setup Service for Orders...");

                _hostname = configuration["RabbitMQ:HostName"] ?? throw new ArgumentNullException("RabbitMQ:HostName");
                _username = configuration["RabbitMQ:UserName"] ?? throw new ArgumentNullException("RabbitMQ:UserName");
                _password = configuration["RabbitMQ:Password"] ?? throw new ArgumentNullException("RabbitMQ:Password");
                _queueName = configuration["RabbitMQ:OrderPublishQueue:Queue"] ?? throw new ArgumentNullException("RabbitMQ:OrderPublishQueue:Queue");
                _exchangeName = configuration["RabbitMQ:OrderPublishQueue:ExchangeName"] ?? throw new ArgumentNullException("RabbitMQ:OrderPublishQueue:ExchangeName");
                _routingKey = configuration["RabbitMQ:OrderPublishQueue:RoutingKey"] ?? throw new ArgumentNullException("RabbitMQ:OrderPublishQueue:RoutingKey");

                var factory = new ConnectionFactory()
                {
                    HostName = _hostname,
                    UserName = _username,
                    Password = _password
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                Console.WriteLine("RabbitMQ connection and channel for Orders established.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed initialization of RabbitMQ for Orders: " + ex.Message);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine("RabbitMQ Setup Service for Orders is running...");

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
                Console.WriteLine("Error during RabbitMQ setup for Orders: " + ex.Message);
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
