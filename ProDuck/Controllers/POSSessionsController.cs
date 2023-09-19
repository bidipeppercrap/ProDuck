﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProDuck.DTO;
using ProDuck.Models;
using ProDuck.QueryParams;
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
            var orders = new List<POSSessionDTOOrder>();
            foreach(var o in session.Orders)
            {
                orders.Add(new POSSessionDTOOrder
                {
                    Id = o.Id,
                    CreatedAt = o.CreatedAt,
                    TotalPrice = o.Items.Sum(x => x.Price * x.Qty),
                    TotalCost = o.Items.Sum(x => x.Cost * x.Qty)
                });
            }

            var dto = new POSSessionDTO
            {
                Id = session.Id,
                ClosingRemark = session.ClosingRemark,
                OpenedAt = session.OpenedAt,
                ClosedAt = session.ClosedAt,
                OpeningBalance = session.OpeningBalance,
                ClosingBalance = session.ClosingBalance,
                OrderCount = session.Orders.Count,
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
                },
                Orders = orders
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
        public async Task<ActionResult<IEnumerable<POSSessionDTO>>> Get([FromQuery] PaginationParams qp)
        {
            var sessions = await _context.POSSession
                .Skip((qp.Page - 1) * qp.PageSize)
                .Take(qp.PageSize)
                .OrderByDescending(x => x.OpenedAt)
                .ToListAsync();

            return Ok(sessions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<POSSessionDTO>> Get(long id)
        {
            var session = await _context.POSSession
                .Include(x => x.SessionOpener)
                .Include(x => x.POS)
                .Include(x => x.Orders)
                    .ThenInclude(o => o.Items)
                .Where(x => x.Id == id)
                .Select(x => SessionToDTO(x))
                .FirstOrDefaultAsync();

            if (session == null) return NotFound();

            return Ok(session);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] POSSessionDTO sessionDTO)
        {
            if (await IsPOSActive(sessionDTO.POSId)) return BadRequest("POS is already has an active session.");

            var session = new POSSession
            {
                OpenedAt = sessionDTO.OpenedAt,
                OpeningBalance = sessionDTO.OpeningBalance,
                UserId = sessionDTO.UserId,
                POSId = sessionDTO.POSId,
            };

            _context.POSSession.Add(session);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("close/{id}")]
        public async Task<IActionResult> CloseSession(long id, [FromBody] POSSessionDTO sessionDTO)
        {
            var session = await _context.POSSession.FindAsync(id);

            if (session == null) return NotFound();
            if (session.ClosedAt != null) return BadRequest("Session already closed.");

            session.ClosingBalance = sessionDTO.ClosingBalance;
            session.ClosingRemark = sessionDTO.ClosingRemark;
            session.ClosedAt = sessionDTO.ClosedAt ?? DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] POSSessionDTO sessionDTO)
        {
            var session = await _context.POSSession.FindAsync(id);

            if (session == null) return NotFound();

            session.OpeningBalance = sessionDTO.OpeningBalance;
            session.ClosingRemark = sessionDTO.ClosingRemark;
            session.ClosingBalance = sessionDTO.ClosingBalance;

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}