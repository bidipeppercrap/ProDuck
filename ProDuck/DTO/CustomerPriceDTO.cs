namespace ProDuck.DTO
{
    public class CustomerPriceDTOCustomer
    {
        public long? Id { get; set; }
        public string? Name { get; set; }
    }

    public class CustomerPriceDTOProduct
    {
        public long? Id { get; set; }
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public decimal? Cost { get; set; }
    }

    public class CustomerPriceDTO
    {
        public long Id { get; set; }
        public decimal Price { get; set; }
        public int MinQty { get; set; }

        public long? CustomerId { get; set; }
        public CustomerPriceDTOCustomer Customer { get; set; } = new CustomerPriceDTOCustomer();

        public long? ProductId { get; set; }
        public CustomerPriceDTOProduct Product { get; set; } = new CustomerPriceDTOProduct();
    }
}
