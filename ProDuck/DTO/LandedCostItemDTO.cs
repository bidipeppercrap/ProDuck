using ProDuck.Models;

namespace ProDuck.DTO
{
    public class LandedCostItemDTOPurchaseOrder
    {
        public long Id { get; set; }
        public string Description { get; set; } = string.Empty;
    }
    public class LandedCostItemBaseDTO
    {
        public int Qty { get; set; }
        public decimal Cost { get; set; }
          
        public long PurchaseOrderId { get; set; }
        public long? LandedCostId { get; set; }
        public long? LandedCostItemId { get; set; }
    }
    public class LandedCostItemDTO : LandedCostItemBaseDTO
    {
        public long Id { get; set; }
        public LandedCostItemDTOPurchaseOrder PurchaseOrder { get; set; } = new();
        public ICollection<LandedCostItemDTO> Children { get; set;  } = new List<LandedCostItemDTO>();
    }

    public class LandedCostItemCreateDTO : LandedCostItemBaseDTO { }
}
