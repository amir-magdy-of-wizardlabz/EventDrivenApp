using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using SharedEvents.Events;
using System.Text;
using System.Text.Json;
using UserService.Core.Interfaces;

namespace UserService.Infrastructure.Messaging
{
    public class EventPublisher : IEventPublisher
    {
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;

        public EventPublisher(IConfiguration configuration)
        {
            _hostname = configuration["RabbitMQ:HostName"] ?? throw new ArgumentNullException("RabbitMQ:HostName");
            _username = configuration["RabbitMQ:UserName"] ?? throw new ArgumentNullException("RabbitMQ:UserName");
            _password = configuration["RabbitMQ:Password"] ?? throw new ArgumentNullException("RabbitMQ:Password");
        }
       
        void IEventPublisher.Publish<T>(string exchangeName, string routingKey, T userEvent)
        {
            var factory = new ConnectionFactory() { HostName = _hostname, UserName = _username, Password = _password };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // Declare the exchange
                channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct);

                var message = JsonSerializer.Serialize(userEvent); // Serialize event object to JSON
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: exchangeName,
                                     routingKey: routingKey,
                                     basicProperties: null,
                                     body: body);
            }
        }
    }
}
