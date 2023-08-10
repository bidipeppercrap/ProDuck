using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using ProDuck.DTO;
using ProDuck.Migrations;
using ProDuck.Models;
using ProDuck.QueryParams;

namespace ProDuck.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly ProDuckContext _context;

        public LocationsController(ProDuckContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LocationDTO>> GetLocation(long id)
        {
            var location = await _context.Locations.FindAsync(id);

            if (location == null)
            {
                return NotFound();
            }

            if (location.LocationId != null)
            {
                var parentLocation = await _context.Locations.FindAsync(location.LocationId);
                var locationDTO = LocationToDTO(location);
                locationDTO.ParentLocation = LocationToDTO(parentLocation!);

                return locationDTO;
            }

            return LocationToDTO(location);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateLocation(long id, [FromBody] LocationDTO dto)
        {
            var location = await _context.Locations.FindAsync(id);

            if (location == null) return NotFound();
            if (!await ValidateLocation(dto)) return BadRequest();

            await _context.Locations
                .Where(l => l.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(l => l.LocationId, dto.LocationId)
                    .SetProperty(l => l.Name, dto.Name));

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteLocation(long id)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var location = await _context.Locations.FindAsync(id);

                if (location == null) return NotFound();

                _context.Locations
                    .Where(l => l.ParentLocation == location)
                    .ExecuteUpdate(s => s
                        .SetProperty(l => l.LocationId, location.LocationId));

                var stocks = await _context.StockLocation
                    .Where(s => s.Location == location)
                    .ToListAsync();

                foreach (var stock in stocks)
                {
                    var duplicateProductStock = await _context.StockLocation
                        .Where(s => s.LocationId == location.LocationId)
                        .Where(s => s.ProductId == stock.ProductId)
                        .FirstOrDefaultAsync();

                    if (duplicateProductStock != null)
                    {
                        var newStock = new StockLocation()
                        {
                            LocationId = location.LocationId,
                            ProductId = stock.ProductId,
                            Stock = stock.Stock + duplicateProductStock.Stock
                        };

                        _context.StockLocation.Remove(stock);
                        _context.StockLocation.Remove(duplicateProductStock);
                        _context.StockLocation.Add(newStock);

                        continue;
                    }

                    _context.StockLocation
                        .Where(s => s.Equals(stock))
                        .ExecuteUpdate(s => s
                            .SetProperty(x => x.LocationId, location.LocationId));
                }

                await _context.SaveChangesAsync();

                await _context.Locations
                    .Where(l => l.Id == id)
                    .ExecuteDeleteAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                throw;
            }

            return NoContent();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocationDTO>>> FetchLocations([FromQuery] long? exclude, [FromQuery] PaginationParams qp)
        {
            var whereQuery = _context.Locations;
                
            if (exclude != null)
            {
                return await _context.Locations
                    .Where(x => x.Id != exclude)
                    .Select(x => LocationToDTO(x))
                    .Skip((qp.Page - 1) * qp.PageSize)
                    .Take(qp.PageSize)
                    .ToListAsync();
            }

            return await whereQuery
                .Select(x => LocationToDTO(x))
                .Skip((qp.Page - 1) * qp.PageSize)
                .Take(qp.PageSize)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<LocationDTO>> PostLocation(LocationDTO locationDTO)
        {
            var location = new Location
            {
                Name = locationDTO.Name,
                LocationId = locationDTO.LocationId,
            };

            if (!await ValidateLocation(locationDTO)) return BadRequest();

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, LocationToDTO(location));
        }

        private async Task<bool> ValidateLocation(LocationDTO locationDTO)
        {
            if (locationDTO.LocationId == null) return true;
            if (locationDTO.LocationId == locationDTO.Id) return false;

            if (locationDTO.LocationId.HasValue)
            {
                var parentLocation = await _context.Locations.FindAsync(locationDTO.LocationId);

                if (parentLocation == null) return false;
            }

            return true;
        }

        private static LocationDTO LocationToDTO(Location location) =>
            new()
            {
                Id = location.Id,
                Name = location.Name,
                LocationId = location.LocationId,
            };
    }
}
