using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using ProDuck.DTO;
using ProDuck.Migrations;
using ProDuck.Models;
using ProDuck.QueryParams;
using ProDuck.Responses;
using ProDuck.Types;

namespace ProDuck.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class LocationsController : ControllerBase
    {
        private readonly ProDuckContext _context;

        public LocationsController(ProDuckContext context)
        {
            _context = context;
        }

        private static ValidationResult ValidateLocationDTO(LocationDTO dto)
        {
            var result = new ValidationResult();

            if (dto.Name.Length < 3) result.ErrorMessages.Add("Name must be longer than 2 characters.");
            if (dto.LocationId != null && dto.LocationId == dto.Id) result.ErrorMessages.Add("Parent Location cannot be thy self.");

            if (result.ErrorMessages.Count > 0) return result;

            result.IsValid = true;

            return result;
        }

        [HttpGet("{id}")]
        public async Task<PaginatedResponse> GetLocation(long id)
        {
            var location = await _context.Locations.FindAsync(id);

            if (location == null)
            {
                throw new ApiException("Location not found.");
            }

            if (location.LocationId != null)
            {
                var parentLocation = await _context.Locations.FindAsync(location.LocationId);
                var locationDTO = LocationToDTO(location);
                locationDTO.ParentLocation = LocationToDTO(parentLocation!);

                return new PaginatedResponse(locationDTO);
            }

            return new PaginatedResponse(LocationToDTO(location));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "root")]
        public async Task<IActionResult> UpdateLocation(long id, [FromBody] LocationDTO dto)
        {
            dto.Id = id;
            var validation = ValidateLocationDTO(dto);
            if (!validation.IsValid) throw new ApiException(validation.ErrorMessages.First());

            var location = await _context.Locations.FindAsync(id);

            if (location == null) throw new ApiException("Location not found.");
            if (!await ValidateLocation(dto)) throw new ApiException("Parent location not found");

            await _context.Locations
                .Where(l => l.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(l => l.LocationId, dto.LocationId)
                    .SetProperty(l => l.Name, dto.Name));

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "root")]
        public async Task<ActionResult> DeleteLocation(long id)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var location = await _context.Locations.FindAsync(id);

                if (location == null) throw new ApiException("Location not found.");

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
            catch (Exception ex)
            {
                if (ex.InnerException != null) throw new ApiException(ex.InnerException.Message);
                throw new ApiException(ex.Message);
            }

            return NoContent();
        }

        [HttpGet]
        public async Task<PaginatedResponse> FetchLocations([FromQuery] long? exclude, [FromQuery] long? parentId, [FromQuery] long? productIdToExclude, [FromQuery] PaginationParams qp, [FromQuery] bool showOnlyRootChilds = false, [FromQuery] string keyword = "")
        {
            var whereQuery = _context.Locations.AsQueryable();

            foreach (var word in keyword.Trim().Split(" ")) whereQuery = whereQuery.Where(x => x.Name.Contains(word));
            if (parentId != null && !showOnlyRootChilds) whereQuery = whereQuery.Where(x => x.LocationId == parentId);
            if (showOnlyRootChilds) whereQuery = whereQuery.Where(x => x.LocationId == null);
            if (exclude != null) whereQuery = whereQuery.Where(x => x.Id != exclude);
            if (productIdToExclude != null) whereQuery = whereQuery.Where(x => x.Products.All(s => s.ProductId != productIdToExclude));

            var result = await whereQuery
                .OrderByDescending(x => x.Id)
                .Select(x => LocationToDTO(x))
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
        [Authorize(Roles = "root")]
        public async Task<ActionResult<LocationDTO>> PostLocation(LocationDTO locationDTO)
        {
            var validation = ValidateLocationDTO(locationDTO);
            if (!validation.IsValid) throw new ApiException(validation.ErrorMessages.First());

            var location = new Location
            {
                Name = locationDTO.Name,
                LocationId = locationDTO.LocationId,
            };

            if (!await ValidateLocation(locationDTO)) throw new ApiException("Parent Location not found.");

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, LocationToDTO(location));
        }

        private async Task<bool> ValidateLocation(LocationDTO locationDTO)
        {
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
