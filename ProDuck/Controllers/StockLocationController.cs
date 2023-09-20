using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProDuck.DTO;
using ProDuck.Models;
using ProDuck.QueryParams;
using ProDuck.Responses;
using ProDuck.Types;
using System.Text.Json.Serialization;
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
        public async Task<PaginatedResponse> GetProductsInLocation(long id, [FromQuery] PaginationParams qp)
        {
            var result = await _context.Locations
                .Include(l => l.Products)
                    .ThenInclude(s => s.Product)
                .Where(l => l.Id == id)
                .Select(x => LocationStockListToDTO(x))
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
        public async Task<PaginatedResponse> GetLocationsOfProduct(long id, [FromQuery] PaginationParams qp)
        {
            var result = await _context.Products
                .Include(p => p.Stocks)
                    .ThenInclude(s => s.Location)
                .Where(p => p.Id == id)
                .Select(x => ProductStockListToDTO(x))
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

            var stock = new StockLocation()
            {
                Stock = stockDTO.Stock,
                ProductId = stockDTO.ProductId,
                LocationId = stockDTO.LocationId
            };

            _context.StockLocation.Add(stock);
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

            var location = await _context.Locations.FindAsync(stockCreateDTO.LocationId);
            if (location == null) result.ErrorMessages.Add("Product not found.");

            if (result.ErrorMessages.Count > 0) return result;

            result.IsValid = true;
            return result;
        }

        private static ProductStockListDTO ProductStockListToDTO(Product product)
        {
            var locations = new List<ProductStockDTO>();

            foreach (var s in product.Stocks)
            {
                locations.Add(new ProductStockDTO()
                {
                    Id = s.Id,
                    Stock = s.Stock,
                    Location = s.Location == null ? null : new ProductStockLocationDTO()
                    {
                        Id = s.Location.Id,
                        Name = s.Location.Name
                    }
                });
            }

            return new ProductStockListDTO()
            {
                Id = product.Id,
                Name = product.Name,
                Stocks = locations
            };
        }

        private static LocationStockListDTO LocationStockListToDTO(Location location)
        {
            var products = new List<LocationStockDTO>();

            foreach (var p in location.Products)
            {
                products.Add(new LocationStockDTO()
                {
                    Id = p.Id,
                    Stock = p.Stock,
                    Product = new LocationStockProductDTO()
                    {
                        Id = p.Product.Id,
                        Name = p.Product.Name
                    }
                });
            }

            return new LocationStockListDTO()
            {
                Id = location.Id,
                Name = location.Name,
                Stocks = products
            };
        }
    }
}
