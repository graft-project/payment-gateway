using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Data;
using PaymentGateway.Models;
using PaymentGateway.Models.StoreViewModels;
using PaymentGateway.Services;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Controllers
{
    [Authorize(Roles = "Merchant")]
    public class StoresController : Controller
    {
        readonly ApplicationDbContext _context;
        readonly UserManager<ApplicationUser> _userManager;
        readonly IUserService _userService;

        public StoresController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IUserService userService)
        {
            _context = context;
            _userManager = userManager;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var MerchantId = await _userService.GetCurrentMerchantId(User);

            var query = _context.Store
                .Where(t => t.MerchantId == MerchantId)
                .Include(t => t.Merchant)
                .ThenInclude(t => t.User)
                .OrderBy(t => t.Name);

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            var MerchantId = await _userService.GetCurrentMerchantId(User);

            var store = await _context.Store
                .Where(t => t.MerchantId == MerchantId)
                .Include(t => t.Merchant)
                .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (store == null)
                return NotFound();

            return View(store);
        }

        public IActionResult Create()
        {
            var model = new CreateStoreViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Address,Active")] CreateStoreViewModel model)
        {
            if (ModelState.IsValid)
            {
                var MerchantId = await _userService.GetCurrentMerchantId(User);

                var store = new Store
                {
                    Name = model.Name,
                    Address = model.Address,
                    Status = model.Status,
                    MerchantId = MerchantId
                };

                _context.Add(store);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var store = await _context.Store.FindAsync(id);
            if (store == null)
                return NotFound();

            return View(store);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var MerchantId = await _userService.GetCurrentMerchantId(User);

                    var store = await _context.Store.SingleOrDefaultAsync(s => s.Id == id && s.MerchantId == MerchantId);
                    if (store == null)
                        return NotFound();

                    if (await TryUpdateModelAsync(store, "",
                        s => s.Name, s => s.Address, s => s.Status))
                    {
                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return RedirectToAction("Error");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var MerchantId = await _userService.GetCurrentMerchantId(User);

            var store = await _context.Store
                .Include(s => s.Merchant)
                .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id && s.MerchantId == MerchantId);

            if (store == null)
                return NotFound();

            var model = new DeleteStoreViewModel
            {
                Id = store.Id,
                Name = store.Name,
                MerchantName = store.Merchant.Name,
                Address = store.Address,
                Status = store.Status,

                TerminalCount = await _context.Terminal.CountAsync(t => t.StoreId == store.Id),
                PaymentCount = await _context.Payment.CountAsync(t => t.StoreId == store.Id)
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var MerchantId = await _userService.GetCurrentMerchantId(User);

            var store = await _context.Store.SingleOrDefaultAsync(s => s.Id == id && s.MerchantId == MerchantId);
            if (store == null)
                return NotFound();

            _context.Store.Remove(store);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool StoreExists(int id)
        {
            return _context.Store.Any(e => e.Id == id);
        }
    }
}
