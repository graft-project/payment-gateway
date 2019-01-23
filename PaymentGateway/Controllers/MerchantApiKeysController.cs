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
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PaymentGateway.Controllers
{
    [Authorize(Roles = "Merchant")]
    public class MerchantApiKeysController : Controller
    {
        readonly ApplicationDbContext _db;
        readonly IUserService _userService;
        readonly UserManager<ApplicationUser> _userManager;

        public MerchantApiKeysController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IUserService userService)
        {
            _db = context;
            _userManager = userManager;
            _userService = userService;
        }

        public async Task<IActionResult> Index(string filter,
           TerminalStatus? status, int? providerId,
           int page = 1, string sortExpression = nameof(Terminal.SerialNumber))
        {
            var merchantId = await _userService.GetCurrentMerchantId(User);

            var query = _db.Terminal
                .AsNoTracking()
                .Include(t => t.ServiceProvider)
                .Include(t => t.Store)
                .Where(t => t.MerchantId == merchantId && t.Virtual == true)
                .AsQueryable()
                .Select(t => new ApiKeyViewModel()
                {
                    Id = t.Id,
                    SerialNumber = t.SerialNumber,
                    StoreId = t.StoreId,
                    StoreName = t.Store.Name,
                    MerchantId = t.MerchantId,
                    ServiceProviderId = t.ServiceProviderId,
                    ServiceProviderName = t.ServiceProvider.Name,
                    Name = t.Name,
                    Status = t.Status,
                    Virtual = t.Virtual
                });

            if (!string.IsNullOrWhiteSpace(filter))
                query = query.Where(p => p.SerialNumber.Contains(filter) ||
                                    p.Name.Contains(filter));
									
            if (status != null)
                query = query.Where(p => p.Status == status);

            if (providerId != null)
                query = query.Where(p => p.ServiceProviderId == providerId);

            var model = await PagingList.CreateAsync(query, AppConstant.PageSize, page, 
                sortExpression, nameof(Terminal.SerialNumber));

            model.RouteValue = new RouteValueDictionary
            {
                { "filter", filter},
                { "status", status },
                { "providerId", providerId }
            };

            ViewData["providerId"] = new SelectList(_db.ServiceProvider, "Id", "Name", null);
            ViewBag.Tags = await _db.TagTerminals.Include(x => x.User).Where(x => x.User.Id == GetUserId()).ToArrayAsync();

            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var merchantId = await _userService.GetCurrentMerchantId(User);

            var terminal = await _db.Terminal
                .Include(t => t.ServiceProvider)
                .Include(t => t.Store)
                .FirstOrDefaultAsync(t => t.Id == id && t.MerchantId == merchantId);

            if (terminal == null)
                return NotFound();

            var model = new ApiKeyViewModel()
            {
                Id = terminal.Id,
                SerialNumber = terminal.SerialNumber,
                ApiSecret = terminal.ApiSecret,
                StoreId = terminal.StoreId,
                StoreName = terminal.Store.Name,
                MerchantId = terminal.MerchantId,
                ServiceProviderId = terminal.ServiceProviderId,
                ServiceProviderName = terminal.ServiceProvider.Name,
                Name = terminal.Name,
                Status = terminal.Status,
                Virtual = terminal.Virtual
            };

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            var merchantId = await _userService.GetCurrentMerchantId(User);
            var model = new CreateApiKeyViewModel()
            {
            };
            ViewData["ServiceProviderId"] = new SelectList(_db.ServiceProvider, "Id", "Name", null);
            ViewData["storeId"] = new SelectList(_db.Store.Where(t => t.MerchantId == merchantId), "Id", "Name", model.StoreId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateApiKeyViewModel model)
        {
            if (ModelState.IsValid)
            {
                var merchantId = await _userService.GetCurrentMerchantId(User);

                var virtualTerminal = new Terminal
                {
                    SerialNumber = Guid.NewGuid().ToString(),
                    ApiSecret = GenerateApiSecret(),
                    Name = model.Name,
                    Status = model.Status,
                    MerchantId = merchantId,
                    StoreId = model.StoreId,
                    ServiceProviderId = model.ServiceProviderId,
                    Virtual = true
                };

                _db.Add(virtualTerminal);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var terminal = await _db.Terminal.FindAsync(id);
            if (terminal == null)
                return NotFound();

            var model = new ApiKeyViewModel
            {
                Name = terminal.Name,
                Status = terminal.Status,
                StoreId = terminal.StoreId,
                ServiceProviderId = terminal.ServiceProviderId
            };

            var merchantId = await _userService.GetCurrentMerchantId(User);
            ViewData["StoreId"] = new SelectList(_db.Store.Where(t => t.MerchantId == merchantId), "Id", "Name", model.StoreId);
            ViewData["ServiceProviderId"] = new SelectList(_db.ServiceProvider, "Id", "Name", model.ServiceProviderId);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, ApiKeyViewModel model)
        {
            if (id <= 0 || id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var terminal = await _db.Terminal.FindAsync(id);
                    if (terminal == null)
                        return NotFound();

                    terminal.Name = model.Name;
                    terminal.Status = model.Status;
                    terminal.StoreId = model.StoreId;
                    terminal.ServiceProviderId = model.ServiceProviderId;

                    _db.Update(terminal);
                    await _db.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TerminalExists(model.Id))
                        return NotFound();
                    throw;
                }
            }

            var merchantId = await _userService.GetCurrentMerchantId(User);
            ViewData["StoreId"] = new SelectList(_db.Store.Where(t => t.MerchantId == merchantId), "Id", "Name", model.StoreId);
            ViewData["ServiceProviderId"] = new SelectList(_db.ServiceProvider, "Id", "Name", model.ServiceProviderId);
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var merchantId = await _userService.GetCurrentMerchantId(User);

            var t = await _db.Terminal
                .Include(s => s.Store)
                .Include(s => s.ServiceProvider)
                .FirstOrDefaultAsync(s => s.Id == id && s.MerchantId == merchantId);

            if (t == null)
                return NotFound();

            var model = new DeleteApiKeyViewModel
            {
                Id = t.Id,
                SerialNumber = t.SerialNumber,
                Name = t.Name,
                Status = t.Status,
                StoreName = t.Store?.Name,
                ServiceProviderName = t.ServiceProvider?.Name,
                PaymentCount = await _db.Payment.CountAsync(x => x.TerminalId == t.Id)
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var merchantId = await _userService.GetCurrentMerchantId(User);

            var t = await _db.Terminal.SingleOrDefaultAsync(s => s.Id == id && s.MerchantId == merchantId);
            if (t == null)
                return NotFound();

            _db.Terminal.Remove(t);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TerminalExists(int id)
        {
            return _db.Terminal.Any(e => e.Id == id);
        }

        private string GetUserId()
        {
            return HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }

        private string GenerateApiSecret()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }
    }
}
