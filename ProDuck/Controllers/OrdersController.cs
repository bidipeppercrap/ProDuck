using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProDuck.DTO;
using ProDuck.Models;
using ProDuck.QueryParams;
using ProDuck.Responses;
using ProDuck.Services;
using ProDuck.Types;

namespace ProDuck.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ProDuckContext _context;

        public OrdersController(ProDuckContext context)
        {
            _context = context;
        }

        private static OrderDTO OrderToDTO(Order order)
        {
            var dto = new OrderDTO
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                CustomerId = order.CustomerId,
                Customer = order.Customer != null ? new OrderDTOCustomer
                {
                    Id = order.Customer.Id,
                    Name = order.Customer.Name,
                } : null,
                UserId = order.UserId,
                ServedBy = new UserDTO
                {
                    Id = order.ServedBy.Id,
                    Name = order.ServedBy.Name,
                    Username = order.ServedBy.Username
                },
                POSSessionId = order.POSSessionId,
                TotalPrice = order.Items.Sum(_ => _.Price * _.Qty),
                TotalCost = order.Items.Sum(_ => _.Cost * _.Qty),
                HasReturn = order.Items.Any(_ => _.Qty < 0)
            };

            return dto;
        }

        private static OrderDTOItem ItemToDTO(OrderItem item)
        {
            var dto = new OrderDTOItem
            {
                Id = item.Id,
                Price = item.Price,
                Cost = item.Cost,
                Qty = item.Qty,
                Product = new OrderDTOItemProduct
                {
                    Id = item.ProductId
                }
            };

            return dto;
        }

        private async Task<bool> IsSessionClosed(long sessionId)
        {
            var session = await _context.POSSession.FindAsync(sessionId);

            if (session == null) return true;
            if (session.ClosedAt == null) return false;

            return true;
        }

        [HttpGet]
        public async Task<PaginatedResponse> Get([FromQuery] PaginationParams qp, [FromQuery] long? userId, [FromQuery] long? customerId)
        {
            IQueryable<Order> whereQuery = _context.Orders
                .Include(x => x.ServedBy)
                .Include(x => x.Customer)
                .Include(x => x.Items);

            if (userId != null) whereQuery = whereQuery.Where(_ => _.UserId == userId);
            if (customerId != null) whereQuery = whereQuery.Where(_ => _.CustomerId == customerId);

            var orders = await whereQuery
                .Select(_ => OrderToDTO(_))
                .ToPagedListAsync(qp.Page, qp.PageSize);

            return new PaginatedResponse(orders, new Pagination
            {
                Count = orders.Count,
                Page = qp.Page,
                PageSize = qp.PageSize,
                TotalPages = orders.TotalPages
            });
        }

        [HttpGet("possessions/{id}")]
        public async Task<PaginatedResponse> GetBySession(long id, [FromQuery] PaginationParams qp, [FromQuery] long? userId, [FromQuery] long? customerId)
        {
            var posSession = await _context.POSSession.FindAsync(id) ?? throw new ApiException("PoS Session not found.");

            IQueryable<Order> whereQuery = _context.Orders
                .Include(x => x.ServedBy)
                .Include(x => x.Customer)
                .Include(x => x.Items);

            if (userId != null) whereQuery = whereQuery.Where(_ => _.UserId == userId);
            if (customerId != null) whereQuery = whereQuery.Where(_ => _.CustomerId == customerId);

            var orders = await whereQuery
                .Where(_ => _.POSSessionId == id)
                .Select(_ => OrderToDTO(_))
                .ToPagedListAsync(qp.Page, qp.PageSize);

            return new PaginatedResponse(orders, new Pagination
            {
                Count = orders.Count,
                Page = qp.Page,
                PageSize = qp.PageSize,
                TotalPages = orders.TotalPages
            });
        }

        [HttpGet("poses/{id}")]
        public async Task<PaginatedResponse> GetByPOS(long id, [FromQuery] PaginationParams qp, [FromQuery] long? userId, [FromQuery] long? customerId)
        {
            var pos = await _context.PointOfSale.FindAsync(id) ?? throw new ApiException("PoS not found.");

            IQueryable<Order> whereQuery = _context.Orders
                .Include(x => x.ServedBy)
                .Include(x => x.Customer)
                .Include(x => x.POSSession)
                .Include(x => x.Items);

            if (userId != null) whereQuery = whereQuery.Where(_ => _.UserId == userId);
            if (customerId != null) whereQuery = whereQuery.Where(_ => _.CustomerId == customerId);

            var orders = await whereQuery
                .Where(_ => _.POSSession.POSId == id)
                .Select(_ => OrderToDTO(_))
                .ToPagedListAsync(qp.Page, qp.PageSize);

            return new PaginatedResponse(orders, new Pagination
            {
                Count = orders.Count,
                TotalPages = orders.TotalPages,
                Page = qp.Page,
                PageSize = qp.PageSize
            });
        }

        [HttpPost("return")]
        public async Task<PaginatedResponse> PostToReturn([FromBody] List<OrderDTOItem> itemsDTO)
        {
            var whereQuery = _context.Orders
                .Include(x => x.ServedBy)
                .Include(x => x.Customer)
                .Include(x => x.POSSession)
                .Include(x => x.Items)
                .AsQueryable();

            foreach(var dto in itemsDTO)
            {
                whereQuery = whereQuery.Where(x => x.Items.Any(i => i.ProductId == dto.Product.Id && i.Qty >= dto.Qty));
            }

            var orders = await whereQuery
                .Select(x => OrderToDTO(x))
                .Take(80)
                .ToListAsync();

            var result = orders.OrderByDescending(x => x.CreatedAt);

            return new PaginatedResponse(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OrderCreateDTO orderDTO)
        {
            using var transaction = _context.Database.BeginTransaction();

            var pos = await _context.PointOfSale.FindAsync(orderDTO.POSId) ?? throw new ApiException("No PoS found, try relogging the application.");

            try
            {
                decimal openingBalance = 0;

                var lastOpenSession = await _context.POSSession
                    .Where(_ => _.POSId == orderDTO.POSId)
                    .Where(_ => _.ClosedAt == null)
                    .OrderBy(_ => _.OpenedAt)
                    .LastOrDefaultAsync();

                if (lastOpenSession == null)
                {
                    var lastClosedSession = await _context.POSSession
                        .Where(_ => _.POSId == orderDTO.POSId)
                        .OrderBy(_ => _.OpenedAt)
                        .LastOrDefaultAsync();

                    if (lastClosedSession != null) openingBalance = lastClosedSession.ClosingBalance;

                    var session = new POSSession
                    {
                        OpenedAt = DateTime.Now,
                        OpeningBalance = openingBalance,
                        UserId = orderDTO.UserId,
                        POSId = orderDTO.POSId
                    };

                    _context.POSSession.Add(session);
                    await _context.SaveChangesAsync();

                    lastOpenSession = session;
                }

                var order = new Order
                {
                    CreatedAt = orderDTO.CreatedAt,
                    UserId = orderDTO.UserId,
                    CustomerId = orderDTO.CustomerId,
                    POSSessionId = lastOpenSession.Id
                };

                foreach (var i in orderDTO.Items)
                {
                    var item = new OrderItem
                    {
                        ProductId = i.Product.Id,
                        Qty = i.Qty,
                        Price = i.Price,
                        Cost = i.Cost,
                    };

                    order.Items.Add(item);
                    await StockService.ModifyStock(item.ProductId, item.Qty * -1, _context);
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) throw new ApiException(ex.InnerException.Message);
                throw new ApiException(ex.Message);
            }

            return NoContent();
        }
    }
}
