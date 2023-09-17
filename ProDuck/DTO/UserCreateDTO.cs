namespace ProDuck.DTO
{
    public class UserCreateDTO
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
