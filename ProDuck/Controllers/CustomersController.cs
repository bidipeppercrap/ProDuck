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
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly ProDuckContext _context;

        public CustomersController(ProDuckContext context)
        {
            _context = context;
        }

        private static ValidationResult ValidateCustomer(CustomerDTO dto)
        {
            var result = new ValidationResult();

            if (dto.Name.Length < 3) result.ErrorMessages.Add("Name length must be longer than 2 characters.");

            if (result.ErrorMessages.Count > 0) return result;

            result.IsValid = true;

            return result;
        }

        [HttpGet]
        public async Task<PaginatedResponse> Get([FromQuery] PaginationParams qp, [FromQuery] string keyword = "")
        {
            var result = await _context.Customers
                .Include(x => x.ProductPrices)
                    .ThenInclude(pp => pp.Product)
                .Where(x => x.Name.ToLower().Contains(keyword.ToLower()))
                .Where(x => x.IsDeleted == false)
                .Select(x => CustomerToDTO(x))
                .ToPagedListAsync(qp.Page, qp.PageSize);

            return new PaginatedResponse(result, new Pagination
            {
                Count = result.Count,
                Page = qp.Page,
                PageSize = qp.PageSize,
                TotalPages = result.TotalPages
            });
        }

        [HttpGet("{id}")]
        public async Task<PaginatedResponse> Get(long id)
        {
            var customer = await _context.Customers
                .Include(x => x.ProductPrices)
                    .ThenInclude(pp => pp.Product)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (customer == null) throw new ApiException("Customer not found.");

            return new PaginatedResponse(CustomerToDTO(customer));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerDTO customerDTO)
        {
            var validation = ValidateCustomer(customerDTO);

            if (!validation.IsValid) throw new ApiException(validation.ErrorMessages.First());

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
        [Authorize(Roles = "root")]
        public async Task<IActionResult> Put(long id, [FromBody] CustomerDTO customerDTO)
        {
            var validation = ValidateCustomer(customerDTO);

            if (!validation.IsValid) throw new ApiException(validation.ErrorMessages.First());

            var customer = await _context.Customers.FindAsync(id);

            if (customer == null) throw new ApiException("Customer not found.");

            customer.Name = customerDTO.Name;
            customer.Address = customerDTO.Address;
            customer.Phone = customerDTO.Phone;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "root")]
        public async Task<IActionResult> Delete(long id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null) throw new ApiException("Customer not found");

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
