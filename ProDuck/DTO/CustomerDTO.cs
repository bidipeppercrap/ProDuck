namespace ProDuck.DTO
{
    public class CustomerDTOProduct
    {
        public long? Id { get; set; }
        public string Name { get; set; } = null!;
    }
    public class CustomerDTOProductPrice
    {
        public long Id { get; set; }
        public CustomerDTOProduct Product { get; set; } = null!;
        public int MinQty { get; set; }
        public decimal Price { get; set; }
    }
    public class CustomerDTO
    {
        public long? Id { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = "";
        public string Phone { get; set; } = "";
        public ICollection<CustomerDTOProductPrice> Prices { get; set; } = new List<CustomerDTOProductPrice>();
    }
}
