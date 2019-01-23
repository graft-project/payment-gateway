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
    public class CurrenciesController : Controller
    {
        readonly ApplicationDbContext _db;

        public CurrenciesController(ApplicationDbContext context)
        {
            _db = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _db.Currency.ToListAsync());
        }

        public async Task<IActionResult> Details(string id)
        {
            var currency = await _db.Currency
                .FirstOrDefaultAsync(m => m.Code == id);

            if (currency == null)
                return NotFound();

            return View(currency);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Code,Name")] Currency currency)
        {
            if (ModelState.IsValid)
            {
                if (_db.Currency.Any(c => c.Code == currency.Code))
                {
                    ModelState.AddModelError(string.Empty, $"Currency with code {currency.Code} already exists");
                }
                else
                {
                    _db.Add(currency);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(currency);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            var currency = await _db.Currency.FindAsync(id);
            if (currency == null)
                return NotFound();
            return View(currency);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id, [Bind("Code,Name")] Currency currency)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(currency);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CurrencyExists(currency.Code))
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
            return View(currency);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var currency = await _db.Currency
                .FirstOrDefaultAsync(m => m.Code == id);

            if (currency == null)
                return NotFound();

            return View(currency);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var currency = await _db.Currency.FindAsync(id);
            _db.Currency.Remove(currency);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool CurrencyExists(string id)
        {
            return _db.Currency.Any(e => e.Code == id);
        }
    }
}
