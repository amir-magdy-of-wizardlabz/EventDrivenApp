namespace OrderService.Core.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public required string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime OrderDate { get; set; }
        public int UserId { get; set; }

    }
}
