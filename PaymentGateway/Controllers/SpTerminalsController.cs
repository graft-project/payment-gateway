using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Data;
using PaymentGateway.Models;
using PaymentGateway.Models.TerminalViewModels;
using PaymentGateway.Services;
using ReflectionIT.Mvc.Paging;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Controllers
{
    [Authorize(Roles = "ServiceProvider")]
    public class SpTerminalsController : Controller
    {
        readonly ApplicationDbContext _db;
        readonly IUserService _userService;
        readonly UserManager<ApplicationUser> _userManager;

        public SpTerminalsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IUserService userService)
        {
            _db = context;
            _userManager = userManager;
            _userService = userService;
        }

        public async Task<IActionResult> Index(string filter,
           TerminalStatus? status, int? merchantId,
           int page = 1, string sortExpression = "SerialNumber")
        {
            var spId = await _userService.GetCurrentServiceProviderId(User);

            var query = _db.Terminal
                .AsNoTracking()
                .Include(t => t.Merchant)
                .Include(t => t.Store)
                .Where(t => t.ServiceProviderId == spId)
                .OrderBy(t => t.SerialNumber)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter))
                query = query.Where(p => p.SerialNumber.Contains(filter) ||
                                    p.Name.Contains(filter) ||
                                    p.Store.Merchant.Name.Contains(filter));

            if (status != null)
                query = query.Where(p => p.Status == status);

            if (merchantId != null)
                query = query.Where(p => p.Store.MerchantId == merchantId);


            var model = await PagingList.CreateAsync(query, AppConstant.PageSize, page, sortExpression, "SerialNumber");

            model.RouteValue = new RouteValueDictionary
            {
                { "filter", filter},
                { "status", status },
                { "merchantId", merchantId },
            };

            ViewData["merchantId"] = new SelectList(_db.Merchant, "Id", "Name", null);

            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var spId = await _userService.GetCurrentServiceProviderId(User);

            var terminal = await _db.Terminal
                .Include(t => t.Store)
                .FirstOrDefaultAsync(t => t.Id == id && t.ServiceProviderId == spId);

            if (terminal == null)
                return NotFound();

            return View(terminal);
        }

        public IActionResult Create()
        {
            ViewData["MerchantId"] = new SelectList(_db.Merchant, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SpTerminalViewModel model)
        {
            if (ModelState.IsValid)
            {
                var terminal = new Terminal
                {
                    ServiceProviderId = await _userService.GetCurrentServiceProviderId(User),
                    MerchantId = model.MerchantId,
                    StoreId = await _userService.GetDefaultStoreId(model.MerchantId),
                    SerialNumber = model.SerialNumber,
                    Name = model.Name,
                    Status = model.Status
                };

                _db.Add(terminal);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MerchantId"] = new SelectList(_db.Merchant, "Id", "Name", model.MerchantId);
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var terminal = await _db.Terminal.FindAsync(id);
            if (terminal == null)
                return NotFound();

            var model = new SpTerminalViewModel
            {
                MerchantId = terminal.MerchantId,
                SerialNumber = terminal.SerialNumber,
                Name = terminal.Name,
                Status = terminal.Status
            };

            ViewData["MerchantId"] = new SelectList(_db.Merchant, "Id", "Name", model.MerchantId);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, SpTerminalViewModel model)
        {
            if (id <= 0 || id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var terminal = new Terminal
                    {
                        Id = model.Id,
                        ServiceProviderId = await _userService.GetCurrentServiceProviderId(User),
                        MerchantId = model.MerchantId,
                        SerialNumber = model.SerialNumber,
                        Name = model.Name,
                        Status = model.Status
                    };

                    //todo if (merchantHasChanged)
                    terminal.StoreId = await _userService.GetDefaultStoreId(model.MerchantId);

                    _db.Update(terminal);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TerminalExists(model.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["MerchantId"] = new SelectList(_db.Merchant, "Id", "Name", model.MerchantId);
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var terminal = await _db.Terminal
                .Include(t => t.ServiceProvider)
                .Include(t => t.Store)
                .Include(t => t.Merchant)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (terminal == null)
                return NotFound();

            var model = new DeleteTerminalViewModel
            {
                Id = terminal.Id,
                Name = terminal.Name,
                StoreName = terminal.Store.Name,
                MerchantName = terminal.Merchant.Name,
                ServiceProviderName = terminal.ServiceProvider.Name,
                SerialNumber = terminal.SerialNumber,
                Status = terminal.Status,

                PaymentCount = await _db.Payment.CountAsync(t => t.TerminalId == terminal.Id)
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var terminal = await _db.Terminal.FindAsync(id);
            _db.Terminal.Remove(terminal);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool TerminalExists(int id)
        {
            return _db.Terminal.Any(e => e.Id == id);
        }

    }
}
