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
    }
}
