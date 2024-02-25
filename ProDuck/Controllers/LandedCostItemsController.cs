using AutoWrapper.Wrappers;
using Humanizer;
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

        public class CreateAllFromPurchaseDTO
        {
            public long PurchaseId { get; set; }
            public long LandedCostId { get; set; }
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateAllBulk([FromBody] CreateAllFromPurchaseDTO dto)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var bulkItem = new LandedCostItem
                {
                    Qty = 0,
                    Cost = 0,
                    LandedCostId = dto.LandedCostId
                };

                _context.LandedCostItems.Add(bulkItem);
                await _context.SaveChangesAsync();

                var orders = await _context.PurchaseOrders
                    .Include(x => x.Product)
                    .Include(x => x.LandedCostItems).ThenInclude(xx => xx.LandedCost)
                    .Include(x => x.LandedCostItems).ThenInclude(xx => xx.Parent).ThenInclude(xxx => xxx!.LandedCost)
                    .Where(x => x.PurchaseId.Equals(dto.PurchaseId))
                    .Select(x => PurchaseOrdersController.OrderToDTO(x))
                    .ToListAsync();

                var itemsToAdd = new List<LandedCostItem> { };

                foreach (var order in orders)
                {
                    var item = new LandedCostItem
                    {
                        Qty = order.Quantity - order.Delivered,
                        Cost = 0,
                        LandedCostItemId = bulkItem.Id,
                        PurchaseOrderId = order.Id
                    };

                    itemsToAdd.Add(item);
                }

                _context.LandedCostItems.AddRange(itemsToAdd);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                throw new ApiException("Internal Error");
            }

            return NoContent();
        }

        [HttpPost("separated")]
        public async Task<IActionResult> CreateAllSeparated([FromBody] CreateAllFromPurchaseDTO dto)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var orders = await _context.PurchaseOrders
                    .Include(x => x.Product)
                    .Include(x => x.LandedCostItems).ThenInclude(xx => xx.LandedCost)
                    .Include(x => x.LandedCostItems).ThenInclude(xx => xx.Parent).ThenInclude(xxx => xxx!.LandedCost)
                    .Where(x => x.PurchaseId.Equals(dto.PurchaseId))
                    .Select(x => PurchaseOrdersController.OrderToDTO(x))
                    .ToListAsync();

                var itemsToAdd = new List<LandedCostItem> { };

                foreach (var order in orders)
                {
                    var item = new LandedCostItem
                    {
                        Qty = order.Quantity - order.Delivered,
                        Cost = 0,
                        LandedCostId = dto.LandedCostId,
                        PurchaseOrderId = order.Id
                    };

                    itemsToAdd.Add(item);
                }

                _context.LandedCostItems.AddRange(itemsToAdd);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                throw new ApiException("Internal Error");
            }

            return NoContent();
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

            _context.LandedCostItems.Add(landedCostItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] LandedCostItemCreateDTO dto)
        {
            var landedCostItem = await _context.LandedCostItems.FindAsync(id) ?? throw new ApiException("Landed Cost Item not found");

            if (dto.LandedCostItemId == null && dto.LandedCostId == null) throw new ApiException("Both Landed Cost Item and Landed Cost cannot be null");
            if (dto.PurchaseOrderId != null && dto.LandedCostItemId != null && dto.LandedCostId != null) throw new ApiException("Parent Landed Cost cannot have a Purchase Order");

            landedCostItem.Qty = dto.Qty;
            landedCostItem.Cost = dto.Cost;
            landedCostItem.PurchaseOrderId = dto.PurchaseOrderId;
            landedCostItem.LandedCostId = dto.LandedCostId;
            landedCostItem.LandedCostItemId = dto.LandedCostItemId;

            _context.LandedCostItems.Update(landedCostItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var landedCostItem = _context.LandedCostItems
                .Where(x => x.Id.Equals(id))
                .Include(x => x.Children)
                .AsQueryable();

            if (await landedCostItem.FirstOrDefaultAsync() == null) throw new ApiException("Landed Cost Item not found");

            var toDelete = await landedCostItem.FirstAsync();

            _context.LandedCostItems.Remove(toDelete);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
