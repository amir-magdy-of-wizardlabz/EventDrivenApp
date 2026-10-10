using System.ComponentModel.DataAnnotations;

namespace OrderService.Api.Contracts
{
    // The id and the order date are set by the server, and an order cannot move to another user.
    public record CreateOrderRequest(
        [Range(1, int.MaxValue)] int UserId,
        [Required, MaxLength(200)] string ProductName,
        [Range(1, int.MaxValue)] int Quantity,
        [Range(0, double.MaxValue)] decimal Price);

    public record UpdateOrderRequest(
        [Required, MaxLength(200)] string ProductName,
        [Range(1, int.MaxValue)] int Quantity,
        [Range(0, double.MaxValue)] decimal Price);
}
