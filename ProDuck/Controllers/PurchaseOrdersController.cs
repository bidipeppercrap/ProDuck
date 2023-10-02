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
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly ProDuckContext _context;

        public PurchaseOrdersController(ProDuckContext context)
        {
            _context = context;
        }

        private static PurchaseOrderDTO OrderToDTO(PurchaseOrder order)
        {
            var dto = new PurchaseOrderDTO
            {
                Id = order.Id,
                Description = order.Description ?? "",
                Cost = order.Cost,
                Quantity = order.Quantity,
                Delivered = order.LandedCostItems
                    .Where(x => (x.LandedCost != null && x.LandedCost.IsDelivered && x.LandedCost.IsPurchase)
                        || (x.LandedCost == null && x.Parent != null && x.Parent.LandedCost != null && x.Parent.LandedCost.IsDelivered && x.Parent.LandedCost.IsPurchase))
                    .Sum(x => x.Qty),
                PurchaseId = order.PurchaseId,
                ProductId = order.ProductId,
                Product = new ProductDTO
                {
                    Id = order.Product.Id,
                    Name = order.Product.Name,
                    Price = order.Product.Price,
                    Cost = order.Product.Cost
                }
            };

            return dto;
        }

        [HttpGet("purchases/{id}")]
        public async Task<PaginatedResponse> GetByPurchase(long id, [FromQuery] PaginationParams qp)
        {
            var orders = await _context.PurchaseOrders
                .Include(x => x.Product)
                .Include(x => x.LandedCostItems).ThenInclude(xx => xx.LandedCost)
                .Include(x => x.LandedCostItems).ThenInclude(xx => xx.Parent).ThenInclude(xxx => xxx!.LandedCost)
                .Where(x => x.PurchaseId.Equals(id))
                .Select(x => OrderToDTO(x))
                .ToPagedListAsync(qp.Page, qp.PageSize);

            return new PaginatedResponse(orders, new Pagination
            {
                Count = orders.Count,
                Page = qp.Page,
                PageSize = qp.PageSize,
                TotalPages = orders.TotalPages
            });
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PurchaseOrderCreateDTO dto)
        {
            var order = new PurchaseOrder
            {
                Description = dto.Description,
                Cost = dto.Cost,
                Quantity = dto.Quantity,
                PurchaseId = dto.PurchaseId,
                ProductId = dto.ProductId,
            };

            _context.PurchaseOrders.Add(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] PurchaseOrderCreateDTO dto)
        {
            await _context.PurchaseOrders
                .Where(x => x.Id.Equals(id))
                .ExecuteUpdateAsync(x => x
                    .SetProperty(s => s.Cost, dto.Cost)
                    .SetProperty(s => s.Quantity, dto.Quantity)
                    .SetProperty(s => s.Description, dto.Description)
                    .SetProperty(s => s.ProductId, dto.ProductId));

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _context.PurchaseOrders
                .Where(x => x.Id.Equals(id))
                .ExecuteDeleteAsync();

            return NoContent();
        }
    }
}
