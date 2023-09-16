using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProDuck.DTO;
using ProDuck.Models;
using ProDuck.QueryParams;

namespace ProDuck.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {

        private readonly ProDuckContext _context;

        public CategoriesController(ProDuckContext context)
        {
            _context = context;
        }

        private static ProductCategoryDTO CategoryToDTO(ProductCategory category) =>
            new()
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ProductCategoryId = category.ProductCategoryId,
                ProductsCount = category.Products.Count
            };

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductCategoryDTO>>> GetCategories([FromQuery] long? exclude, [FromQuery] PaginationParams qp)
        {
            var whereQuery = _context.ProductCategories;

            if (exclude != null)
            {
                return await whereQuery
                    .Where(x => x.Id != exclude)
                    .Include(x => x.Products)
                    .Select(x => CategoryToDTO(x))
                    .Skip((qp.Page - 1) * qp.PageSize)
                    .Take(qp.PageSize)
                    .ToListAsync(); 
            }

            return await whereQuery
                .Include(x => x.Products)
                .Select(x => CategoryToDTO(x))
                .Skip((qp.Page - 1) * qp.PageSize)
                .Take(qp.PageSize)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductCategoryDTO>> GetCategory(long id)
        {
            var category = await _context.ProductCategories
                .Where(x => x.Id == id)
                .Include(x => x.Products)
                .FirstOrDefaultAsync();

            if (category == null)
            {
                return NotFound();
            }

            return CategoryToDTO(category);
        }

        [HttpPost]
        public async Task<ActionResult<ProductCategoryDTO>> PostCategory(ProductCategoryDTO categoryDTO)
        {
            if (categoryDTO.Name == null) { return BadRequest(); }

            var category = new ProductCategory
            {
                Name = categoryDTO.Name,
                Description = categoryDTO.Description,
            };

            if (categoryDTO.ProductCategoryId != null)
            {
                var parentCategory = await _context.ProductCategories.FindAsync(categoryDTO.ProductCategoryId);

                if (parentCategory == null)
                {
                    return BadRequest();
                }

                category.ProductCategoryId = categoryDTO.ProductCategoryId;
            }

            _context.ProductCategories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id =  category.Id }, CategoryToDTO(category));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(long id, ProductCategoryDTO categoryDTO)
        {
            var category = await _context.ProductCategories.FindAsync(id);

            if (category == null) return NotFound();
            if (category.Id == categoryDTO.ProductCategoryId) return BadRequest();

            if (categoryDTO.ProductCategoryId != null)
            {
                var parentCategory = await _context.ProductCategories.FindAsync(categoryDTO.ProductCategoryId);
                if (parentCategory == null) return BadRequest();
            }

            await _context.ProductCategories
                .Where(c => c.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(c => c.Name, categoryDTO.Name)
                    .SetProperty(c => c.Description, categoryDTO.Description)
                    .SetProperty(c => c.ProductCategoryId, categoryDTO.ProductCategoryId));

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(long id)
        {
            using var transaction = _context.Database.BeginTransaction();

            var category = await _context.ProductCategories.FindAsync(id);

            if (category == null) return NotFound();

            await _context.ProductCategories
                .Where(c => c.ParentCategory == category)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(c => c.ProductCategoryId, category.ProductCategoryId));

            _context.ProductCategories.Remove(category);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return NoContent();
        }
    }
}
