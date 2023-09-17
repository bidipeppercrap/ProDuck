using Microsoft.EntityFrameworkCore;

namespace ProDuck.Models
{
    [Index(nameof(Username), IsUnique = true)]
    public class User
    {
        public long? Id { get; set; }
        public string Username { get; set; } = null!;
        public string? Name { get; set; }
        public string Password { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;

        public ICollection<POSSession> Sessions { get; } = new List<POSSession>();
        public ICollection<Order> OrdersServed { get; } = new List<Order>();

        public List<PointOfSale> AssignedPOSes { get; } = new();

    }
}
