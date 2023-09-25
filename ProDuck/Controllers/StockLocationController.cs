using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProDuck.DTO;
using ProDuck.Models;
using ProDuck.QueryParams;
using ProDuck.Responses;
using ProDuck.Types;
using System.Text.Json.Serialization;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using static ProDuck.Controllers.ProductsController;

namespace ProDuck.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StockLocationController : ControllerBase
    {
        private readonly ProDuckContext _context;

        public StockLocationController(ProDuckContext context)
        {
            _context = context;
        }

        [HttpGet("location/{id}")]
        public async Task<PaginatedResponse> GetLocationStocks(long id, [FromQuery] PaginationParams qp, [FromQuery] bool isRootLocation = false, [FromQuery] string keyword = "")
        {
            var whereQuery = _context.StockLocation
                .Include(x => x.Product)
                .Where(x => x.Product.Deleted == false)
                .Where(x => x.LocationId == (isRootLocation ? null : id));

            var words = keyword.Trim().Split(" ");
            foreach (var word in words)
            {
                whereQuery = whereQuery.Where(x => x.Product.Name.Contains(word));
            }

            var result = await whereQuery
                .Select(x => ProductStockToDTO(x))
                .ToPagedListAsync(qp.Page, qp.PageSize);

            return new PaginatedResponse(result, new Pagination
            {
                Count = result.Count,
                Page = qp.Page,
                PageSize = qp.PageSize,
                TotalPages = result.TotalPages
            });
        }

        [HttpGet("product/{id}")]
        public async Task<PaginatedResponse> GetProductStocks(long id, [FromQuery] PaginationParams qp, [FromQuery] string keyword = "")
        {
            var whereQuery = _context.StockLocation
                .Include(x => x.Location)
                .Where(x => x.ProductId == id);

            var words = keyword.Trim().Split(" ");
            if (keyword.Length > 0)
            {
                foreach (var word in words)
                {
                    whereQuery = whereQuery.Where(x => x.Location != null && x.Location.Name.Contains(word));
                }
            }

            var result = await whereQuery
                .OrderBy(x => x.Location)
                .ThenBy(x => x.Stock)
                .Select(x => LocationStockToDTO(x))
                .ToPagedListAsync(qp.Page, qp.PageSize);

            return new PaginatedResponse(result, new Pagination
            {
                Count = result.Count,
                Page = qp.Page,
                PageSize = qp.PageSize,
                TotalPages = result.TotalPages
            });
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] StockCreateDTO stockDTO)
        {
            var validation = await ValidateStockCreateAsync(stockDTO);
            if (!validation.IsValid) throw new ApiException(validation.ErrorMessages.First());

            var stock = await _context.StockLocation
                .Where(x => x.ProductId.Equals(stockDTO.ProductId))
                .Where(x => x.LocationId.Equals(stockDTO.LocationId))
                .FirstOrDefaultAsync();

            if (stock != null)
            {
                stock.Stock += stockDTO.Stock;

                _context.StockLocation.Update(stock);
            }
            else
            {
                stock = new StockLocation()
                {
                    Stock = stockDTO.Stock,
                    ProductId = stockDTO.ProductId,
                    LocationId = stockDTO.LocationId
                };

                _context.StockLocation.Add(stock);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(long id, [FromBody] StockUpdateDTO updateDTO)
        {
            var stock = await _context.StockLocation.FindAsync(id);
            if (stock == null) throw new ApiException("Stock not found.");

            await _context.StockLocation
                .Where(s => s.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Stock, updateDTO.Stock));

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(long id)
        {
            var stock = await _context.StockLocation.FindAsync(id);

            if (stock == null) throw new ApiException("Stock not found.");

            await _context.StockLocation
                .Where(s => s.Id == id)
                .ExecuteDeleteAsync();

            return NoContent();
        }

        private async Task<ValidationResult> ValidateStockCreateAsync(StockCreateDTO stockCreateDTO)
        {
            var result = new ValidationResult();

            var product = await _context.Products.FindAsync(stockCreateDTO.ProductId);
            if (product == null) result.ErrorMessages.Add("Product not found.");

            if (stockCreateDTO.LocationId != null)
            {
                var location = await _context.Locations.FindAsync(stockCreateDTO.LocationId);
                if (location == null) result.ErrorMessages.Add("Location not found.");
            }

            if (result.ErrorMessages.Count > 0) return result;

            result.IsValid = true;
            return result;
        }

        private static LocationStockDTO LocationStockToDTO(StockLocation stock)
        {
            var location = stock.Location != null ? new LocationStockDTOLocation
            {
                Id = stock.Location.Id,
                Name = stock.Location.Name,
            } : null;

            return new LocationStockDTO
            {
                Id = stock.Id,
                Stock = stock.Stock,
                Location = location
            };
        }

        private static ProductStockDTO ProductStockToDTO(StockLocation stock)
        {
            var product = new ProductStockDTOProduct
            {
                Id = stock.Product.Id,
                Name = stock.Product.Name
            };

            return new ProductStockDTO
            {
                Id = stock.Id,
                Stock = stock.Stock,
                Product = product
            };
        }
    }
}
