using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProDuck.DTO;
using ProDuck.Models;
using ProDuck.QueryParams;
using ProDuck.Responses;
using ProDuck.Types;
using System.Linq;

namespace ProDuck.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "root")]
    public class LandedCostsController : ControllerBase
    {
        private readonly ProDuckContext _context;

        public LandedCostsController(ProDuckContext context)
        {
            _context = context;
        }

        private static LandedCostDTO LandedCostToDTO(LandedCost landedCost)
        {
            var dto = new LandedCostDTO
            {
                Id = landedCost.Id,
                Date = landedCost.Date,
                Biller = landedCost.Biller,
                IsPurchase = landedCost.IsPurchase,
                IsDelivered = landedCost.IsDelivered,
                TotalCost = landedCost.Items.Sum(x => x.Cost),
                SourceLocationId = landedCost.SourceLocationId,
                TargetLocationId = landedCost.TargetLocationId,
                SourceLocation = landedCost.SourceLocation != null ? new LandedCostDTOLocation
                {
                    Id = landedCost.SourceLocation.Id,
                    Name = landedCost.SourceLocation.Name,
                } : null,
                TargetLocation = landedCost.TargetLocation != null ? new LandedCostDTOLocation
                {
                    Id = landedCost.TargetLocation.Id,
                    Name = landedCost.TargetLocation.Name
                } : null
            };

            return dto;
        }

        [HttpGet]
        public async Task<PaginatedResponse> Get([FromQuery] PaginationParams qp, [FromQuery] bool showNotDelivered = false, [FromQuery] string keyword = "")
        {
            var whereQuery = _context.LandedCosts
                .Include(x => x.SourceLocation)
                .Include(x => x.TargetLocation)
                .Include(x => x.Items)
                .Where(x => x.Biller.Contains(keyword))
                .AsQueryable();

            if (showNotDelivered) whereQuery = whereQuery.Where(x => x.IsDelivered.Equals(false));

            var landedCosts = await whereQuery
                .OrderBy(x => x.IsDelivered)
                .ThenByDescending(x => x.Date)
                .Select(x => LandedCostToDTO(x))
                .ToPagedListAsync(qp.Page, qp.PageSize);

            return new PaginatedResponse(landedCosts, new Pagination
            {
                Count = landedCosts.Count,
                Page = qp.Page,
                PageSize = qp.PageSize,
                TotalPages = landedCosts.TotalPages
            });
        }

        [HttpGet("{id}")]
        public async Task<PaginatedResponse> Get(long id)
        {
            var landedCost = await _context.LandedCosts
                .Include(x => x.SourceLocation)
                .Include(x => x.TargetLocation)
                .Include(x => x.Items)
                .Where(x => x.Id.Equals(id))
                .Select(x => LandedCostToDTO(x))
                .FirstOrDefaultAsync() ?? throw new ApiException("Landed Cost not found.");

            return new PaginatedResponse(landedCost);
        }

        private record PurchaseOrderSum(long ProductId, decimal CostSum);

        [HttpPost("deliver")]
        public async Task<IActionResult> Deliver([FromBody] long id)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var landedCost = await _context.LandedCosts
                .Include(x => x.Items).ThenInclude(xx => xx.PurchaseOrder)
                .Include(x => x.Items).ThenInclude(xx => xx.Children).ThenInclude(xxx => xxx.PurchaseOrder)
                .Where(x => x.Id.Equals(id))
                .FirstOrDefaultAsync() ?? throw new ApiException("Landed Cost not found.");

                landedCost.DeliveredAt = DateOnly.FromDateTime(DateTime.Now);
                landedCost.IsDelivered = true;

                await _context.SaveChangesAsync();

                var items = await _context.LandedCostItems
                    .Include(x => x.Parent)
                    .Include(x => x.PurchaseOrder)
                    .Where(x => (x.PurchaseOrderId != null && x.LandedCostId.Equals(id)) || (x.PurchaseOrderId != null && x.LandedCostItemId != null && x.Parent!.LandedCostId.Equals(id)))
                    .ToListAsync();

                foreach(var x in items)
                {
                    var targetStock = await _context.StockLocation
                        .Where(s => s.LocationId.Equals(landedCost.TargetLocationId))
                        .Where(s => s.ProductId.Equals(x.PurchaseOrder!.ProductId))
                        .FirstOrDefaultAsync();

                    if (targetStock != null)
                    {
                        targetStock.Stock += x.Qty;
                        _context.StockLocation.Update(targetStock);
                    }
                    if (targetStock == null)
                    {
                        var newStock = new StockLocation { LocationId = landedCost.TargetLocationId, ProductId = x.PurchaseOrder!.ProductId, Stock = x.Qty };
                        _context.StockLocation.Add(newStock);
                    }

                    if (!landedCost.IsPurchase)
                    {
                        var sourceStock = await _context.StockLocation
                            .Where(s => s.LocationId.Equals(landedCost.SourceLocationId))
                            .Where(s => s.ProductId.Equals(x.PurchaseOrder!.ProductId))
                            .FirstOrDefaultAsync();

                        if (sourceStock != null)
                        {
                            sourceStock.Stock -= x.Qty;
                            _context.StockLocation.Update(sourceStock);
                        }
                        if (sourceStock == null)
                        {
                            var newStock = new StockLocation { LocationId = landedCost.SourceLocationId, ProductId = x.PurchaseOrder!.ProductId, Stock = x.Qty * -1 };
                            _context.StockLocation.Add(newStock);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                var purchaseOrders = await _context.PurchaseOrders
                    .Include(x => x.LandedCostItems).ThenInclude(xx => xx.Parent).ThenInclude(xxx => xxx!.Children).ThenInclude(xxxx => xxxx.PurchaseOrder)
                    .Where(x => x.LandedCostItems.Any(xx => xx.LandedCostId.Equals(id) || xx.Parent!.LandedCostId.Equals(id)))
                    .Select(x => new PurchaseOrderSum(
                        x.ProductId,
                        x.Cost
                            +
                            x.LandedCostItems.Where(xx => xx.LandedCostId != null && xx.LandedCostItemId == null).Sum(xx => (xx.Cost / xx.Qty) * (xx.Qty / x.Quantity))
                            +
                            x.LandedCostItems.Where(xx => xx.LandedCostId == null && xx.LandedCostItemId != null).Sum(xx =>
                                (((xx.Qty * x.Cost) / xx.Parent!.Children.Sum(c => c.Qty * c.PurchaseOrder!.Cost)
                                *
                                xx.Parent!.Cost)
                                /
                                xx.Qty)
                                *
                                (xx.Qty / x.Quantity)
                            )
                    ))
                    .ToListAsync();

                foreach(var po in purchaseOrders)
                {
                    await _context.Products
                        .Where(x => x.Id.Equals(po.ProductId))
                        .Where(x => x.Cost < po.CostSum)
                        .ExecuteUpdateAsync(x => x
                            .SetProperty(s => s.Cost, po.CostSum)
                         );
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) throw new ApiException(ex.InnerException.Message);
                throw new ApiException(ex.Message);
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<long>> Post([FromBody] LandedCostCreateDTO dto)
        {
            var landedCost = new LandedCost
            {
                Date = dto.Date,
                Biller = dto.Biller,
                SourceLocationId = dto.SourceLocationId,
                TargetLocationId = dto.TargetLocationId,
                IsPurchase = dto.IsPurchase,
                IsDelivered = false
            };

            _context.LandedCosts.Add(landedCost);
            await _context.SaveChangesAsync();

            return Ok(landedCost.Id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] LandedCostCreateDTO dto)
        {
            await _context.LandedCosts
                .Where(x => x.Id.Equals(id))
                .ExecuteUpdateAsync(x => x
                    .SetProperty(s => s.Date, dto.Date)
                    .SetProperty(s => s.Biller, dto.Biller)
                    .SetProperty(s => s.IsPurchase, dto.IsPurchase)
                    .SetProperty(s => s.SourceLocationId, dto.SourceLocationId)
                    .SetProperty(s => s.TargetLocationId, dto.TargetLocationId)
                );

            return NoContent();
        }

        // DELETE api/<LandedCostsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
