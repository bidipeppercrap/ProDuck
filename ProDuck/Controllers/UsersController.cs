using AutoWrapper.Wrappers;
using Isopoh.Cryptography.Argon2;
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
    public class UsersController : ControllerBase
    {
        private readonly ProDuckContext _context;

        public UsersController(ProDuckContext context)
        {
            _context = context;
        }

        private static UserDTO UserToDTO(User user) =>
            new()
            {
                Id = user.Id,
                Name = user.Name,
                Username = user.Username
            };
        private static ValidationResult ValidateDTO(UserCreateDTO dto)
        {
            var result = new ValidationResult();

            if (dto.Password.Length < 8) result.ErrorMessages.Add("Password length must be longer than 8 characters.");
            if (dto.Username.Length < 3) result.ErrorMessages.Add("Username length must be longer than 3 characters.");

            if (result.ErrorMessages.Count > 0) return result;

            result.IsValid = true;

            return result;
        }

        [HttpGet]
        public async Task<PaginatedResponse> Get([FromQuery] PaginationParams qp, [FromQuery] string keyword = "")
        {
            var result = await _context.Users
                .Where(x => x.Username.ToLower().Contains(keyword) || (x.Name == null || x.Name.ToLower().Contains(keyword)))
                .Where(x => x.IsDeleted == false)
                .Select(x => UserToDTO(x))
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
            var user = await _context.Users.FindAsync(id) ?? throw new ApiException("User not found.");

            return new PaginatedResponse(UserToDTO(user));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserCreateDTO userDTO)
        {
            var validation = ValidateDTO(userDTO);
            if (!validation.IsValid) throw new ApiException(validation.ErrorMessages.First());

            var passwordHash = Argon2.Hash(userDTO.Password);

            var user = new User
            {
                Name = userDTO.Name ?? null,
                Username = userDTO.Username,
                Password = passwordHash,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] UserCreateDTO userDTO)
        {
            var validation = ValidateDTO(userDTO);
            if (!validation.IsValid) throw new ApiException(validation.ErrorMessages.First());

            var user = await _context.Users.FindAsync(id) ?? throw new ApiException("User not found.");

            var passwordHash = Argon2.Hash(userDTO.Password);

            await _context.Users
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.Name, userDTO.Name)
                    .SetProperty(u => u.Username, userDTO.Username)
                    .SetProperty(u => u.Password, passwordHash)
                );

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var user = await _context.Users.FindAsync(id) ?? throw new ApiException("User not found.");

            user.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
