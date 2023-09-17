namespace ProDuck.Models
{
    public class POSSession
    {
        public long Id { get; set; }
        public string? ClosingRemark { get; set; }
        public DateTime OpenedAt { get; set; } = DateTime.Now;
        public DateTime? ClosedAt { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }

        public long UserId { get; set; }
        public User SessionOpener { get; set; } = null!;

        public long POSId { get; set; }
        public PointOfSale POS { get; set; } = null!;

        public ICollection<Order> Orders { get; } = new List<Order>();
    }
}
