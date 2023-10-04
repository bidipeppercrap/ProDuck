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
                ProductsCount = category.Products.Count,
                ChildCategoriesCount = category.ChildCategories.Count,
                MinQty = category.MinQty,
                TotalStock = category.Products.Sum(xx => xx.Stocks.Sum(s => s.Stock))
            };

        [HttpGet]
        public async Task<PaginatedResponse> GetCategories([FromQuery] long? exclude, [FromQuery] long? parentId, [FromQuery] PaginationParams qp, [FromQuery] bool showOnlyRootChilds = false, [FromQuery] string keyword = "")
        {
            var whereQuery = _context.ProductCategories
                .Include(x => x.Products)
                .Include(x => x.ChildCategories)
                .AsQueryable();

            if (parentId != null && !showOnlyRootChilds) whereQuery = whereQuery.Where(x => x.ProductCategoryId.Equals(parentId));
            if (showOnlyRootChilds) whereQuery = whereQuery.Where(x => x.ProductCategoryId == null);

            foreach(var word in keyword.Trim().Split(" "))
            {
                whereQuery = whereQuery.Where(x => x.Name.Contains(word));
            }

            if (exclude != null)
            {
                var data = await whereQuery
                    .Where(x => x.Id != exclude)
                    .OrderByDescending(x => x.Id)
                    .Select(x => CategoryToDTO(x))
                    .ToPagedListAsync(qp.Page, qp.PageSize);

                return new PaginatedResponse(data, new Pagination { Count = data.Count, Page = qp.Page, PageSize = qp.PageSize, TotalPages = data.TotalPages });
            }

            var result = await whereQuery
                .OrderByDescending(x => x.Id)
                .Select(x => CategoryToDTO(x))
                .ToPagedListAsync(qp.Page, qp.PageSize);

            return new PaginatedResponse(result, new Pagination { Count = result.Count, Page = qp.Page, PageSize = qp.PageSize, TotalPages = result.TotalPages });
        }

        [HttpGet("replenishment")]
        public async Task<PaginatedResponse> GetReplenishment([FromQuery] PaginationParams qp)
        {
            var categories = await _context.ProductCategories
                .Include(x => x.Products).ThenInclude(xx => xx.Stocks)
                .Where(x => x.MinQty >= x.Products.Sum(xx => xx.Stocks.Sum(s => s.Stock)))
                .Select(x => CategoryToDTO(x))
                .ToPagedListAsync(qp.Page, qp.PageSize);

            return new PaginatedResponse(categories, new Pagination
            {
                Count = categories.Count,
                Page = qp.Page,
                PageSize = qp.PageSize,
                TotalPages = categories.TotalPages
            });
        }

        [HttpGet("{id}")]
        public async Task<PaginatedResponse> GetCategory(long id)
        {
            var category = await _context.ProductCategories
                .Include(x => x.Products)
                .Include(x => x.ChildCategories)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            return category == null ? throw new ApiException("Category not found.", 404) : new PaginatedResponse(CategoryToDTO(category));
        }

        [HttpPost]
        [Authorize(Roles = "root")]
        public async Task<ActionResult<ProductCategoryDTO>> PostCategory(ProductCategoryDTO categoryDTO)
        {
            if (categoryDTO.Name == null) throw new ApiException("Category Name is required.");
            if (categoryDTO.Name.Length < 3) throw new ApiException("Category Name must be longer than 2 characters.");

            var category = new ProductCategory
            {
                Name = categoryDTO.Name,
                Description = categoryDTO.Description,
                MinQty = categoryDTO.MinQty ?? 0
            };

            if (categoryDTO.ProductCategoryId != null)
            {
                var parentCategory = await _context.ProductCategories.FindAsync(categoryDTO.ProductCategoryId) ?? throw new ApiException("Parent Category not found.");

                category.ProductCategoryId = categoryDTO.ProductCategoryId;
            }

            _context.ProductCategories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id =  category.Id }, CategoryToDTO(category));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "root")]
        public async Task<IActionResult> PutCategory(long id, ProductCategoryDTO categoryDTO)
        {
            var category = await _context.ProductCategories.FindAsync(id);

            if (category == null) throw new ApiException("Category not found", 404);
            if (category.Id == categoryDTO.ProductCategoryId) throw new ApiException("Parent Category cannot be thy self.");

            if (categoryDTO.ProductCategoryId != null)
            {
                var parentCategory = await _context.ProductCategories.FindAsync(categoryDTO.ProductCategoryId);
                if (parentCategory == null) throw new ApiException("Parent Category not found.");
            }

            await _context.ProductCategories
                .Where(c => c.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(c => c.Name, categoryDTO.Name)
                    .SetProperty(c => c.Description, categoryDTO.Description)
                    .SetProperty(c => c.ProductCategoryId, categoryDTO.ProductCategoryId)
                    .SetProperty(c => c.MinQty, categoryDTO.MinQty ?? 0));

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "root")]
        public async Task<IActionResult> DeleteCategory(long id)
        {
            using var transaction = _context.Database.BeginTransaction();

            var category = await _context.ProductCategories.FindAsync(id) ?? throw new ApiException("Category not found.", 404);

            await _context.ProductCategories
                .Where(c => c.ParentCategory == category)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(c => c.ProductCategoryId, category.ProductCategoryId));

            await _context.Products
                .Include(x => x.Category)
                .Where(x => x.Category == category)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.CategoryId, category.ProductCategoryId));

            _context.ProductCategories.Remove(category);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return NoContent();
        }
    }
}
