using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Data;
using PaymentGateway.Models.TagMerchantViewModels;
using PaymentGateway.Models.Tags;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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

        public async Task<IActionResult> Index()
        {
            return View(await _context.TagMerchant.Where(x=>x.User.Id == GetUserId()).ToListAsync());
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

            var tagMerchant = await _context.TagMerchant
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tagMerchant == null)
            {
                return NotFound();
            }

            return View(tagMerchant);
        }

        public IActionResult Create()
        {
            return View();
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] TagMerchantViewModel model)
        {
            var tagMerchant = await _context.TagMerchant
                .SingleOrDefaultAsync(s => s.Id == id && s.User.Id == GetUserId());

            if (tagMerchant == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    tagMerchant.Name = model.Name;
                    tagMerchant.Description = model.Description;

                    _context.Update(tagMerchant);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(tagMerchant);
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

            var tagMerchant = await _context.TagMerchant
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tagMerchant == null)
            {
                return NotFound();
            }

            return View(tagMerchant);
        }

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
