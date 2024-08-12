using OrderService.Core.Entities;
using OrderService.Core.Interfaces;
using System;

namespace OrderService.Core.Services
{
    public class OrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
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
