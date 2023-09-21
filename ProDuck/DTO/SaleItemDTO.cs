namespace ProDuck.DTO
{
    public class SaleItemDTO
    {
        public long? ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal TotalSalePrice { get; set; } = 0;
        public decimal TotalSaleCost { get; set; } = 0;
        public int TotalSold { get; set; } = 0;
    }
}
