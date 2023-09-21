namespace ProDuck.DTO
{
    public class UserDTOClaim
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
    }
    public class UserDTO
    {
        public long? Id { get; set; }
        public string? Name { get; set; }
        public string Username { get; set; } = null!;

        public List<UserDTOClaim>? Claims { get; set; } = new();
    }
    public class UserPOSAssignDTO
    {
        public long UserId { get; set; }
        public long POSId { get; set; }
    }
}
