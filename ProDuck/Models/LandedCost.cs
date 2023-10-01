namespace ProDuck.Models
{
    public class LandedCost
    {
        public long Id { get; set; }
        public string Biller { get; set; } = string.Empty;
        public bool IsDelivered { get; set; } = false;
        public bool IsPurchase { get; set; } = true;

        public long? SourceLocationId { get; set; }
        public Location SourceLocation { get; set; } = new();

        public long? TargetLocationId { get; set; }
        public Location TargetLocation { get; set; } = new();

        public ICollection<LandedCostItem> Items { get; } = new List<LandedCostItem>();
    }
}
