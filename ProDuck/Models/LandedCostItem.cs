namespace ProDuck.Models
{
    public class LandedCostItem
    {
        public long Id { get; set; }
        public int Qty { get; set; }
        public decimal Cost { get; set; }

        public long? PurchaseOrderId { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }
        
        public long? LandedCostId { get; set; }
        public LandedCost? LandedCost { get; set; }

        public long? LandedCostItemId { get; set; }
        public LandedCostItem? Parent { get; set; }

        public ICollection<LandedCostItem> Children { get; } = new List<LandedCostItem>();
    }
}
