using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProDuck.DTO;
using ProDuck.Models;
using ProDuck.QueryParams;

namespace ProDuck.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CustomerPricesController : ControllerBase
    {
        private readonly ProDuckContext _context;

        public CustomerPricesController(ProDuckContext context)
        {
            _context = context;
        }

        private static CustomerPriceDTO PriceToDTO(CustomerPrice price)
        {
            var dto = new CustomerPriceDTO
            {
                Id = price.Id,
                Price = price.Price,
                MinQty = price.MinQty
            };

            if (price.Product != null)
            {
                dto.Product = new CustomerPriceDTOProduct
                {
                    Id = price.Product.Id,
                    Name = price.Product.Name,
                    Price = price.Product.Price,
                    Cost = price.Product.Cost,
                };
            }

            if (price.Customer != null)
            {
                dto.Customer = new CustomerPriceDTOCustomer
                {
                    Id = price.Customer.Id,
                    Name = price.Customer.Name,
                };
            }

            return dto;
        }

        [HttpGet("products/{id}")]
        public async Task<ActionResult<IEnumerable<CustomerPriceDTO>>> GetProductPrices([FromQuery] PaginationParams qp, long id)
        {
            var prices = await _context.CustomerPrice
                .Where(x => x.ProductId == id)
                .Include(x => x.Customer)
                .Select(x => PriceToDTO(x))
                .Skip((qp.Page - 1) * qp.PageSize)
                .Take(qp.PageSize)
                .ToListAsync();

            return Ok(prices);
        }

        [HttpGet("customers/{id}")]
        public async Task<ActionResult<IEnumerable<CustomerPriceDTO>>> GetCustomerPrices([FromQuery] PaginationParams qp, long id)
        {
            var prices = await _context.CustomerPrice
                .Where(x => x.CustomerId == id)
                .Include(x => x.Product)
                .Select(x => PriceToDTO(x))
                .Skip((qp.Page - 1) * qp.PageSize)
                .Take(qp.PageSize)
                .ToListAsync();

            return Ok(prices);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerPriceDTO priceDTO)
        {
            if (priceDTO.CustomerId == null || priceDTO.ProductId == null) return BadRequest("Customer and Product is required.");

            var customer = await _context.Customers.FindAsync(priceDTO.CustomerId);
            var product = await _context.Products.FindAsync(priceDTO.ProductId);
            if (customer == null || product == null) return BadRequest("Invalid Customer or Product.");

            var price = new CustomerPrice
            {
                Price = priceDTO.Price,
                MinQty = priceDTO.MinQty,
                CustomerId = (long)priceDTO.CustomerId,
                ProductId = (long)priceDTO.ProductId
            };

            _context.CustomerPrice.Add(price);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] CustomerPriceDTO priceDTO)
        {
            var price = await _context.CustomerPrice.FindAsync(id);

            if (price == null) return NotFound();
            if (priceDTO.CustomerId == null || priceDTO.ProductId == null) return BadRequest("Customer and Product is required.");

            var customer = await _context.Customers.FindAsync(priceDTO.CustomerId);
            var product = await _context.Products.FindAsync(priceDTO.ProductId);
            if (customer == null || product == null) return BadRequest("Customer or Product is invalid.");

            price.Price = priceDTO.Price;
            price.MinQty = priceDTO.MinQty;
            price.CustomerId = (long)priceDTO.CustomerId;
            price.ProductId = (long)priceDTO.ProductId;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var price = await _context.CustomerPrice.FindAsync(id);

            if (price == null) return NotFound();

            _context.CustomerPrice.Remove(price);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
