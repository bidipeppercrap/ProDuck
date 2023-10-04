using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public class ProductsController : ControllerBase
    {
        private readonly ProDuckContext _context;

        public ProductsController(ProDuckContext context)
        {
            _context = context;
        }

        [HttpGet("all")]
        [Authorize(Roles = "root, clerk")]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
        {
            var products = await _context.Products
                .Where(x => x.Deleted == false)
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet]
        [Authorize]
        public async Task<PaginatedResponse> GetProducts([FromQuery] long? categoryId, [FromQuery] long? excludeFromLocationId,[FromQuery] PaginationParams qp, [FromQuery] string keyword = "")
        {
            if (_context.Products == null)
            {
                throw new ApiException("Product not found");
            }
            var category = categoryId != null ? await _context.ProductCategories.FindAsync(categoryId) : null;
            if (category == null && categoryId != null) throw new ApiException("Category not found.");


            var q = _context.Products
                .Include(x => x.Category)
                .Include(_ => _.Stocks)
                .Where(x => x.Deleted == false)
                .AsQueryable();


            if (categoryId != null) q = _context.Products
                .Where(x => x.Category == category);

            if (excludeFromLocationId != null) q = q.Where(x => x.Stocks.All(s => s.LocationId != excludeFromLocationId));

            var keywords = keyword.Trim().Split(" ");
            foreach(var word in keywords)
            {
                q = q.Where(x => x.Name.Contains(word));
            }

            try
            {
                var products = await q
                    .OrderByDescending(x => x.Id)
                    .Select(x => ProductToDTO(x))
                    .ToPagedListAsync(qp.Page, qp.PageSize);

                return new PaginatedResponse(
                new Pagination
                {
                    Count = products.Count,
                    PageSize = products.PageSize,
                    Page = products.PageNumber,
                    TotalPages = products.TotalPages
                },
                products
            );
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) throw new ApiException(ex.InnerException.Message);
                throw new ApiException(ex.Message);
            }
        }

        [HttpGet("negativeprice")]
        [Authorize]
        public async Task<PaginatedResponse> GetNegativePriceProducts([FromQuery] PaginationParams qp)
        {
            var products = await _context.Products
                .Where(x => x.Deleted == false)
                .Where(x => x.Price < x.Cost)
                .Select(x => ProductToDTO(x))
                .ToPagedListAsync(qp.Page, qp.PageSize);

            return new PaginatedResponse(products, new Pagination
            {
                Count = products.Count,
                Page = qp.Page,
                PageSize = qp.PageSize,
                TotalPages = products.TotalPages
            });
        }

        [HttpGet("negativecustomerprice")]
        [Authorize]
        public async Task<PaginatedResponse> GetNegativeCustomerPrice([FromQuery] PaginationParams qp)
        {
            var products = await _context.Products
                .Include(x => x.CustomerPrices)
                .Where(x => x.Deleted == false)
                .Where(x => x.CustomerPrices.Any(xx => x.Cost > xx.Price))
                .Select(x => ProductToDTO(x))
                .ToPagedListAsync(qp.Page, qp.PageSize);

            return new PaginatedResponse(products, new Pagination
            {
                Count = products.Count,
                Page = qp.Page,
                PageSize = qp.PageSize,
                TotalPages = products.TotalPages
            });
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<PaginatedResponse> GetProduct(long id)
        {
          if (_context.Products == null)
          {
            throw new ApiException("Product not found.", 404);
          }
            var product = await _context.Products
                .Where(product => product.Id == id)
                .Include(product => product.Category)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                throw new ApiException("Product not found.", 404);
            }

            return new PaginatedResponse(ProductToDTO(product));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "root")]
        public async Task<IActionResult> PutProduct(long id, ProductDTO productDTO)
        {
            if (productDTO.Name.Length < 3) throw new ApiException("Name must be longer than 2.");

            var product = await _context.Products
                .Include(x => x.Category)
                .Where(x => x.Id.Equals(id))
                .FirstOrDefaultAsync();

            if (product == null) throw new ApiException("Product not found.", 404);

            if (productDTO.CategoryId != null)
            {
                var category = await _context.ProductCategories.FindAsync(productDTO.CategoryId);
                if (category == null) new ApiException("Category not found.", 400);

                product.Category = category;
            }
            if (productDTO.CategoryId == null) product.Category = null;

            product.Name = productDTO.Name;
            product.Price = productDTO.Price;
            product.Cost = productDTO.Cost;
            product.Barcode = productDTO.Barcode;

            try
            {
                _context.Products.Update(product);
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost]
        [Authorize(Roles = "root")]
        public async Task<ActionResult<ProductDTO>> PostProduct(ProductDTO productDTO)
        {
            if (productDTO.Name.Length < 3) throw new ApiException("Name must be longer than 2.");

            if (_context.Products == null)
            {
                return Problem("Entity set 'ProDuckContext.Products'  is null.");
            }

            var category = new ProductCategory();
            
            if (productDTO.CategoryId != null)
            {
                category = await _context.ProductCategories.FindAsync(productDTO.CategoryId);
                if (category == null) throw new ApiException("Category not found", 400);
            }

            if (productDTO.CategoryId == null) category = null;

            var product = new Product
            {
                Name = productDTO.Name,
                Price = productDTO.Price,
                Cost = productDTO.Cost,
                Barcode = productDTO.Barcode,
                Category = category
            };

            try
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) throw new ApiException(ex.InnerException.Message);
                throw new ApiException(ex.Message);
            }

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, ProductToDTO(product));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "root")]
        public async Task<IActionResult> DeleteProduct(long id)
        {
            if (_context.Products == null)
            {
                throw new ApiException("Product not found", 404);
            }
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                throw new ApiException("Product not found", 404);
            }

            product.Deleted = true;
            product.Barcode = null;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(long id)
        {
            return (_context.Products?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public static ProductDTO ProductToDTO(Product product) =>
            new()
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Cost = product.Cost,
                Barcode = product.Barcode,
                Stock = product.Stocks.Sum(_ => _.Stock),
                Category = product.Category != null ? new ProductCategoryDTO
                {
                    Id = product.Category?.Id,
                    Name = product.Category?.Name
                } : null
            };
    }
}
