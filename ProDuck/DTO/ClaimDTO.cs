namespace ProDuck.DTO
{
    public class ClaimDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;

        public List<UserDTO> Users { get; set; } = new();
    }
    public class UserClaimDTO
    {
        public long UserId { get; set; }
        public long ClaimId { get; set; }
    }
}
