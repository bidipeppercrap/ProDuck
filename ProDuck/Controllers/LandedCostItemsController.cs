using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProDuck.DTO;
using ProDuck.Models;
using ProDuck.QueryParams;
using ProDuck.Responses;
using ProDuck.Types;

namespace ProDuck.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LandedCostItemsController : ControllerBase
    {
        private readonly ProDuckContext _context;

        public LandedCostItemsController(ProDuckContext context)
        {
            _context = context;
        }

        private static LandedCostItemDTO ItemToDTO(LandedCostItem item)
        {
            var childrenDTO = new List<LandedCostItemDTO>();

            foreach(var child in item.Children)
            {
                childrenDTO.Add(new LandedCostItemDTO
                {
                    Id = child.Id,
                    Qty = child.Qty,
                    Cost = child.Cost,
                    PurchaseOrderId = child.PurchaseOrderId,
                    LandedCostId = child.LandedCostId,
                    LandedCostItemId = child.LandedCostItemId,
                    PurchaseOrder = new LandedCostItemDTOPurchaseOrder
                    {
                        Id = child.PurchaseOrder.Id,
                        Description = child.PurchaseOrder.Description,
                    }
                });
            }
            
            var dto = new LandedCostItemDTO
            {
                Id = item.Id,
                Qty = item.Qty,
                Cost = item.Cost,
                PurchaseOrderId = item.PurchaseOrderId,
                LandedCostId = item.LandedCostId,
                LandedCostItemId = item.LandedCostItemId,
                Children = childrenDTO,
                PurchaseOrder = new LandedCostItemDTOPurchaseOrder
                {
                    Id = item.PurchaseOrder.Id,
                    Description = item.PurchaseOrder.Description,
                }
            };

            return dto;
        }

        [HttpGet("landedcosts/{id}")]
        public async Task<PaginatedResponse> GetByLandedCost(long id, [FromQuery] PaginationParams qp, [FromQuery] string keyword = "")
        {
            var items = await _context.LandedCostItems
                .Include(x => x.PurchaseOrder).ThenInclude(xx => xx.Product)
                .Include(x => x.Children)
                .Where(x => x.LandedCostId == id)
                .Where(x => x.PurchaseOrder.Product.Name.Contains(keyword))
                .Select(x => ItemToDTO(x))
                .ToPagedListAsync(qp.Page, qp.PageSize);

            return new PaginatedResponse(items, new Pagination
            {
                Count = items.Count,
                Page = qp.Page,
                PageSize = qp.PageSize,
                TotalPages = items.TotalPages
            });
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LandedCostItemCreateDTO dto)
        {
            if (dto.LandedCostItemId == null && dto.LandedCostId == null) throw new ApiException("Both Landed Cost Item and Landed Cost cannot be null");

            var landedCostItem = new LandedCostItem
            {
                Qty = dto.Qty,
                Cost = dto.Cost,
                PurchaseOrderId = dto.PurchaseOrderId,
                LandedCostId = dto.LandedCostId,
                LandedCostItemId = dto.LandedCostItemId,
            };

            Console.WriteLine(landedCostItem);

            _context.LandedCostItems.Add(landedCostItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT api/<LandedCostItemsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<LandedCostItemsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
