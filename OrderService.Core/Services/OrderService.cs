using OrderService.Core.Entities;
using OrderService.Core.Interfaces;
using SharedEvents;
using SharedEvents.Events;

namespace OrderService.Core.Services
{
    public class OrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IEventPublisher _eventPublisher;

        private const string EVENT_VERSION = "1.0";

        public OrderService(IOrderRepository orderRepository, IEventPublisher eventPublisher)
        {
            _orderRepository = orderRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<Order?> GetOrderById(int id)
        {
            return await _orderRepository.GetOrderByIdAsync(id);
        }

        public async Task<IEnumerable<Order>> GetAllOrders()
        {
            return await _orderRepository.GetAllOrdersAsync();
        }

        // Returns null when the user is not known to this service (yet).
        public async Task<Order?> AddOrder(int userId, string productName, int quantity, decimal price)
        {
            if (!await _orderRepository.UserExistsAsync(userId))
            {
                return null;
            }

            var order = new Order
            {
                UserId = userId,
                ProductName = productName,
                Quantity = quantity,
                Price = price,
                OrderDate = DateTime.UtcNow
            };
            await _orderRepository.AddOrderAsync(order);

            // Same dual-write gap as in UserService: saved but not published if the broker is down.
            _eventPublisher.Publish(Exchanges.Orders, RoutingKeys.OrderCreated, new OrderCreatedEvent
            {
                OrderId = order.Id,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                TotalAmount = order.Price,
                Version = EVENT_VERSION
            });

            return order;
        }

        public async Task<bool> UpdateOrder(int id, string productName, int quantity, decimal price)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
            {
                return false;
            }

            order.ProductName = productName;
            order.Quantity = quantity;
            order.Price = price;
            await _orderRepository.UpdateOrderAsync(order);

            return true;
        }

        public async Task<bool> DeleteOrder(int id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
            {
                return false;
            }

            await _orderRepository.DeleteOrderAsync(order);
            return true;
        }
    }
}
