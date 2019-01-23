using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Data;
using PaymentGateway.Models.Tags;

namespace PaymentGateway.Controllers
{
    [Authorize(Roles = "Admin, ServiceProvider, Merchant")]
    public class TagTerminalSelectorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TagTerminalSelectorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TagMerchantSelectors
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var terminal = await _context
                .Terminal
                .Include(x => x.Tags)
                .FirstOrDefaultAsync(x => x.Id == id.Value);

            if (terminal == null)
            {
                return NotFound();
            }

            var tags = _context.TagTerminals.Where(x => x.User.Id == GetUserId());

            var ids = terminal.Tags.Select(x => x.TagId);

            var data = tags.Select(x => new TagTerminalItem { TagName = x.Name, IsSelected = ids.Contains(x.Id), TagId = x.Id }).ToList();

            return View(new TagTerminalSelector { TerminalId = id.Value, TagTerminalItems = data });
        }

        [HttpPost]
        public async Task<IActionResult> Index(TagTerminalSelector data)
        {
            var terminal = await _context
                .Terminal
                .Include(x => x.Tags)
                .FirstOrDefaultAsync(x => x.Id == data.TerminalId);

            if (terminal == null)
            {
                return NotFound();
            }

            var tagIds = data.TagTerminalItems.Where(x => x.IsSelected).Select(x => x.TagId);

            var newTagsList = await _context
                .TagTerminals
                .Where(x => x.User.Id == GetUserId() && tagIds.Contains(x.Id))
                .Select(x => new TagTerminalConnection { TerminalId = data.TerminalId, TagId = x.Id, })
                .ToListAsync();

            var oldRecords = await _context.TagTerminalConnections.Where(x => x.TerminalId == data.TerminalId).ToArrayAsync();
            _context.TagTerminalConnections.RemoveRange(oldRecords);

            await _context.TagTerminalConnections.AddRangeAsync(newTagsList);


            await _context.SaveChangesAsync();

            return View(data);
        }

        private string GetUserId()
        {
            return HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
    }
}