namespace ProDuck.DTO
{
    public class ProductCategoryDTO
    {
        public long? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public long? ProductCategoryId { get; set; }
        public int? ProductsCount { get; set; }
    }
}
