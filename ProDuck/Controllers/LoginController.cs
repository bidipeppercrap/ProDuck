using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProDuck.DTO;
using ProDuck.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProDuck.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ProDuckContext _context;

        public LoginController(ProDuckContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<string>> Login([FromBody] LoginDTO loginDTO)
        {
            var user = await _context.Users
                .Where(x => x.Username == loginDTO.Username)
                .FirstOrDefaultAsync();

            if (user == null) return NotFound("No Username found with the given credentials.");

            if (Argon2.Verify(user.Password, loginDTO.Password) == false) return BadRequest("Incorrect Password.");

            return Ok("Correct!");
        }
    }
}
