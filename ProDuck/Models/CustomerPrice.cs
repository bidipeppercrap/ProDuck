namespace ProDuck.Models
{
    public class CustomerPrice
    {
        public long Id { get; set; }
        public int MinQty { get; set; }
        public decimal Price { get; set; }

        public long CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public long ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}
