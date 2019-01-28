using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Data;
using PaymentGateway.Models.Tags;
using PaymentGateway.Models.TagTerminalViewModels;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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

        public async Task<IActionResult> Index()
        {
            return View(await _context.TagTerminals.Where(x=>x.User.Id == GetUserId()).ToListAsync());
        }

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

        public IActionResult Create()
        {
            return View();
        }

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

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, [Bind("Id,Name,Description")] TagTerminalViewModel model)
        {
            var tagTerminal = await _context.TagTerminals
                .SingleOrDefaultAsync(s => s.Id == id && s.User.Id == GetUserId());

            if (tagTerminal == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    tagTerminal.Name = model.Name;
                    tagTerminal.Description = model.Description;

                    _context.Update(tagTerminal);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(tagTerminal);
        }

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
