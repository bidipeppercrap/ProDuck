namespace ProDuck.DTO
{
    public class ClaimDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;

        public List<UserDTO> Users { get; set; } = new();
    }
}
