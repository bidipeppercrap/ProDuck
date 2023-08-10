namespace ProDuck.Models
{
    public class PurchaseOrder
    {
        public long Id { get; set; }
        public long PurchaseId { get; set; }
        public long ProductId { get; set; }
        public int Cost { get; set; }
        public int Quantity { get; set; }

        public Purchase Purchase { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
