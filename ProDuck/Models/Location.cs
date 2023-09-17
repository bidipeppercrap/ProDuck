using System.Text.Json.Serialization;

namespace ProDuck.Models
{
    public class Location
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;

        public long? LocationId { get; set; }
        public Location? ParentLocation { get; set; }

        public ICollection<Location> ChildLocations { get; } = new List<Location>();
        public ICollection<StockLocation> Products { get; } = new List<StockLocation>();
    }
}
