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
    public class ClaimsController : ControllerBase
    {
        private readonly ProDuckContext _context;

        public ClaimsController(ProDuckContext context)
        {
            _context = context;
        }

        private static ValidationResult ValidateClaim(ClaimDTO dto)
        {
            var result = new ValidationResult();

            if (dto.Name.Length < 3) result.ErrorMessages.Add("Claim name should be longer than 2.");

            if (result.ErrorMessages.Count > 0) return result;

            result.IsValid = true;
            return result;
        }

        [HttpGet]
        public async Task<PaginatedResponse> Get([FromQuery] PaginationParams qp, [FromQuery] string keyword = "")
        {
            var result = await _context.Claims
                .Where(x => x.Name.ToLower().Contains(keyword.ToLower()))
                .ToPagedListAsync(qp.Page, qp.PageSize);

            return new PaginatedResponse(result, new Pagination
            {
                Count = result.Count,
                Page = qp.Page,
                PageSize = qp.PageSize,
                TotalPages = result.TotalPages
            });
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ClaimDTO claimDTO)
        {
            var validation = ValidateClaim(claimDTO);
            if (!validation.IsValid) throw new ApiException(validation.ErrorMessages.First());

            var claim = new Claim
            {
                Name = claimDTO.Name
            };

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("assign")]
        public async Task<IActionResult> PostAssignment([FromBody] UserClaimDTO dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId) ?? throw new ApiException("User not found.");
            var claim = await _context.Claims.FindAsync(dto.ClaimId) ?? throw new ApiException("Claim not found.");

            try
            {
                claim.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                if (ex.InnerException != null) throw new ApiException(ex.InnerException.Message);
                throw new ApiException(ex.Message);
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] ClaimDTO claimDTO)
        {
            var validation = ValidateClaim(claimDTO);
            if (!validation.IsValid) throw new ApiException(validation.ErrorMessages.First());

            var claim = await _context.Claims.FindAsync(id) ?? throw new ApiException("Claim not found.");
            claim.Name = claimDTO.Name;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _context.Claims
                .Include(x => x.Users)
                .Where(x => x.Id == id)
                .ExecuteDeleteAsync();

            return NoContent();
        }
    }
}
