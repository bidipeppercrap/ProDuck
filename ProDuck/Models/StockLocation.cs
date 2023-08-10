namespace ProDuck.Models
{
    public class StockLocation
    {
        public long Id { get; set; }
        public int Stock { get; set; }

        public long ProductId { get; set; }
        public long? LocationId { get; set; }
        public Product Product { get; set; } = null!;
        public Location? Location { get; set; }
    }
}
