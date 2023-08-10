using Microsoft.EntityFrameworkCore;

namespace ProDuck.Models
{
    [Index(nameof(Barcode), IsUnique = true)]
    public class Product
    {
        public long? Id { get; set; }
        public string Name { get; set; } = null!;
        public int Price { get; set; }
        public int Cost { get; set; }
        public string? Barcode { get; set; }

        public ICollection<StockLocation> Stocks { get; } = new List<StockLocation>();
    }
}
