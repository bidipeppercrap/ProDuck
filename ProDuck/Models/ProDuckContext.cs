using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;

namespace ProDuck.Models
{
    public class ProDuckContext : DbContext
    {
        public ProDuckContext(DbContextOptions<ProDuckContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<PointOfSale> PointOfSale { get; set; } = null!;
        public DbSet<POSSession> POSSession { get; set; } = null!;
        public DbSet<Location> Locations { get; set; } = null!;
        public DbSet<StockLocation> StockLocation { get; set; } = null!;
        public DbSet<Purchase> Purchases { get; set; } = null!;
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; } = null!;
        public DbSet<Vendor> Vendors { get; set; } = null!;
        public DbSet<ProductCategory> ProductCategories { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<CustomerPrice> CustomerPrice { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrdersItem { get; set; } = null!;
        public DbSet<Claim> Claims { get; set; } = null!;
        public DbSet<LandedCost> LandedCosts { get; set; } = null!;
        public DbSet<LandedCostItem> LandedCostItems { get; set; } = null!;
    }
}
