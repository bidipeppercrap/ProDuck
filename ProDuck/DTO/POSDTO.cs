namespace ProDuck.DTO
{
    public class POSDTOSession
    {
        public long Id { get; set; }
        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        public override string ToString()
        {
            return ClosedAt == null ? $"Current session since {OpenedAt}" : $"{OpenedAt} - {ClosedAt}";
        }
    }
    public class POSDTOAssignedUser
    {
        public long? Id { get; set; }
        public string Username { get; set; } = null!;
        public string? Name { get; set; } = null;

        public override string ToString()
        {
            return Name ?? Username;
        }
    }
    public class POSDTO
    {
        public long? Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public POSDTOSession? LastSession { get; set; }
        public List<POSDTOAssignedUser> AssignedUsers { get; set; } = new List<POSDTOAssignedUser>();
    }
}
