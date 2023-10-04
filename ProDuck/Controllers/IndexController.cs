using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    }
}
