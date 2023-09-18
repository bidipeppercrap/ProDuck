using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProDuck.DTO;
using ProDuck.Models;
using ProDuck.QueryParams;

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
        public async Task<ActionResult<IEnumerable<POSDTO>>> Get([FromQuery] PaginationParams qp, [FromQuery] string keyword = "")
        {
            var poses = await _context.PointOfSale
                .Include(x => x.Sessions.Where(s => s.ClosedAt == null))
                .Where(x => x.IsDeleted == false)
                .Where(x => x.Name.ToLower().Contains(keyword.ToLower()))
                .Select(x => POSToDTO(x))
                .Skip((qp.Page - 1) * qp.PageSize)
                .Take(qp.PageSize)
                .ToListAsync();

            return Ok(poses);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<POSDTO>> Get(long id)
        {
            var pos = await _context.PointOfSale
                .Include(x => x.Sessions.Where(s => s.ClosedAt == null))
                .Include(x => x.AssignedUsers.Where(u => u.IsDeleted == false))
                .Select(x => POSToDTO(x))
                .FirstOrDefaultAsync();

            if (pos == null) return NotFound();

            return Ok(pos);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PointOfSale pos)
        {
            _context.PointOfSale.Add(pos);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] POSDTO posDTO)
        {
            var pos = await _context.PointOfSale.FindAsync(id);

            if (pos == null) return NotFound();

            pos.Name = posDTO.Name;
            pos.Description = posDTO.Description;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var pos = await _context.PointOfSale.FindAsync(id);

            if (pos == null) return NotFound();

            pos.IsDeleted = true;

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
