namespace ProDuck.Types
{
    public class ValidationResult
    {
        public bool IsValid { get; set; } = false;
        public List<string> ErrorMessages { get; set; } = new List<string>();
    }
}
