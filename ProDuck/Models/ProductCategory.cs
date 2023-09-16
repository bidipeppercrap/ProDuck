namespace ProDuck.Models
{
    public class ProductCategory
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public long? ProductCategoryId { get; set; }

        public ProductCategory? ParentCategory { get; set; }
        public ICollection<ProductCategory> ChildCategories { get; } = new List<ProductCategory>();
        public ICollection<Product> Products { get; } = new List<Product>();
    }
}
