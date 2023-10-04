using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "root")]
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
                    PurchaseOrder = child.PurchaseOrder != null ? new LandedCostItemDTOPurchaseOrder
                    {
                        Id = child.PurchaseOrder.Id,
                        Description = child.PurchaseOrder.Description,
                    } : null
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
                PurchaseOrder = item.PurchaseOrder != null ? new LandedCostItemDTOPurchaseOrder
                {
                    Id = item.PurchaseOrder.Id,
                    Description = item.PurchaseOrder.Description,
                } : null
            };

            return dto;
        }

        [HttpGet("landedcosts/{id}")]
        public async Task<PaginatedResponse> GetByLandedCost(long id, [FromQuery] PaginationParams qp, [FromQuery] string keyword = "")
        {
            var items = await _context.LandedCostItems
                .Include(x => x.PurchaseOrder).ThenInclude(xx => xx!.Product)
                .Include(x => x.Children).ThenInclude(xx => xx.PurchaseOrder).ThenInclude(xxx => xxx!.Product)
                .Where(x => x.LandedCostId == id)
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
            if (dto.PurchaseOrderId != null && dto.LandedCostItemId != null && dto.LandedCostId != null) throw new ApiException("Parent Landed Cost cannot have a Purchase Order");

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
