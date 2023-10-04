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

                // Update Stock to Source / Target Location
                foreach (var item in landedCost.Items)
                {
                    if (item.PurchaseOrderId != null && item.LandedCostItemId == null)
                    {
                        var stock = await _context.StockLocation
                            .Where(x => x.LocationId.Equals(landedCost.TargetLocationId))
                            .Where(x => x.ProductId.Equals(item.PurchaseOrder!.ProductId))
                            .FirstOrDefaultAsync();

                        if (stock != null)
                        {
                            stock.Stock += item.Qty;
                            _context.StockLocation.Update(stock);
                            await _context.SaveChangesAsync();

                        }
                        if (stock == null)
                        {
                            var newStock = new StockLocation
                            {
                                LocationId = landedCost.TargetLocationId,
                                ProductId = item.PurchaseOrder!.ProductId,
                                Stock = item.Qty
                            };

                            _context.StockLocation.Add(newStock);
                            await _context.SaveChangesAsync();
                        }
                    }

                    if (item.PurchaseOrderId == null) foreach (var child in item.Children)
                    {
                        var childStock = await _context.StockLocation
                            .Where(x => x.LocationId.Equals(landedCost.TargetLocationId))
                            .Where(x => x.ProductId.Equals(child.PurchaseOrder!.ProductId))
                            .FirstOrDefaultAsync();

                            if (childStock != null)
                            {
                                childStock.Stock += item.Qty;
                                _context.StockLocation.Update(childStock);
                                await _context.SaveChangesAsync();
                            }
                            if (childStock == null)
                        {
                            var newStock = new StockLocation
                            {
                                LocationId = landedCost.TargetLocationId,
                                ProductId = child.PurchaseOrder!.ProductId,
                                Stock = child.Qty
                            };

                            _context.StockLocation.Add(newStock);
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                if (!landedCost.IsPurchase) foreach (var item in landedCost.Items)
                {
                    if (item.PurchaseOrderId != null && item.LandedCostItemId == null)
                    {
                        var stock = await _context.StockLocation
                        .Where(x => x.LocationId.Equals(landedCost.SourceLocationId))
                        .Where(x => x.ProductId.Equals(item.PurchaseOrder!.ProductId))
                        .FirstOrDefaultAsync();

                        if (stock != null)
                        {
                            stock.Stock -= item.Qty;
                            _context.StockLocation.Update(stock);
                            await _context.SaveChangesAsync();

                        }
                        if (stock == null)
                        {
                            var newStock = new StockLocation
                            {
                                LocationId = landedCost.SourceLocationId,
                                ProductId = item.PurchaseOrder!.ProductId,
                                Stock = item.Qty * -1
                            };

                            _context.StockLocation.Add(newStock);
                            await _context.SaveChangesAsync();
                            }
                    }

                    if (item.PurchaseOrderId == null) foreach (var child in item.Children)
                    {
                        var childStock = await _context.StockLocation
                            .Where(x => x.LocationId.Equals(landedCost.SourceLocationId))
                            .Where(x => x.ProductId.Equals(child.PurchaseOrder!.ProductId))
                            .FirstOrDefaultAsync();

                        if (childStock != null)
                        {
                            childStock.Stock -= item.Qty;
                            _context.StockLocation.Update(childStock);
                            await _context.SaveChangesAsync();

                        }
                        if (childStock == null)
                        {
                            var newStock = new StockLocation
                            {
                                LocationId = landedCost.SourceLocationId,
                                ProductId = child.PurchaseOrder!.ProductId,
                                Stock = child.Qty * -1
                            };

                            _context.StockLocation.Add(newStock);
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                var products = await _context.Products
                    .Include(x => x.PurchaseOrders).ThenInclude(po => po.LandedCostItems).ThenInclude(lci => lci.LandedCost)
                    .Include(x => x.PurchaseOrders).ThenInclude(po => po.LandedCostItems).ThenInclude(lci => lci.Parent).ThenInclude(lcip => lcip!.Children)
                    .Where(x => x.PurchaseOrders
                        .Any(po => po.LandedCostItems
                            .Any(lci => (lci.LandedCost != null && lci.LandedCost.IsDelivered && lci.PurchaseOrderId != null) || (lci.Parent != null && lci.PurchaseOrderId != null && lci.Parent!.LandedCost!.IsDelivered))
                            &&
                            x.Cost
                            <
                            po!.Cost
                            + po.LandedCostItems.Where(lci => lci.LandedCost != null && lci.LandedCost.IsDelivered && lci.PurchaseOrderId != null).Sum(lci => lci.Cost / lci.Qty * (lci.Qty / po.Quantity))
                            + po.LandedCostItems.Where(lci => lci.Parent != null && lci.Parent!.LandedCost!.IsDelivered && lci.PurchaseOrderId != null).Sum(lci => (((lci.Parent!.Children.Sum(child => po.Cost * child.Qty) / (po.Cost * lci.Qty)) * lci.Parent.Cost) / lci.Qty) * (lci.Qty / po.Quantity))

                        )
                    )
                    .ToListAsync();

                foreach (var p in products)
                {
                    var sumQuery = _context.PurchaseOrders
                        .Include(x => x.LandedCostItems).ThenInclude(lci => lci.LandedCost)
                        .Include(x => x.LandedCostItems).ThenInclude(lci => lci.Parent).ThenInclude(lcip => lcip!.Children)
                        .Where(x => x.ProductId.Equals(p.Id))
                        .Where(x => x.LandedCostItems
                            .Any(lci => (lci.LandedCost != null && lci.LandedCost.IsDelivered && lci.PurchaseOrderId != null) || (lci.Parent != null && lci.Parent!.LandedCost!.IsDelivered && lci.PurchaseOrderId != null))).AsQueryable();

                    var sum = await sumQuery
                        .Select(x => x.Cost
                            + x.LandedCostItems.Where(lci2 => lci2.LandedCost != null && lci2.LandedCost.IsDelivered && lci2.PurchaseOrderId != null).Sum(lci2 => lci2.Cost / lci2.Qty * (lci2.Qty / x.Quantity)
                            + x.LandedCostItems.Where(lci2 => lci2.Parent != null && lci2.Parent!.LandedCost!.IsDelivered && lci2.PurchaseOrderId != null).Sum(lci2 => (((lci2.Parent!.Children.Sum(child => x.Cost * child.Qty) / (x.Cost * lci2.Qty)) * lci2.Parent.Cost) / lci2.Qty) * (lci2.Qty / x.Quantity))))
                        .FirstOrDefaultAsync();

                    await _context.Products
                        .Where(x => x.Id.Equals(p.Id))
                        .ExecuteUpdateAsync(x => x
                            .SetProperty(s => s.Cost, sum));
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
