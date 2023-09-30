using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProDuck.DTO;
using ProDuck.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Claim = System.Security.Claims.Claim;

namespace ProDuck.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ProDuckContext _context;

        public LoginController(ProDuckContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private static ClaimDTO ClaimToDTO(Models.Claim claim) =>
            new()
            {
                Id = claim.Id,
                Name = claim.Name
            };

        [HttpPost]
        public async Task<ActionResult<string>> Login([FromBody] LoginDTO loginDTO)
        {
            var user = await _context.Users
                .Where(x => x.Username == loginDTO.Username)
                .Where(x => x.IsDeleted == false)
                .FirstOrDefaultAsync();

            if (user == null) return NotFound("No Username found with the given credentials.");

            if (Argon2.Verify(user.Password, loginDTO.Password) == false) return BadRequest("Incorrect Password.");

            var roles = await _context.Claims
                .Where(x => x.Users.Any(u => u.Id == user.Id))
                .Select(x => ClaimToDTO(x))
                .ToListAsync();

            var token = CreateToken(user, roles);

            return Ok(token);
        }

        private string CreateToken(User user, List<ClaimDTO> roles)
        {
            List<Claim> claims = new()
            {
                new Claim(JwtRegisteredClaimNames.Jti, user.Id.ToString()!),
                new Claim(JwtRegisteredClaimNames.Sub, user.Username)
            };

            if (user.Name != null) claims.Add(new Claim(JwtRegisteredClaimNames.Name, user.Name));

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SigningKey"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
