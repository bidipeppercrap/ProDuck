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
    [Authorize(Roles = "root")]
    public class SalesController : ControllerBase
    {
        private readonly ProDuckContext _context;

        public SalesController(ProDuckContext context)
        {
            _context = context;
        }

        private static SaleItemDTO SaleItemToDTO(Product product)
        {
            var dto = new SaleItemDTO
            {
                ProductId = product.Id,
                ProductName = product.Name,
                TotalSalePrice = product.OrderItems.Sum(oi => oi.Price * oi.Qty),
                TotalSaleCost = product.OrderItems.Sum(oi => oi.Cost * oi.Qty),
                TotalSold = product.OrderItems.Sum(oi => oi.Qty)
            };

            return dto;
        }

        [HttpGet]
        public async Task<PaginatedResponse> Get([FromQuery] PaginationParams qp, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            try
            {
                var result = await _context.Products
                    .Include(x => x.OrderItems)
                        .ThenInclude(oi => oi.Order)
                    .Where(x =>
                        x.OrderItems.Any(oi => DateOnly.FromDateTime(oi.Order.CreatedAt) >= startDate) &&
                        x.OrderItems.Any(oi => DateOnly.FromDateTime(oi.Order.CreatedAt) <= endDate))
                    .Where(x => x.OrderItems.Sum(oi => oi.Qty) != 0)
                    .OrderByDescending(x => x.OrderItems.Sum(oi => oi.Qty))
                    .Select(x => SaleItemToDTO(x))
                    .ToPagedListAsync(qp.Page, qp.PageSize);

                return new PaginatedResponse(result, new Pagination
                {
                    Count = result.Count,
                    Page = qp.Page,
                    PageSize = qp.PageSize,
                    TotalPages = result.TotalPages
                });
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) throw new ApiException(ex.InnerException.Message);
                throw new ApiException(ex.Message);
            }
        }
    }
}
