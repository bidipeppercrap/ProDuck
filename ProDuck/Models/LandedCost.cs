namespace ProDuck.Models
{
    public class LandedCost
    {
        public long Id { get; set; }
        public DateOnly Date { get; set; }
        public string Biller { get; set; } = string.Empty;
        public bool IsDelivered { get; set; } = false;
        public bool IsPurchase { get; set; } = true;

        public long? SourceLocationId { get; set; }
        public Location? SourceLocation { get; set; }

        public long? TargetLocationId { get; set; }
        public Location? TargetLocation { get; set; }

        public ICollection<LandedCostItem> Items { get; } = new List<LandedCostItem>();
    }
}
