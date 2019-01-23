using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Data;
using PaymentGateway.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Controllers
{
    [Authorize]
    public class CryptocurrenciesController : Controller
    {
        readonly ApplicationDbContext _db;

        public CryptocurrenciesController(ApplicationDbContext context)
        {
            _db = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _db.Cryptocurrency.ToListAsync());
        }

        public async Task<IActionResult> Details(string id)
        {
            var cryptocurrency = await _db.Cryptocurrency
                .FirstOrDefaultAsync(m => m.Code == id);

            if (cryptocurrency == null)
                return NotFound();

            return View(cryptocurrency);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Code,Name")] Cryptocurrency cryptocurrency)
        {
            if (ModelState.IsValid)
            {
                if (_db.Currency.Any(c => c.Code == cryptocurrency.Code))
                {
                    ModelState.AddModelError(string.Empty, $"Cryptocurrency with code {cryptocurrency.Code} already exists");
                }
                else
                {
                    _db.Add(cryptocurrency);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(cryptocurrency);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            var cryptocurrency = await _db.Cryptocurrency.FindAsync(id);
            if (cryptocurrency == null)
                return NotFound();
            return View(cryptocurrency);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id, [Bind("Code,Name")] Cryptocurrency cryptocurrency)
        {
            if (id != cryptocurrency.Code)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(cryptocurrency);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CryptocurrencyExists(cryptocurrency.Code))
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
            return View(cryptocurrency);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var cryptocurrency = await _db.Cryptocurrency
                .FirstOrDefaultAsync(m => m.Code == id);

            if (cryptocurrency == null)
                return NotFound();

            return View(cryptocurrency);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var cryptocurrency = await _db.Cryptocurrency.FindAsync(id);
            _db.Cryptocurrency.Remove(cryptocurrency);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool CryptocurrencyExists(string id)
        {
            return _db.Cryptocurrency.Any(e => e.Code == id);
        }
    }
}
