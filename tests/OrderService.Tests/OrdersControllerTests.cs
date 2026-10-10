using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderService.Api.Contracts;
using OrderService.Api.Controllers;

namespace OrderService.Tests
{
    public class OrdersControllerTests
    {
        private const int KnownUserId = 7;

        private readonly OrdersController _controller = new(new Core.Services.OrderService(
            new InMemoryOrderRepository { KnownUserIds = { KnownUserId } },
            new RecordingEventPublisher()));

        [Fact]
        public async Task Creating_an_order_returns_201()
        {
            var result = await _controller.AddOrder(new CreateOrderRequest(KnownUserId, "Laptop", 1, 1500m));

            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task Creating_an_order_for_an_unknown_user_returns_422()
        {
            var result = await _controller.AddOrder(new CreateOrderRequest(999, "Laptop", 1, 1500m));

            var problem = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, problem.StatusCode);
        }

        [Fact]
        public async Task Updating_an_existing_order_returns_204()
        {
            await _controller.AddOrder(new CreateOrderRequest(KnownUserId, "Laptop", 1, 1500m));

            var result = await _controller.UpdateOrder(1, new UpdateOrderRequest("Laptop", 2, 3000m));

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task A_missing_order_returns_404_for_get_update_and_delete()
        {
            Assert.IsType<NotFoundResult>(await _controller.GetOrder(42));
            Assert.IsType<NotFoundResult>(await _controller.UpdateOrder(42, new UpdateOrderRequest("Laptop", 1, 1m)));
            Assert.IsType<NotFoundResult>(await _controller.DeleteOrder(42));
        }
    }
}
