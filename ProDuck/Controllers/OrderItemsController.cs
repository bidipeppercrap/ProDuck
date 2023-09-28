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
    public class OrderItemsController : ControllerBase
    {
        private readonly ProDuckContext _context;

        public OrderItemsController(ProDuckContext context)
        {
            _context = context;
        }

        [HttpGet("orders/{id}")]
        public async Task<PaginatedResponse> GetByOrder(long id, [FromQuery] PaginationParams qp, [FromQuery] string keyword = "")
        {
            var whereQuery = _context.OrdersItem
                .Include(x => x.Product)
                .Where(x => x.OrderId == id)
                .AsQueryable();

            var words = keyword.Trim().Split(" ");
            foreach(var word in words)
            {
                whereQuery = whereQuery.Where(x => x.Product.Name.Contains(word));
            }

            var orderItems = await whereQuery
                .Select(x => OrderItemToDTO(x))
                .ToPagedListAsync(qp.Page, qp.PageSize);

            return new PaginatedResponse(orderItems, new Pagination
            {
                Count = orderItems.Count,
                Page = qp.Page,
                PageSize = qp.PageSize,
                TotalPages = orderItems.TotalPages
            });
        }

        private static OrderItemDTO OrderItemToDTO(OrderItem orderItem)
        {
            var dto = new OrderItemDTO
            {
                Id = orderItem.Id,
                Qty = orderItem.Qty,
                Price = orderItem.Price,
                Cost = orderItem.Cost,
                IsReturn = orderItem.Qty < 0,
                Product = new OrderItemDTOProduct
                {
                    Id = orderItem.Product.Id,
                    Name = orderItem.Product.Name,
                    Barcode = orderItem.Product.Barcode
                }
            };

            return dto;
        }
    }
}
