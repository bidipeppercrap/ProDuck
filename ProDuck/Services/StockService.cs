using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ProDuck.Models;

namespace ProDuck.Services
{
    public static class StockService
    {
        public async static Task ModifyStock(long productId, int qty, ProDuckContext context)
        {
            var product = await context.Products.FindAsync(productId) ?? throw new Exception("Invalid product");

            try
            {
                var productStock = await context.StockLocation
                .Where(_ => _.ProductId == productId)
                .OrderByDescending(_ => _.Stock)
                .FirstOrDefaultAsync();

                if (productStock == null)
                {
                    var stock = new StockLocation { LocationId = null, ProductId = productId, Stock = qty };

                    context.StockLocation.Add(stock);
                    await context.SaveChangesAsync();

                    return;
                }

                productStock.Stock += qty;

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
