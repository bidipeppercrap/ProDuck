using AutoWrapper.Wrappers;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProDuck.DTO;
using ProDuck.Models;
using ProDuck.Responses;

namespace ProDuck.Controllers
{
    [Route("/")]
    [ApiController]
    public class IndexController : ControllerBase
    {
        private readonly ProDuckContext _context;

        public IndexController(ProDuckContext context)
        {
            _context = context;
        }
        [HttpGet] public ActionResult<string> Index()
        {
            return "Welcome to ProDuck - bidipeppercrap";
        }

        private record DashboardPayload(int ReplenishmentCount, int ProductPriceCount, int CustomerPriceCount);

        [HttpGet("dashboard")]
        [Authorize]
        public async Task<PaginatedResponse> GetDashboardInformation()
        {
            var needReplenishmentCount = await _context.ProductCategories
                .Include(x => x.Products).ThenInclude(xx => xx.Stocks)
                .Where(x => x.MinQty >= x.Products.Sum(xx => xx.Stocks.Sum(s => s.Stock)))
                .CountAsync();

            var badCustomerPriceCount = await _context.Products
                .Include(x => x.CustomerPrices)
                .Where(x => x.Deleted == false)
                .Where(x => x.CustomerPrices.Any(xx => x.Cost > xx.Price))
                .CountAsync();

            var badProductPriceCount = await _context.Products
                .Where(x => x.Deleted == false)
                .Where(x => x.Price < x.Cost)
                .CountAsync();

            var payload = new DashboardPayload(needReplenishmentCount, badProductPriceCount, badCustomerPriceCount);

            return new PaginatedResponse(payload);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRootUser(UserCreateDTO userDTO)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var users = _context.Users
                .Count();

                if (users > 0) return Unauthorized("Root user already exitsts");

                var passwordHash = Argon2.Hash(userDTO.Password);

                var user = new User
                {
                    Name = userDTO.Name ?? null,
                    Username = userDTO.Username,
                    Password = passwordHash,
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var claim = new Claim
                {
                    Name = "root",
                };

                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();

                user.Claims.Add(claim);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) throw new ApiException(ex.InnerException.Message);
                throw new ApiException(ex.Message);
            }

            return NoContent();
        }
    }
}
