using OrderService.Core.Entities;
using OrderService.Core.Interfaces;

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
            await _orderRepository.AddOrderAsync(order);
        }

        public async Task UpdateOrder(Order order)
        {
            await _orderRepository.UpdateOrderAsync(order);
        }

        public async Task DeleteOrder(int id)
        {
            await _orderRepository.DeleteOrderAsync(id);
        }
    }
}
