using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProDuck.DTO;
using ProDuck.Models;
using ProDuck.Services;

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

        private async Task<bool> IsSessionClosed(long sessionId)
        {
            var session = await _context.POSSession.FindAsync(sessionId);

            if (session == null) return true;
            if (session.ClosedAt == null) return false;

            return true;
        }

        // GET: api/<OrdersController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<OrdersController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OrderCreateDTO orderDTO)
        {
            using var transaction = _context.Database.BeginTransaction();
            var pos = await _context.PointOfSale.FindAsync(orderDTO.POSId);

            if (pos == null) return BadRequest(new List<string> { "No Point of Sale found.", "Try relogging to find a new PoS." });

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
            catch (Exception e)
            {
                return BadRequest(new List<string> { e.Message });
            }

            return NoContent();
        }
    }
}
