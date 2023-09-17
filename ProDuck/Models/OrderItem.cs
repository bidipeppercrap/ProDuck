namespace ProDuck.Models
{
    public class OrderItem
    {
        public long Id { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }

        public long ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public long OrderId { get; set; }
        public Order Order { get; set; } = null!;
    }
}
