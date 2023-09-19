namespace ProDuck.DTO
{
    public class ProductStockLocationDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
    }
    public class ProductStockDTO
    {
        public long Id { get; set; }
        public int Stock { get; set; }
        public ProductStockLocationDTO? Location { get; set; }
    }
    public class ProductStockListDTO
    {
        public long? Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<ProductStockDTO>? Stocks { get; set; }
    }
}
