using Microsoft.Build.ObjectModelRemoting;

namespace ProDuck.DTO
{
    public class LocationDTO
    {
        public long? Id { get; set; }
        public string Name { get; set; } = null!;
        public LocationDTO? ParentLocation { get; set; }
        public long? LocationId { get; set; }

    }
}
