using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProDuck.DTO;
using ProDuck.Models;
using ProDuck.QueryParams;
using ProDuck.Responses;
using ProDuck.Types;
using System.Runtime.InteropServices;

namespace ProDuck.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class POSSessionsController : ControllerBase
    {
        private readonly ProDuckContext _context;

        public POSSessionsController(ProDuckContext context)
        {
            _context = context;
        }

        private static POSSessionDTO SessionToDTO(POSSession session)
        {
            var dto = new POSSessionDTO
            {
                Id = session.Id,
                ClosingRemark = session.ClosingRemark,
                OpenedAt = session.OpenedAt,
                ClosedAt = session.ClosedAt,
                OpeningBalance = session.OpeningBalance,
                ClosingBalance = session.ClosingBalance,
                OrderCount = session.Orders.Count,
                TotalSalesPrice = session.Orders.Sum(o => o.Items.Sum(i => i.Price * i.Qty)),
                TotalSalesCost = session.Orders.Sum(o => o.Items.Sum(i => i.Cost * i.Qty)),
                SessionOpener = new UserDTO
                {
                    Id = session.SessionOpener.Id,
                    Name = session.SessionOpener.Name,
                    Username = session.SessionOpener.Username
                },
                POS = new POSSessionDTOPOS
                {
                    Id = session.POS.Id,
                    Name = session.POS.Name,
                    Description = session.POS.Description
                }
            };

            return dto;
        }

        private async Task<bool> IsPOSActive(long posId)
        {
            var session = await _context.POSSession
                .Where(x => x.POSId == posId)
                .Where(x => x.ClosedAt == null)
                .FirstOrDefaultAsync();

            return session != null;
        }

        [HttpGet]
        public async Task<PaginatedResponse> Get([FromQuery] PaginationParams qp, [FromQuery] long? posId)
        {
            var whereQuery = _context.POSSession.AsQueryable();

            if (posId != null) whereQuery = whereQuery.Where(x => x.POSId == posId);

            var result = await whereQuery
                .OrderByDescending(x => x.OpenedAt)
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
            var session = await _context.POSSession
                .Include(x => x.SessionOpener)
                .Include(x => x.POS)
                .Include(x => x.Orders)
                    .ThenInclude(o => o.Items)
                .Where(x => x.Id == id)
                .Select(x => SessionToDTO(x))
                .FirstOrDefaultAsync();

            if (session == null) throw new ApiException("Invalid Session Id.");

            return new PaginatedResponse(session);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] POSSessionDTO sessionDTO)
        {
            if (await IsPOSActive(sessionDTO.POSId)) throw new ApiException("POS is already has an active session.");

            var session = new POSSession
            {
                OpenedAt = sessionDTO.OpenedAt,
                OpeningBalance = sessionDTO.OpeningBalance,
                UserId = sessionDTO.UserId,
                POSId = sessionDTO.POSId,
            };

            _context.POSSession.Add(session);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("close/{id}")]
        public async Task<IActionResult> CloseSession(long id, [FromBody] POSSessionDTO sessionDTO)
        {
            var session = await _context.POSSession.FindAsync(id);

            if (session == null) throw new ApiException("Session not found.");
            if (session.ClosedAt != null) throw new ApiException("Session already closed.");

            session.ClosingBalance = sessionDTO.ClosingBalance;
            session.ClosingRemark = sessionDTO.ClosingRemark;
            session.ClosedAt = sessionDTO.ClosedAt ?? DateTime.Now;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] POSSessionDTO sessionDTO)
        {
            var session = await _context.POSSession.FindAsync(id);

            if (session == null) throw new ApiException("Session not found.");

            session.OpeningBalance = sessionDTO.OpeningBalance;
            session.ClosingRemark = sessionDTO.ClosingRemark;
            session.ClosingBalance = sessionDTO.ClosingBalance;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
