using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProDuck.DTO;
using ProDuck.Models;
using ProDuck.QueryParams;
using System.Collections.ObjectModel;
using System.Dynamic;

namespace ProDuck.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PurchasesController : ControllerBase
    {
        private readonly ProDuckContext _context;

        public PurchasesController(ProDuckContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<dynamic>>> Get([FromQuery] PaginationParams qp)
        {
            return await _context.Purchases
                .Include(p => p.Vendor)
                .Include(p => p.Orders)
                .Select(p => PurchaseToListDTO(p, p.Orders.Sum(o => o.Cost)))
                .Skip((qp.Page - 1) * qp.PageSize)
                .Take(qp.PageSize)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<dynamic>> Get(long id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.Orders)
                    .ThenInclude(o => o.Product)
                .Include(p => p.Vendor)
                .Where(p =>  p.Id == id)
                .Select(p => PurchaseToDTO(p))
                .FirstOrDefaultAsync();

            if (purchase == null) return BadRequest();

            dynamic dto;

            dto = new ExpandoObject();
            dto.Id = purchase!.Id;
            dto.Date = purchase!.Date;
            dto.SourceDocument = purchase.SourceDocument;
            dto.Memo = purchase.Memo;
            dto.Vendor = purchase.Vendor;
            dto.Orders = purchase.Orders;
            dto.IsDelivered = purchase.IsDelivered;

            return dto;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] PurchaseDTO payload)
        {
            using var transaction = _context.Database.BeginTransaction();

            if (!await ValidatePurchase(payload)) return BadRequest();

            var purchase = new Purchase
            {
                Date = payload.Date,
                VendorId = payload.VendorId,
                SourceDocument = payload.SourceDocument,
                Memo = payload.Memo,
                IsDelivered = payload.IsDelivered,
            };

            try
            {
                _context.Purchases.Add(purchase);
                await _context.SaveChangesAsync();

                if (payload.Orders != null)
                {
                    if (payload.Orders!.Count > 0)
                    {
                        foreach (var order in payload.Orders)
                        {
                            var po = new PurchaseOrder
                            {
                                PurchaseId = (long)purchase.Id!,
                                ProductId = order.ProductId,
                                Cost = order.Cost,
                                Quantity = order.Quantity,
                            };

                            _context.PurchaseOrders.Add(po);
                        }

                        await _context.SaveChangesAsync();
                    }
                }

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                throw;
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(long id, [FromBody] PurchaseDTO dto)
        {
            var transaction = _context.Database.BeginTransaction();
            var purchase = await _context.Purchases.FindAsync(id);

            if (purchase == null) return NotFound();

            purchase.Date = dto.Date;
            purchase.VendorId = dto.VendorId;
            purchase.SourceDocument = dto.SourceDocument;
            purchase.Memo = dto.Memo;

            await _context.Purchases
                .Where(p => p.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Date, dto.Date)
                    .SetProperty(p => p.VendorId, dto.VendorId)
                    .SetProperty(p => p.SourceDocument, dto.SourceDocument)
                    .SetProperty(p => p.Memo, dto.Memo));

            var newOrders = new List<PurchaseOrderDTO>();
            var submittedOrders = new List<PurchaseOrderDTO>();

            if (dto.Orders != null)
            {
                newOrders = dto.Orders.Where(o => o.Id == null).ToList();
                submittedOrders = dto.Orders.Where(o => o.Id != null).ToList();
            }

            foreach(var o in newOrders)
            {
                var po = new PurchaseOrder
                {
                    PurchaseId = (long)purchase.Id!,
                    ProductId = o.ProductId,
                    Cost = o.Cost,
                    Quantity = o.Quantity,
                };

                _context.PurchaseOrders.Add(po);
            }

            foreach(var o in submittedOrders)
            {
                await _context.PurchaseOrders
                    .Where(x => x.Id == o.Id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.ProductId, o.ProductId)
                        .SetProperty(x => x.Quantity, o.Quantity)
                        .SetProperty(x => x.Cost, o.Cost));
            }

            if (dto.DeletedOrders != null)
            {
                foreach(long o in dto.DeletedOrders)
                {
                    var order = await _context.PurchaseOrders.FindAsync(o);

                    if (order != null) _context.PurchaseOrders.Remove(order);
                }
            }

            await _context.SaveChangesAsync();

            await _context.Database.CommitTransactionAsync();

            return NoContent();
        }

        private bool PurchaseExists(long id)
        {
            return (_context.Purchases?.Any(p => p.Id == id)).GetValueOrDefault();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(long id)
        {
            var purchase = await _context.Purchases.FindAsync(id);

            if (purchase == null) return NotFound();

            _context.Purchases.Remove(purchase);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> ValidatePurchase(PurchaseDTO purchase)
        {
            var vendor = await _context.Vendors.FindAsync(purchase.VendorId);

            if (vendor == null) return false;

            return true;
        }

        public static dynamic PurchaseToListDTO (Purchase purchase, int totalCost)
        {
            dynamic dto;

            dto = new ExpandoObject();
            dto.Id = purchase.Id;
            dto.Date = purchase.Date;
            dto.SourceDocument = purchase.SourceDocument;
            dto.Vendor = purchase.Vendor.Name;
            dto.TotalCost = totalCost;
            dto.IsDelivered = purchase.IsDelivered;

            return dto;
        }

        private static dynamic PurchaseToDTO(Purchase purchase)
        {
            var orders = new List<dynamic>();
            dynamic vendor = new ExpandoObject();

            vendor.Id = purchase.Vendor.Id;
            vendor.Name = purchase.Vendor.Name;

            foreach (var order in purchase.Orders)
            {
                dynamic o = new ExpandoObject();

                o.Id = order.Id;
                o.Product = order.Product;
                o.Quantity = order.Quantity;
                o.Cost = order.Cost;

                orders.Add(o);
            }

            dynamic dto = new ExpandoObject();

            dto.Id = purchase.Id;
            dto.VendorId = purchase.VendorId;
            dto.Date = purchase.Date;
            dto.SourceDocument = purchase.SourceDocument;
            dto.Memo = purchase.Memo;
            dto.Vendor = vendor;
            dto.Orders = orders;
            dto.IsDelivered = purchase.IsDelivered;

            return dto;
        }
    }
}
