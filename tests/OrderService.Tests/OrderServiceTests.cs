using SharedEvents;
using SharedEvents.Events;

namespace OrderService.Tests
{
    public class OrderServiceTests
    {
        private const int KnownUserId = 7;

        private readonly InMemoryOrderRepository _repository = new() { KnownUserIds = { KnownUserId } };
        private readonly RecordingEventPublisher _publisher = new();
        private readonly Core.Services.OrderService _service;

        public OrderServiceTests()
        {
            _service = new Core.Services.OrderService(_repository, _publisher);
        }

        [Fact]
        public async Task AddOrder_saves_the_order_and_publishes_order_created()
        {
            var before = DateTime.UtcNow;

            var order = await _service.AddOrder(KnownUserId, "Laptop", 1, 1500m);

            Assert.NotNull(order);
            Assert.Same(order, await _repository.GetOrderByIdAsync(order.Id));
            Assert.Equal(DateTimeKind.Utc, order.OrderDate.Kind);
            Assert.InRange(order.OrderDate, before, DateTime.UtcNow);

            var published = Assert.Single(_publisher.Published);
            Assert.Equal(Exchanges.Orders, published.ExchangeName);
            Assert.Equal(RoutingKeys.OrderCreated, published.RoutingKey);

            var @event = Assert.IsType<OrderCreatedEvent>(published.Event);
            Assert.Equal(order.Id, @event.OrderId);
            Assert.Equal(KnownUserId, @event.UserId);
            Assert.True(@event.IsValidVersion());
        }

        [Fact]
        public async Task AddOrder_for_an_unknown_user_saves_and_publishes_nothing()
        {
            var order = await _service.AddOrder(999, "Laptop", 1, 1500m);

            Assert.Null(order);
            Assert.Empty(await _repository.GetAllOrdersAsync());
            Assert.Empty(_publisher.Published);
        }

        [Fact]
        public async Task UpdateOrder_changes_the_details_but_not_the_owner_or_the_date()
        {
            var order = await _service.AddOrder(KnownUserId, "Laptop", 1, 1500m);
            var orderDate = order!.OrderDate;

            var updated = await _service.UpdateOrder(order.Id, "Laptop Pro", 2, 3000m);

            Assert.True(updated);
            Assert.Equal("Laptop Pro", order.ProductName);
            Assert.Equal(2, order.Quantity);
            Assert.Equal(3000m, order.Price);
            Assert.Equal(KnownUserId, order.UserId);
            Assert.Equal(orderDate, order.OrderDate);
        }

        [Fact]
        public async Task Update_and_delete_return_false_for_a_missing_order()
        {
            Assert.False(await _service.UpdateOrder(42, "Laptop", 1, 1m));
            Assert.False(await _service.DeleteOrder(42));
        }

        [Fact]
        public async Task DeleteOrder_removes_the_order()
        {
            var order = await _service.AddOrder(KnownUserId, "Laptop", 1, 1500m);

            Assert.True(await _service.DeleteOrder(order!.Id));
            Assert.Null(await _repository.GetOrderByIdAsync(order.Id));
        }
    }
}
