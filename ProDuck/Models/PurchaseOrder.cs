namespace ProDuck.Models
{
    public class PurchaseOrder
    {
        public long Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public int Quantity { get; set; }

        public long PurchaseId { get; set; }
        public Purchase Purchase { get; set; } = null!;

        public long ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public ICollection<LandedCostItem> LandedCostItems { get; } = new List<LandedCostItem>();
    }
}
