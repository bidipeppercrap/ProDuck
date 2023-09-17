namespace ProDuck.Models
{
    public class PointOfSale
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsDeleted { get; set; } = false;

        public ICollection<POSSession> Sessions { get; } = new List<POSSession>();

        public List<User> AssignedUsers { get; } = new();
    }
}
