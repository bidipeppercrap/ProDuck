namespace ProDuck.DTO;
public class ProductDTO
{
    public long? Id { get; set; }
    public string Name { get; set; } = null!;
    public int Price { get; set; }
    public int Cost { get; set; }
    public string? Barcode { get; set; }
}