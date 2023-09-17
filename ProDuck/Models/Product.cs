using Microsoft.EntityFrameworkCore;

namespace ProDuck.Models
{
    [Index(nameof(Barcode), IsUnique = true)]
    public class Product
    {
        public long? Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public string? Barcode { get; set; }
        public bool Deleted { get; set; } = false;

        public ProductCategory? Category { get; set; }

        public ICollection<StockLocation> Stocks { get; } = new List<StockLocation>();
        public ICollection<CustomerPrice> CustomerPrices { get; } = new List<CustomerPrice>();
        public ICollection<OrderItem> OrderItems { get; } = new List<OrderItem>();
    }
}
