using OrderService.Core.Interfaces;
using SharedEvents.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Messaging
{
    public class OrderServicePublisher : IOrderPublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        private const string EVENT_VERSION = "1.0";
        private readonly string _exchangeName;
        private readonly string _routingKey;

        public OrderServicePublisher(IConfiguration configuration)
        {
            var rabbitMqHostName = configuration["RabbitMQ:HostName"] ?? throw new ArgumentNullException("RabbitMQ:HostName");
            var rabbitMqUserName = configuration["RabbitMQ:UserName"] ?? throw new ArgumentNullException("RabbitMQ:UserName");
            var rabbitMqPassword = configuration["RabbitMQ:Password"] ?? throw new ArgumentNullException("RabbitMQ:Password");
            _exchangeName = configuration["RabbitMQ:OrderPublishQueue:ExchangeName"] ?? throw new ArgumentNullException("RabbitMQ:OrderPublishQueue:ExchangeName");
            _routingKey = configuration["RabbitMQ:OrderPublishQueue:RoutingKey"] ?? throw new ArgumentNullException("RabbitMQ:OrderPublishQueue:RoutingKey");

            var factory = new ConnectionFactory()
            {
                HostName = rabbitMqHostName,
                UserName = rabbitMqUserName,
                Password = rabbitMqPassword
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Direct);
        }

        public Task CreateOrder(OrderCreatedEvent orderCreatedEvent)
        {
            orderCreatedEvent.Version = EVENT_VERSION;

            var message = JsonSerializer.Serialize(orderCreatedEvent);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: _exchangeName,
                                  routingKey: _routingKey,
                                  basicProperties: null,
                                  body: body);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
