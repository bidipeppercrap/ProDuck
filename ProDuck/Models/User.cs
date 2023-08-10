namespace ProDuck.Models
{
    public class User
    {
        public long? Id { get; set; }
        public string Username { get; set; } = null!;
        public string? Name { get; set; }
        public string Password { get; set; } = null!;

        public ICollection<POSSession> Sessions { get; } = new List<POSSession>();
    }
}
