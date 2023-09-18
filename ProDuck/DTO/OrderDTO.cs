namespace ProDuck.DTO
{
    public class OrderDTOItemProduct
    {
        public long Id { get; set; }
        public string? Name { get; set; }
    }
    public class OrderDTOItem
    {
        public long Id { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public int Qty { get; set; }

        public OrderDTOItemProduct Product { get; set; } = null!;

    }
    public class OrderDTOCustomer
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
    }
    public class OrderBaseDTO
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public long UserId { get; set; }
        public UserDTO? ServedBy { get; set; }

        public long CustomerId { get; set; }
        public OrderDTOCustomer? Customer { get; set; }

        public List<OrderDTOItem> Items { get; set; } = new List<OrderDTOItem>();
    }
    public class OrderDTO : OrderBaseDTO
    {
        public long POSSessionId { get; set; }
    }
    public class OrderCreateDTO : OrderBaseDTO
    {
        public long POSId { get; set; }
    }
}
