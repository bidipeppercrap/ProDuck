using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProDuck.DTO;
using ProDuck.Models;
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
        public async Task<ActionResult<IEnumerable<string>>> GetProductsInLocation(long id)
        {
            var stocks = await _context.Locations
                .Include(l => l.Products)
                .ThenInclude(s => s.Product)
                .Where(l => l.Id == id)
                .Select(x => LocationStockListToDTO(x))
                .ToListAsync();

            return Ok(stocks);
        }

        [HttpGet("product/{id}")]
        public async Task<ActionResult<IEnumerable<string>>> GetLocationsOfProduct(long id)
        {
            var stocks = await _context.Products
                .Include(p => p.Stocks)
                .ThenInclude(s => s.Location)
                .Where(p => p.Id == id)
                .Select(x => ProductStockListToDTO(x))
                .ToListAsync();

            return Ok(stocks);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] StockCreateDTO stockDTO)
        {
            if (!await ValidateStockCreate(stockDTO)) return BadRequest();

            var stock = new StockLocation()
            {
                Stock = stockDTO.Stock,
                ProductId = stockDTO.ProductId,
                LocationId = stockDTO.LocationId
            };

            _context.StockLocation.Add(stock);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(long id, [FromBody] StockUpdateDTO updateDTO)
        {
            var stock = await _context.StockLocation.FindAsync(id);

            if (stock == null)
            {
                return NotFound();
            }

            await _context.StockLocation
                .Where(s => s.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Stock, updateDTO.Stock));

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(long id)
        {
            var stock = await _context.StockLocation.FindAsync(id);

            if (stock == null)
            {
                return NotFound();
            }

            await _context.StockLocation
                .Where(s => s.Id == id)
                .ExecuteDeleteAsync();

            return Ok();
        }

        private async Task<bool> ValidateStockCreate(StockCreateDTO stockCreateDTO)
        {
            var product = await _context.Products.FindAsync(stockCreateDTO.ProductId);

            if (product == null) return false;

            var location = await _context.Locations.FindAsync(stockCreateDTO.LocationId);

            if (location == null) return false;

            return true;
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
                    Location = new ProductStockLocationDTO()
                    {
                        Id = s.Location!.Id,
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
