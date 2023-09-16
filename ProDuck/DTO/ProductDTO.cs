using ProDuck.Models;

namespace ProDuck.DTO;
public class ProductDTO
{
    public long? Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
    public string? Barcode { get; set; }
    public long? CategoryId { get; set; }
    public ProductCategoryDTO? Category { get; set; }
}