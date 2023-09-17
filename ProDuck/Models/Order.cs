namespace ProDuck.Models
{
    public class Order
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public long UserId { get; set; }
        public User ServedBy { get; set; } = null!;

        public long? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public long POSSessionId { get; set; }
        public POSSession POSSession { get; set; } = null!;

        public ICollection<OrderItem> Items { get; } = new List<OrderItem>();
    }
}
