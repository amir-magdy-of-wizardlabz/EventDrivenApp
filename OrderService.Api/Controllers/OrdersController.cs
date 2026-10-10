using Microsoft.AspNetCore.Mvc;
using OrderService.Api.Contracts;

namespace OrderService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService.Core.Services.OrderService _orderService;

        public OrdersController(OrderService.Core.Services.OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _orderService.GetAllOrders();
            return Ok(orders);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrder([FromBody] CreateOrderRequest request)
        {
            var order = await _orderService.AddOrder(request.UserId, request.ProductName, request.Quantity, request.Price);
            if (order == null)
            {
                // Users arrive here asynchronously, so a brand-new user may not be known for a moment.
                return Problem(
                    statusCode: StatusCodes.Status422UnprocessableEntity,
                    detail: $"User {request.UserId} is not known to the order service.");
            }

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderRequest request)
        {
            var updated = await _orderService.UpdateOrder(id, request.ProductName, request.Quantity, request.Price);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var deleted = await _orderService.DeleteOrder(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
