namespace ProDuck.DTO
{
    public class POSSessionDTOOrder
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalCost { get; set; }
    }
    public class POSSessionDTOPOS
    {
        public long? Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
    public class POSSessionDTO
    {
        public long? Id { get; set; }
        public string? ClosingRemark { get; set; }
        public DateTime OpenedAt { get; set; } = DateTime.Now;
        public DateTime? ClosedAt { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public int OrderCount { get; set; } = 0;
        public decimal TotalSalesPrice { get; set; } = decimal.Zero;
        public decimal TotalSalesCost { get; set; } = decimal.Zero;

        public long UserId { get; set; }
        public UserDTO? SessionOpener { get; set; }

        public long POSId { get; set; }
        public POSSessionDTOPOS? POS { get; set; }

        public List<POSSessionDTOOrder>? Orders { get; set; }
    }
}
