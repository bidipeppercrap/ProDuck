﻿namespace ProDuck.DTO
{
    public class PurchaseOrderDTO
    {
        public long? Id { get; set; }
        public long? PurchaseId { get; set; }
        public long ProductId { get; set; }
        public int Cost { get; set; }
        public int Quantity { get; set; }
    }
}
