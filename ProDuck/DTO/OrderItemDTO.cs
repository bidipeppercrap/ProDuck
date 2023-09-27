namespace ProDuck.DTO
{
    public class OrderItemDTOProduct
    {
        public long? Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Barcode { get; set; }
    }
    public class OrderItemDTO
    {
        public long Id { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public OrderItemDTOProduct Product { get; set; } = null!;
        public bool IsReturn { get; set; } = false;
    }
}
