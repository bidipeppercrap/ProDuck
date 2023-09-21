namespace ProDuck.DTO
{
    public class SaleItemDTO
    {
        public long? ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal TotalSalePrice = 0;
        public decimal TotalSaleCost = 0;
        public int TotalSold = 0;
    }
}
