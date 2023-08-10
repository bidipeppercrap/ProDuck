using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.EntityFrameworkCore;
using ProDuck.DTO;
using ProDuck.Models;
using ProDuck.QueryParams;
using System.Dynamic;

namespace ProDuck.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class VendorsController : ControllerBase
    {
        private readonly ProDuckContext _context;

        public VendorsController(ProDuckContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VendorDTO>>> Get([FromQuery] PaginationParams qp, [FromQuery] string keyword = "")
        {
            return await _context.Vendors
                .Where(v => !v.IsDeleted)
                .Where(v => v.Name.Contains(keyword))
                .Select(v => VendorToDTO(v))
                .Skip((qp.Page - 1) * qp.PageSize)
                .Take(qp.PageSize)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(long id)
        {
            var vendor = await _context.Vendors.FindAsync(id);

            if (vendor == null) return BadRequest();

            dynamic vendorDTO;

            vendorDTO = new ExpandoObject();
            vendorDTO.Id = vendor.Id;
            vendorDTO.Name = vendor.Name;
            vendorDTO.Description = vendor.Description;
            vendorDTO.Contact = vendor.Contact;
            vendorDTO.IsDeleted = vendor.IsDeleted;

            return Ok(vendorDTO);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] VendorDTO payload)
        {
            var vendor = new Vendor
            {
                Name = payload.Name,
                Description = payload.Description,
                Contact = payload.Contact
            };

            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] VendorDTO dto)
        {
            var vendor = await _context.Vendors.FindAsync(id);

            if (vendor == null) return BadRequest();

            vendor.Name = dto.Name;
            vendor.Description = dto.Description;
            vendor.Contact = dto.Contact;

            _context.Entry(vendor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(long id)
        {
            var vendor = await _context.Vendors.FindAsync(id);

            if (vendor == null) return NotFound();

            vendor.IsDeleted = true;
            _context.Entry(vendor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return NoContent();
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
