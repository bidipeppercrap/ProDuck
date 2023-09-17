using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProDuck.DTO;
using ProDuck.Models;
using ProDuck.QueryParams;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts([FromQuery] long? categoryId,[FromQuery] PaginationParams qp)
        {
            if (_context.Products == null)
            {
                return NotFound();
            }
            var category = categoryId != null ? await _context.ProductCategories.FindAsync(categoryId) : null;

            var q = _context.Products
                .Where(x => x.Deleted == false); ;

            if (categoryId != null) q = _context.Products
                .Where(x => x.Deleted == false)
                .Where(x => x.Category == category);

            return await q
                .Include(x => x.Category)
                .Select(x => ProductToDTO(x))
                .Skip((qp.Page - 1) * qp.PageSize)
                .Take(qp.PageSize)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetProduct(long id)
        {
          if (_context.Products == null)
          {
              return NotFound();
          }
            var product = await _context.Products
                .Where(product => product.Id == id)
                .Include(product => product.Category)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            return ProductToDTO(product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(long id, ProductDTO productDTO)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null) return NotFound();

            if (productDTO.CategoryId != null)
            {
                var category = await _context.ProductCategories.FindAsync(productDTO.CategoryId);
                if (category == null) return BadRequest();

                product.Category = category;
            }
            if (productDTO.CategoryId == null) product.Category = null;

            product.Name = productDTO.Name;
            product.Price = productDTO.Price;
            product.Cost = productDTO.Cost;
            product.Barcode = productDTO.Barcode;

            _context.Products.Update(product);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<ProductDTO>> PostProduct(ProductDTO productDTO)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'ProDuckContext.Products'  is null.");
            }

            var category = new ProductCategory();
            
            if (productDTO.CategoryId != null)
            {
                category = await _context.ProductCategories.FindAsync(productDTO.CategoryId);
                if (category == null) return BadRequest();
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

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, ProductToDTO(product));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(long id)
        {
            if (_context.Products == null)
            {
                return NotFound();
            }
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
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
                Category = product.Category != null ? new ProductCategoryDTO
                {
                    Id = product.Category?.Id,
                    Name = product.Category?.Name
                } : null
            };
    }
}
