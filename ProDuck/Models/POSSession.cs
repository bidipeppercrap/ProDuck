namespace ProDuck.Models
{
    public class POSSession
    {
        public long Id { get; set; }
        public string? Note { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime ClosedAt { get; set; }

        public long UserId { get; set; }
        public User SessionOpener { get; set; } = null!;
        public long POSId { get; set; }
        public PointOfSale POS { get; set; } = null!;
    }
}
