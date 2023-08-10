namespace ProDuck.Models
{
    public class Vendor
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;

        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }
}
