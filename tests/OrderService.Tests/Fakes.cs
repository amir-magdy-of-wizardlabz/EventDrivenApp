using OrderService.Core.Entities;
using OrderService.Core.Interfaces;
using SharedEvents;
using SharedEvents.Events;

namespace OrderService.Tests
{
    public record PublishedEvent(string ExchangeName, string RoutingKey, EventBase Event);

    public class RecordingEventPublisher : IEventPublisher
    {
        public List<PublishedEvent> Published { get; } = new();

        public void Publish<T>(string exchangeName, string routingKey, T @event) where T : EventBase
        {
            Published.Add(new PublishedEvent(exchangeName, routingKey, @event));
        }
    }

    public class InMemoryOrderRepository : IOrderRepository
    {
        private readonly List<Order> _orders = new();
        private int _nextId = 1;

        public HashSet<int> KnownUserIds { get; } = new();

        public Task<Order?> GetOrderByIdAsync(int id) => Task.FromResult(_orders.SingleOrDefault(o => o.Id == id));

        public Task<IEnumerable<Order>> GetAllOrdersAsync() => Task.FromResult<IEnumerable<Order>>(_orders.ToList());

        public Task AddOrderAsync(Order order)
        {
            order.Id = _nextId++;
            _orders.Add(order);
            return Task.CompletedTask;
        }

        public Task UpdateOrderAsync(Order order) => Task.CompletedTask;

        public Task DeleteOrderAsync(Order order)
        {
            _orders.Remove(order);
            return Task.CompletedTask;
        }

        public Task<bool> UserExistsAsync(int userId) => Task.FromResult(KnownUserIds.Contains(userId));
    }
}
