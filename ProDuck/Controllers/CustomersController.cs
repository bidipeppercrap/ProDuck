using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProDuck.DTO;
using ProDuck.Models;
using ProDuck.QueryParams;

namespace ProDuck.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ProDuckContext _context;

        public CustomersController(ProDuckContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> Get([FromQuery] PaginationParams qp, [FromQuery] string keyword = "")
        {
            return await _context.Customers
                .Where(x => x.Name.ToLower().Contains(keyword.ToLower()))
                .Where(x => x.IsDeleted == false)
                .Include(x => x.ProductPrices)
                .Select(x => CustomerToDTO(x))
                .Skip((qp.Page - 1) * qp.PageSize)
                .Take(qp.PageSize)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDTO>> Get(long id)
        {
            var customer = await _context.Customers
                .Where(x => x.Id == id)
                .Include(x => x.ProductPrices)
                .FirstOrDefaultAsync();

            if (customer == null) return NotFound();

            return Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerDTO customerDTO)
        {
            var customer = new Customer
            {
                Name = customerDTO.Name,
                Address = customerDTO.Address,
                Phone = customerDTO.Phone
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] CustomerDTO customerDTO)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null) return NotFound();

            customer.Name = customerDTO.Name;
            customer.Address = customerDTO.Address;
            customer.Phone = customerDTO.Phone;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null) return NotFound();

            customer.IsDeleted = true;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static CustomerDTO CustomerToDTO(Customer customer)
        {
            var dto = new CustomerDTO
            {
                Id = customer.Id,
                Name = customer.Name,
                Address = customer.Address,
                Phone = customer.Phone
            };

            if (customer.ProductPrices != null)
            {
                foreach (var productPrice in customer.ProductPrices)
                {
                    var priceDTO = new CustomerDTOProductPrice
                    {
                        Id = productPrice.Id,
                        MinQty = productPrice.MinQty,
                        Price = productPrice.Price,
                        Product = new CustomerDTOProduct
                        {
                            Id = productPrice.Product.Id,
                            Name = productPrice.Product.Name
                        }
                    };

                    dto.Prices.Add(priceDTO);
                }
            }

            return dto;
        }
    }
}
