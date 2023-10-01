namespace ProDuck.Models
{
    public class Purchase
    {
        public long? Id { get; set; }
        public long VendorId { get; set; }
        public DateOnly Date { get; set; }
        public string SourceDocument { get; set; } = string.Empty;
        public string Memo { get; set; } = string.Empty;

        public Vendor Vendor { get; set; } = null!;
        public ICollection<PurchaseOrder> Orders { get; } = new List<PurchaseOrder>();
    }
}
