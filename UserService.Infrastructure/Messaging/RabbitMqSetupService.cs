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
        private readonly string _exchangeName;
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMqSetupService(IConfiguration configuration)
        {
            _hostname = configuration["RabbitMQ:HostName"];
            _username = configuration["RabbitMQ:UserName"];
            _password = configuration["RabbitMQ:Password"];
            _exchangeName = configuration["RabbitMQ:ExchangeName"] ?? "UserExchange";

            var factory = new ConnectionFactory()
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Declare the exchange at startup
            _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Direct);
            Console.WriteLine($"Exchange {_exchangeName} declared successfully.");

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
