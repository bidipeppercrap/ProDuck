namespace ProDuck.DTO
{
    public class ProductStockDTO
    {
        public long Id { get; set; }
        public int Stock { get; set; }
        public ProductStockDTOProduct Product { get; set; } = new ProductStockDTOProduct();
    }

    public class ProductStockDTOProduct
    {
        public long? Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class LocationStockDTO
    {
        public long Id { get; set; }
        public int Stock { get; set; }
        public LocationStockDTOLocation? Location { get; set; } = new LocationStockDTOLocation();
    }

    public class LocationStockDTOLocation
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
