namespace ProDuck.DTO
{
    public class LocationStockProductDTO
    {
        public long? Id { get; set; }
        public string Name { get; set; } = null!;
    }
    public class LocationStockDTO
    {
        public long? Id { get; set; }
        public int Stock { get; set; }
        public LocationStockProductDTO Product { get; set; } = null!;
    }

    public class LocationStockListDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<LocationStockDTO>? Stocks { get; set; }
    }
}
