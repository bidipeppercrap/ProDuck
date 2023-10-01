using ProDuck.Models;

namespace ProDuck.DTO
{
    public class PurchaseDTO
    {
        public long? Id { get; set; }
        public long VendorId { get; set; }
        public DateOnly Date { get; set; }
        public string SourceDocument { get; set; } = string.Empty;
        public string Memo { get; set; } = string.Empty;

        public decimal TotalCost { get; set; }

        public VendorDTO? Vendor { get; set; }
    }
}
