using OrderService.Core.Entities;
using OrderService.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedEvents.Events;

namespace OrderService.Core.Services
{
    public class OrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderPublisher _orderPublisher;

        public OrderService(IOrderRepository orderRepository, IOrderPublisher orderPublisher)
        {
            _orderRepository = orderRepository;
            _orderPublisher = orderPublisher;
        }

        public async Task<Order> GetOrderById(int id)
        {
            return await _orderRepository.GetOrderByIdAsync(id);
        }

        public async Task<IEnumerable<Order>> GetAllOrders()
        {
            return await _orderRepository.GetAllOrdersAsync();
        }

        public async Task AddOrder(Order order)
        {
            // Convert OrderDate to UTC before saving
            order.OrderDate = DateTime.SpecifyKind(order.OrderDate, DateTimeKind.Utc);
            await _orderRepository.AddOrderAsync(order);

            // Publish an OrderCreatedEvent after the order is successfully added
            var orderCreatedEvent = new OrderCreatedEvent
            {
                OrderId = order.Id,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                TotalAmount = order.Price,
                Version = "1.0"
            };

            await _orderPublisher.CreateOrder(orderCreatedEvent);
        }

        public async Task UpdateOrder(Order order)
        {
            // Convert OrderDate to UTC before updating
            order.OrderDate = DateTime.SpecifyKind(order.OrderDate, DateTimeKind.Utc);
            await _orderRepository.UpdateOrderAsync(order);
        }

        public async Task DeleteOrder(int id)
        {
            await _orderRepository.DeleteOrderAsync(id);
        }
    }
}
