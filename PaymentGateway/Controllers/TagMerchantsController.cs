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
    [Authorize(Roles = "Admin, ServiceProvider")]
    public class TagMerchantsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TagMerchantsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TagMerchants
        public async Task<IActionResult> Index()
        {
            return View(await _context.TagMerchant.Where(x=>x.User.Id == GetUserId()).ToListAsync());
        }

        // GET: TagMerchants/Details/5
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

            var tagMerchant = await _context.TagMerchant
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tagMerchant == null)
            {
                return NotFound();
            }

            return View(tagMerchant);
        }

        // GET: TagMerchants/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TagMerchants/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] TagMerchant tagMerchant)
        {
            tagMerchant.User = _context.Find<ApplicationUser>(GetUserId());

            if (!string.IsNullOrEmpty(tagMerchant.Name))
            {
                _context.Add(tagMerchant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(tagMerchant);
        }

        // GET: TagMerchants/Edit/5
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

            var tagMerchant = await _context.TagMerchant.FindAsync(id);
            if (tagMerchant == null)
            {
                return NotFound();
            }
            return View(tagMerchant);
        }

        // POST: TagMerchants/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] TagMerchant tagMerchant)
        {
            if (id != tagMerchant.Id)
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
                    _context.Update(tagMerchant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TagMerchantExists(tagMerchant.Id))
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
            return View(tagMerchant);
        }

        // GET: TagMerchants/Delete/5
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

            var tagMerchant = await _context.TagMerchant
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tagMerchant == null)
            {
                return NotFound();
            }

            return View(tagMerchant);
        }

        // POST: TagMerchants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!CanEditThisConnection(id))
            {
                return NotFound();
            }

            var tagMerchant = await _context.TagMerchant.FindAsync(id);
            _context.TagMerchant.Remove(tagMerchant);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CanEditThisConnection(int id)
        {
            return _context
                .TagMerchant
                .Where(x => x.User.Id == GetUserId())
                .Any(x => x.Id == id);
        }

        private bool TagMerchantExists(int id)
        {
            return _context.TagMerchant.Any(e => e.Id == id);
        }

        private string GetUserId()
        {
            return HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
    }
}
