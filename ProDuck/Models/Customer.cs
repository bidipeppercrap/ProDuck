namespace ProDuck.Models
{
    public class Customer
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = "";
        public string Phone { get; set; } = "";
        public bool IsDeleted { get; set; } = false;

        public ICollection<CustomerPrice> ProductPrices { get; } = new List<CustomerPrice>();
        public ICollection<Order> Orders { get; } = new List<Order>();
    }
}
