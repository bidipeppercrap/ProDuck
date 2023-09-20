using AutoWrapper.Wrappers;
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
    public class POSController : ControllerBase
    {
        private readonly ProDuckContext _context;

        public POSController(ProDuckContext context)
        {
            _context = context;
        }

        private static ValidationResult ValidatePOS(PointOfSale pos)
        {
            var result = new ValidationResult();

            if (pos.Name.Length < 3) result.ErrorMessages.Add("PoS Name should be longer than 2 characters.");

            if (result.ErrorMessages.Count > 0) return result;

            result.IsValid = true;

            return result;
        }

        private static POSDTO POSToDTO(PointOfSale pos)
        {
            var assignedUsers = new List<POSDTOAssignedUser>();

            foreach (var u in pos.AssignedUsers)
                assignedUsers.Add(new POSDTOAssignedUser
                {
                    Id = u.Id,
                    Username = u.Username,
                    Name = u.Name,
                });

            var lastSession = pos.Sessions.LastOrDefault();

            var dto = new POSDTO
            {
                Id = pos.Id,
                Name = pos.Name,
                Description = pos.Description,
                LastSession = lastSession != null ? new POSDTOSession
                {
                    Id = lastSession.Id,
                    OpenedAt = lastSession.OpenedAt,
                    ClosedAt = lastSession.ClosedAt
                } : null,
                AssignedUsers = assignedUsers
            };

            return dto;
        }

        [HttpGet]
        public async Task<PaginatedResponse> Get([FromQuery] PaginationParams qp, [FromQuery] string keyword = "")
        {
            var result = await _context.PointOfSale
                .Include(x => x.Sessions.Where(s => s.ClosedAt == null))
                .Where(x => x.IsDeleted == false)
                .Where(x => x.Name.ToLower().Contains(keyword.ToLower()))
                .Select(x => POSToDTO(x))
                .ToPagedListAsync(qp.Page, qp.PageSize);

            return new PaginatedResponse(result, new Pagination
            {
                Count = result.Count,
                Page = qp.Page,
                PageSize = qp.PageSize,
                TotalPages = result.TotalPages,
            });
        }

        [HttpGet("{id}")]
        public async Task<PaginatedResponse> Get(long id)
        {
            var pos = await _context.PointOfSale
                .Include(x => x.Sessions.Where(s => s.ClosedAt == null))
                .Include(x => x.AssignedUsers.Where(u => u.IsDeleted == false))
                .Where(x => x.Id == id)
                .Select(x => POSToDTO(x))
                .FirstOrDefaultAsync();

            return pos == null ? throw new ApiException("PoS not found.") : new PaginatedResponse(pos);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PointOfSale pos)
        {
            var validation = ValidatePOS(pos);
            if (!validation.IsValid) throw new ApiException(validation.ErrorMessages.First());

            _context.PointOfSale.Add(pos);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] PointOfSale posDTO)
        {
            var validation = ValidatePOS(posDTO);
            if (!validation.IsValid) throw new ApiException(validation.ErrorMessages.First());

            var pos = await _context.PointOfSale.FindAsync(id);
            if (pos == null) throw new ApiException("PoS not found.");

            pos.Name = posDTO.Name;
            pos.Description = posDTO.Description;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var pos = await _context.PointOfSale.FindAsync(id);

            if (pos == null) throw new ApiException("PoS not found.");

            pos.IsDeleted = true;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
