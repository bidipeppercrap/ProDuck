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
        public async Task<PaginatedResponse> Get([FromQuery] PaginationParams qp, [FromQuery] string keyword = "")
        {
            var landedCosts = await _context.LandedCosts
                .Include(x => x.SourceLocation)
                .Include(x => x.TargetLocation)
                .Include(x => x.Items)
                .Where(x => x.Biller.Contains(keyword))
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
            // Use transaction pls
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var landedCost = await _context.LandedCosts
                .Include(x => x.Items).ThenInclude(xx => xx.PurchaseOrder)
                .Include(x => x.Items).ThenInclude(xx => xx.Children)
                .Where(x => x.Id.Equals(id))
                .FirstOrDefaultAsync() ?? throw new ApiException("Landed Cost not found.");

                // Update Stock to Source / Target Location
                foreach (var item in landedCost.Items)
                {
                    var stock = await _context.StockLocation
                        .Where(x => x.LocationId.Equals(landedCost.TargetLocationId))
                        .Where(x => x.ProductId.Equals(item.PurchaseOrder.ProductId))
                        .FirstOrDefaultAsync();

                    if (item.LandedCostId != null && item.LandedCostItemId == null)
                    {
                        if (stock != null) stock.Stock += item.Qty;
                        if (stock == null)
                        {
                            var newStock = new StockLocation
                            {
                                LocationId = landedCost.TargetLocationId,
                                ProductId = item.PurchaseOrder.ProductId,
                                Stock = item.Qty
                            };

                            _context.StockLocation.Add(newStock);
                            await _context.SaveChangesAsync();
                        }
                    }

                    foreach (var child in item.Children)
                    {
                        var childStock = await _context.StockLocation
                            .Where(x => x.LocationId.Equals(landedCost.TargetLocationId))
                            .Where(x => x.ProductId.Equals(child.PurchaseOrder.ProductId))
                            .FirstOrDefaultAsync();

                        if (childStock != null) childStock.Stock += child.Qty;
                        if (childStock == null)
                        {
                            var newStock = new StockLocation
                            {
                                LocationId = landedCost.TargetLocationId,
                                ProductId = child.PurchaseOrder.ProductId,
                                Stock = child.Qty
                            };

                            _context.StockLocation.Add(newStock);
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                if (!landedCost.IsPurchase) foreach (var item in landedCost.Items)
                    {
                        var stock = await _context.StockLocation
                        .Where(x => x.LocationId.Equals(landedCost.SourceLocationId))
                        .Where(x => x.ProductId.Equals(item.PurchaseOrder.ProductId))
                        .FirstOrDefaultAsync();

                        if (item.LandedCostId != null && item.LandedCostItemId == null)
                        {
                            if (stock != null) stock.Stock -= item.Qty;
                            if (stock == null)
                            {
                                var newStock = new StockLocation
                                {
                                    LocationId = landedCost.SourceLocationId,
                                    ProductId = item.PurchaseOrder.ProductId,
                                    Stock = item.Qty * -1
                                };

                                _context.StockLocation.Add(newStock);
                                await _context.SaveChangesAsync();
                            }
                        }

                        foreach (var child in item.Children)
                        {
                            var childStock = await _context.StockLocation
                                .Where(x => x.LocationId.Equals(landedCost.SourceLocationId))
                                .Where(x => x.ProductId.Equals(child.PurchaseOrder.ProductId))
                                .FirstOrDefaultAsync();

                            if (childStock != null) childStock.Stock -= child.Qty;
                            if (childStock == null)
                            {
                                var newStock = new StockLocation
                                {
                                    LocationId = landedCost.SourceLocationId,
                                    ProductId = item.PurchaseOrder.ProductId,
                                    Stock = item.Qty * -1
                                };

                                _context.StockLocation.Add(newStock);
                                await _context.SaveChangesAsync();
                            }
                        }
                    }

                // Update Cost + Previous Landed Cost of Purchase Order

                // Update Price if bigger than previous

                landedCost.IsDelivered = true;
                await _context.SaveChangesAsync();

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
