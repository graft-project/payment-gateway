using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Data;
using PaymentGateway.Models.Tags;

namespace PaymentGateway.Controllers
{
    [Authorize(Roles = "Admin, ServiceProvider, Merchant")]
    public class TagTerminalsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TagTerminalsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TagTerminals
        public async Task<IActionResult> Index()
        {
            return View(await _context.TagTerminals.Where(x=>x.User.Id == GetUserId()).ToListAsync());
        }

        // GET: TagTerminals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (!CanEditThisConnection(id.Value))
            {
                return NotFound();
            }

            var tagTerminal = await _context.TagTerminals
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tagTerminal == null)
            {
                return NotFound();
            }

            return View(tagTerminal);
        }

        // GET: TagTerminals/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TagTerminals/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] TagTerminal tagTerminal)
        {
            tagTerminal.User = _context.Find<ApplicationUser>(GetUserId());

            if (!string.IsNullOrEmpty(tagTerminal.Name))
            {
                _context.Add(tagTerminal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tagTerminal);
        }

        // GET: TagTerminals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (!CanEditThisConnection(id.Value))
            {
                return NotFound();
            }

            var tagTerminal = await _context.TagTerminals.FindAsync(id);
            if (tagTerminal == null)
            {
                return NotFound();
            }
            return View(tagTerminal);
        }

        // POST: TagTerminals/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] TagTerminal tagTerminal)
        {
            if (id != tagTerminal.Id)
            {
                return NotFound();
            }

            if (!CanEditThisConnection(id))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tagTerminal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TagTerminalExists(tagTerminal.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tagTerminal);
        }

        // GET: TagTerminals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (!CanEditThisConnection(id.Value))
            {
                return NotFound();
            }

            var tagTerminal = await _context.TagTerminals
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tagTerminal == null)
            {
                return NotFound();
            }

            return View(tagTerminal);
        }

        // POST: TagTerminals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!CanEditThisConnection(id))
            {
                return NotFound();
            }

            var tagTerminal = await _context.TagTerminals.FindAsync(id);
            _context.TagTerminals.Remove(tagTerminal);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TagTerminalExists(int id)
        {
            return _context.TagTerminals.Any(e => e.Id == id);
        }

        private bool CanEditThisConnection(int id)
        {
            return _context
                .TagTerminals
                .Where(x => x.User.Id == GetUserId())
                .Any(x => x.Id == id);
        }

        private string GetUserId()
        {
            return HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
    }
}
