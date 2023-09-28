using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.EntityFrameworkCore;
using ProDuck.DTO;
using ProDuck.Models;
using ProDuck.QueryParams;
using ProDuck.Responses;
using ProDuck.Types;
using System.Dynamic;

namespace ProDuck.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "root")]
    public class VendorsController : ControllerBase
    {
        private readonly ProDuckContext _context;

        public VendorsController(ProDuckContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<PaginatedResponse> Get([FromQuery] PaginationParams qp, [FromQuery] string keyword = "")
        {
            var whereQuery = _context.Vendors
                .Where(v => !v.IsDeleted)
                .AsQueryable();
            
            foreach(var word in keyword.Trim().Split(" ")) whereQuery = whereQuery.Where(x => x.Name.Contains(word));

            var result = await whereQuery
                .Select(v => VendorToDTO(v))
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
            var vendor = await _context.Vendors.FindAsync(id) ?? throw new ApiException("Vendor not found.");

            return new PaginatedResponse(VendorToDTO(vendor));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] VendorDTO payload)
        {
            var validation = ValidateVendor(payload);
            if (!validation.IsValid) throw new ApiException(validation.ErrorMessages.First());

            var vendor = new Vendor
            {
                Name = payload.Name,
                Description = payload.Description,
                Contact = payload.Contact
            };

            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] VendorDTO dto)
        {
            var validation = ValidateVendor(dto);
            if (!validation.IsValid) throw new ApiException(validation.ErrorMessages.First());

            var vendor = await _context.Vendors.FindAsync(id) ?? throw new ApiException("Unable to find Vendor");

            vendor.Name = dto.Name;
            vendor.Description = dto.Description;
            vendor.Contact = dto.Contact;

            _context.Entry(vendor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (ex.InnerException != null) throw new ApiException(ex.InnerException.Message);
                throw new ApiException(ex.Message);
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(long id)
        {
            var vendor = await _context.Vendors.FindAsync(id) ?? throw new ApiException("Could not find Vendor.");

            vendor.IsDeleted = true;
            _context.Entry(vendor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (ex.InnerException != null) throw new ApiException(ex.InnerException.Message);
                throw new ApiException(ex.Message);
            }

            return NoContent();
        }

        private static ValidationResult ValidateVendor(VendorDTO dto)
        {
            var result = new ValidationResult();

            if (dto.Name.Length < 3) result.ErrorMessages.Add("Vendor Name should at least 3 characters.");

            if (result.ErrorMessages.Count > 0) return result;

            result.IsValid = true;
            return result;
        }

        private bool VendorExists(long id)
        {
            return (_context.Vendors?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public static VendorDTO VendorToDTO(Vendor vendor) =>
            new()
            {
                Id = vendor.Id,
                Name = vendor.Name,
                Description = vendor.Description,
                Contact = vendor.Contact,
            };
    }
}
