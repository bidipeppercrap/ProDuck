using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProDuck.DTO;
using ProDuck.Models;
using ProDuck.QueryParams;
using ProDuck.Responses;
using ProDuck.Types;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace ProDuck.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
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
        public async Task<PaginatedResponse> GetProductPrices([FromQuery] PaginationParams qp, long id, [FromQuery] string keyword = "")
        {
            var whereQuery = _context.CustomerPrice
                .Include(x => x.Customer)
                .Where(x => x.ProductId == id)
                .AsQueryable();

            var keywords = keyword.Trim().Split(" ");
            foreach (var word in keywords) whereQuery = whereQuery.Where(x => x.Customer.Name.Contains(word));

            var prices = await whereQuery
                .OrderByDescending(x => x.Id)
                .Select(x => PriceToDTO(x))
                .ToPagedListAsync(qp.Page, qp.PageSize);

            return new PaginatedResponse(prices, new Pagination { Count = prices.Count, Page = qp.Page, PageSize = qp.PageSize, TotalPages = prices.TotalPages });
        }

        [HttpGet("customers/{id}")]
        public async Task<PaginatedResponse> GetCustomerPrices([FromQuery] PaginationParams qp, long id, [FromQuery] string keyword = "")
        {
            var whereQuery = _context.CustomerPrice
                .Include(x => x.Product)
                .Where(x => x.CustomerId == id)
                .AsQueryable();

            var keywords = keyword.Trim().Split(" ");
            foreach (var word in keywords) whereQuery = whereQuery.Where(x => x.Product.Name.Contains(word));
            
            var prices = await whereQuery
                .OrderByDescending(x => x.Id)
                .Select(x => PriceToDTO(x))
                .ToPagedListAsync(qp.Page, qp.PageSize);

            return new PaginatedResponse(prices, new Pagination
            {
                Count = prices.Count,
                Page = qp.Page,
                PageSize = qp.PageSize,
                TotalPages = prices.TotalPages
            });
        }

        [HttpGet("customers/all/{id}")]
        public async Task<ActionResult<IEnumerable<CustomerPrice>>> GetAllCustomerPrices(long id)
        {
            var prices = await _context.CustomerPrice
                .Where(x => x.CustomerId.Equals(id))
                .ToListAsync();

            return Ok(prices);
        }

        [HttpPost]
        [Authorize(Roles = "root")]
        public async Task<IActionResult> Post([FromBody] CustomerPriceDTO priceDTO)
        {
            if (priceDTO.CustomerId == null || priceDTO.ProductId == null) throw new ApiException("Customer and Product is required.");

            var customer = await _context.Customers.FindAsync(priceDTO.CustomerId);
            var product = await _context.Products.FindAsync(priceDTO.ProductId);
            if (customer == null || product == null) throw new ApiException("Customer or Product not found.");

            var duplicates = await _context.CustomerPrice
                .Where(x => x.CustomerId.Equals(priceDTO.CustomerId))
                .Where(x => x.ProductId.Equals(priceDTO.ProductId))
                .FirstOrDefaultAsync();
            if (duplicates != null) throw new ApiException("Price already exist.");

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
        [Authorize(Roles = "root")]
        public async Task<IActionResult> Put(long id, [FromBody] CustomerPriceDTO priceDTO)
        {
            var price = await _context.CustomerPrice.FindAsync(id);

            if (price == null) throw new ApiException("Price not found.");
            if (priceDTO.CustomerId == null || priceDTO.ProductId == null) throw new ApiException("Customer and Product is required.");

            var customer = await _context.Customers.FindAsync(priceDTO.CustomerId);
            var product = await _context.Products.FindAsync(priceDTO.ProductId);
            if (customer == null || product == null) throw new ApiException("Customer or Product is invalid.");

            price.Price = priceDTO.Price;
            price.MinQty = priceDTO.MinQty;
            price.CustomerId = (long)priceDTO.CustomerId;
            price.ProductId = (long)priceDTO.ProductId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "root")]
        public async Task<IActionResult> Delete(long id)
        {
            var price = await _context.CustomerPrice.FindAsync(id) ?? throw new ApiException("Price not found.");

            _context.CustomerPrice.Remove(price);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
