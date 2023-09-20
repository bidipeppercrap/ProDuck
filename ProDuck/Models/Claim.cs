using Microsoft.EntityFrameworkCore;

namespace ProDuck.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Claim
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;

        public List<User> Users { get; } = new();
    }
}
