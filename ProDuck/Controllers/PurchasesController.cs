using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProDuck.DTO;
using ProDuck.Models;
using ProDuck.QueryParams;
using ProDuck.Responses;
using ProDuck.Types;
using System.Collections.ObjectModel;
using System.Dynamic;

namespace ProDuck.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "root")]
    public class PurchasesController : ControllerBase
    {
        private readonly ProDuckContext _context;

        public PurchasesController(ProDuckContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<PaginatedResponse> Get([FromQuery] PaginationParams qp, [FromQuery] long? vendorId, [FromQuery] bool showNotDelivered = false, [FromQuery] string keyword = "")
        {
            var whereQuery = _context.Purchases
                .Include(p => p.Vendor)
                .Include(p => p.Orders).ThenInclude(xx => xx.LandedCostItems).ThenInclude(xxx => xxx.LandedCost)
                .Include(p => p.Orders).ThenInclude(xx => xx.LandedCostItems).ThenInclude(xxx => xxx.Parent).ThenInclude(xxxx => xxxx!.LandedCost)
                .AsQueryable();

            if (vendorId != null) whereQuery = whereQuery.Where(x => x.VendorId == vendorId);
            if (showNotDelivered) whereQuery = whereQuery.Where(x =>
                x.Orders.Sum(o => o.Quantity) != 0
                &&
                x.Orders.Any(o =>
                    o.Quantity != 0 &&
                    o.Quantity
                    >
                    o.LandedCostItems.Where(xx =>
                        (xx.LandedCost != null && xx.LandedCost.IsDelivered && xx.LandedCost.IsPurchase))
                    .Sum(xx => xx.Qty)
                    +
                    o.LandedCostItems.Where(xx =>
                        (xx.LandedCost == null && xx.LandedCostItemId != null && xx.Parent!.LandedCost!.IsDelivered && xx.Parent!.LandedCost!.IsPurchase))
                    .Sum(xx => xx.Qty)));

            var words = keyword.Trim().Split(" ");
            foreach (var word in words) whereQuery = whereQuery.Where(x => x.Vendor.Name.Contains(word) || x.SourceDocument.Contains(word) || x.Memo.Contains(word));

            var result = await whereQuery
                .OrderByDescending(x => x.Date)
                .Select(p => PurchaseToDTO(p))
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
            var purchase = await _context.Purchases
                .Include(p => p.Orders)
                .Include(p => p.Vendor)
                .Where(p =>  p.Id.Equals(id))
                .Select(p => PurchaseToDTO(p))
                .FirstOrDefaultAsync()
                ?? throw new ApiException("Purchase not found.");

            return new PaginatedResponse(purchase);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PurchaseDTO payload)
        {
            var validation = await ValidatePurchaseAsync(payload);
            if (!validation.IsValid) throw new ApiException(validation.ErrorMessages.First());

            var purchase = new Purchase
            {
                Date = payload.Date,
                VendorId = payload.VendorId,
                SourceDocument = payload.SourceDocument,
                Memo = payload.Memo,
            };

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(long id, [FromBody] PurchaseDTO dto)
        {
            var validation = await ValidatePurchaseAsync(dto);
            if (!validation.IsValid) throw new ApiException(validation.ErrorMessages.First());

            var purchase = await _context.Purchases.FindAsync(id) ?? throw new ApiException("Purchase not found.");

            await _context.Purchases
                .Where(p => p.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Date, dto.Date)
                    .SetProperty(p => p.VendorId, dto.VendorId)
                    .SetProperty(p => p.SourceDocument, dto.SourceDocument)
                    .SetProperty(p => p.Memo, dto.Memo));

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

        private async Task<ValidationResult> ValidatePurchaseAsync(PurchaseDTO dto)
        {
            var result = new ValidationResult();
            var vendor = await _context.Vendors.FindAsync(dto.VendorId);

            if (vendor == null) result.ErrorMessages.Add("Vendor not found.");

            if (result.ErrorMessages.Count > 0) return result;

            result.IsValid = true;

            return result;
        }

        public static dynamic PurchaseToListDTO (Purchase purchase, decimal totalCost)
        {
            dynamic dto;

            dto = new ExpandoObject();
            dto.Id = purchase.Id;
            dto.Date = purchase.Date;
            dto.SourceDocument = purchase.SourceDocument;
            dto.Vendor = purchase.Vendor.Name;
            dto.TotalCost = totalCost;

            return dto;
        }

        private static PurchaseDTO PurchaseToDTO(Purchase purchase)
        {
            var dto = new PurchaseDTO
            {
                Id = purchase.Id,
                VendorId = purchase.VendorId,
                Date = purchase.Date,
                SourceDocument = purchase.SourceDocument,
                Memo = purchase.Memo,
                Vendor = new VendorDTO
                {
                    Id = purchase.Vendor.Id,
                    Name = purchase.Vendor.Name
                },
                TotalCost = purchase.Orders.Sum(o => o.Cost * o.Quantity),
                AllDelivered = purchase.Orders.All(x => x.Quantity
                    <=
                    x.LandedCostItems.Where(xx =>
                        (xx.LandedCost != null && xx.LandedCost.IsDelivered && xx.LandedCost.IsPurchase))
                        .Sum(xx => xx.Qty)
                    +
                    x.LandedCostItems.Where(xx =>
                        xx.LandedCost == null && xx.LandedCostItemId != null && xx.Parent!.LandedCost!.IsDelivered && xx.Parent!.LandedCost!.IsPurchase)
                        .Sum(xx => xx.Qty))
            };

            return dto;
        }
    }
}
