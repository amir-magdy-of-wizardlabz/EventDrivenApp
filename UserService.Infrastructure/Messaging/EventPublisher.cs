using RabbitMQ.Client;
using System.Text;
using UserService.Core.Interfaces;

namespace UserService.Infrastructure.Messaging
{
    public class EventPublisher : IEventPublisher
    {
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;

        public EventPublisher(string hostname, string username, string password)
        {
            _hostname = hostname;
            _username = username;
            _password = password;
        }

        public void Publish(string queueName, string message)
        {
            var factory = new ConnectionFactory() { HostName = _hostname, UserName = _username, Password = _password };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: queueName,
                                     basicProperties: null,
                                     body: body);
            }
        }
    }
}
