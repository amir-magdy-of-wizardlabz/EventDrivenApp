using OrderService.Core.Entities;
using OrderService.Core.Interfaces;
using SharedEvents.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace OrderService.Infrastructure.Messaging
{
    public class OrderServicePublisher : IOrderPublisher
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        private const string EVENT_VERSION = "1.0";
        private readonly string _exchangeName;
        private readonly string _routingKey;

        public OrderServicePublisher(IOrderRepository orderRepository, IConfiguration configuration)
        {
            _orderRepository = orderRepository;

            var rabbitMqHostName = configuration["RabbitMQ:HostName"] ?? throw new ArgumentNullException("RabbitMQ:HostName");
            var rabbitMqUserName = configuration["RabbitMQ:UserName"] ?? throw new ArgumentNullException("RabbitMQ:UserName");
            var rabbitMqPassword = configuration["RabbitMQ:Password"] ?? throw new ArgumentNullException("RabbitMQ:Password");
            _exchangeName = configuration["RabbitMQ:OrderPublishQueue:ExchangeName"] ?? throw new ArgumentNullException("RabbitMQ:OrderPublishQueue:ExchangeName");
            _routingKey = configuration["RabbitMQ:OrderPublishQueue:RoutingKey"] ?? throw  new ArgumentNullException("RabbitMQ:OrderPublishQueue:RoutingKey");

            var factory = new ConnectionFactory()
            {
                HostName = rabbitMqHostName,
                UserName = rabbitMqUserName,
                Password = rabbitMqPassword
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare the exchange
            _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Direct);
        }

        public async Task CreateOrder(Order order)
        {
            await _orderRepository.AddOrderAsync(order);

            // Publish an event to RabbitMQ after the order is successfully created
            var orderCreatedEvent = new OrderCreatedEvent
            {
                OrderId = order.Id,
                UserId = order.UserId,
                TotalAmount = order.Price,
                OrderDate = order.OrderDate,
                Version = EVENT_VERSION
            };

            PublishOrderCreatedEvent(orderCreatedEvent);
        }

        private void PublishOrderCreatedEvent(OrderCreatedEvent orderCreatedEvent)
        {
            var message = JsonSerializer.Serialize(orderCreatedEvent);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: _exchangeName,
                                  routingKey: _routingKey,
                                  basicProperties: null,
                                  body: body);
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
