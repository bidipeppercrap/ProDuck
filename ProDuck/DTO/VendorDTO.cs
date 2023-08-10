namespace ProDuck.DTO
{
    public class VendorDTO
    {
        public long? Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
    }
}
