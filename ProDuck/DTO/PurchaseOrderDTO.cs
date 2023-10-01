namespace ProDuck.DTO
{
    public class PurchaseOrderBaseDTO
    {
        public string Description { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public int Quantity { get; set; }

        public long PurchaseId { get; set; }

        public long ProductId { get; set; }
    }
    public class PurchaseOrderCreateDTO : PurchaseOrderBaseDTO
    {

    }
    public class PurchaseOrderDTO : PurchaseOrderBaseDTO
    {
        public long Id { get; set; }
        public int Delivered { get; set; } = 0;

        public ProductDTO Product { get; set; } = new();
    }
}
