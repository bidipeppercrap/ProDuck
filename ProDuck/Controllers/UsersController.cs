using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Mvc;
using ProDuck.DTO;
using ProDuck.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProDuck.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ProDuckContext _context;

        public UsersController(ProDuckContext context)
        {
            _context = context;
        }


        // GET: api/<UsersController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<UsersController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserCreateDTO userDTO)
        {
            if (userDTO.Password.Length < 8) return BadRequest("Password length must be longer than 8 characters.");
            if (userDTO.Username.Length < 3) return BadRequest("Username length must be longer than 3 characters.");

            var passwordHash = Argon2.Hash(userDTO.Password);

            var user = new User
            {
                Name = userDTO.Name ?? null,
                Username = userDTO.Username,
                Password = passwordHash,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT api/<UsersController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<UsersController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
