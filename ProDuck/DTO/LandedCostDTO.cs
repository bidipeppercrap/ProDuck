namespace ProDuck.DTO
{
    public class LandedCostDTOLocation
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class LandedCostBaseDTO
    {
        public DateOnly Date { get; set; }
        public DateOnly? DeliveredAt { get; set; }
        public string Biller { get; set; } = string.Empty;
        public long? SourceLocationId { get; set; }
        public long? TargetLocationId { get; set; }
        public bool IsPurchase { get; set; } = true;
    }
    public class LandedCostDTO : LandedCostBaseDTO
    {
        public long Id { get; set; }
        public bool IsDelivered { get; set; } = false;
        public decimal TotalCost { get; set; }

        public LandedCostDTOLocation? SourceLocation = new();
        public LandedCostDTOLocation? TargetLocation = new();
    }
    public class LandedCostCreateDTO : LandedCostBaseDTO
    {
    }
}
