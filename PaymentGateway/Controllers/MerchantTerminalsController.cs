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
using System.Security.Claims;
using System.Threading.Tasks;

namespace PaymentGateway.Controllers
{
    [Authorize(Roles = "Merchant")]
    public class MerchantTerminalsController : Controller
    {
        readonly ApplicationDbContext _db;
        readonly IUserService _userService;
        readonly UserManager<ApplicationUser> _userManager;

        public MerchantTerminalsController(
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
           int page = 1, string sortExpression = nameof(Terminal.SerialNumber), int? tagid = null)
        {
            var merchantId = await _userService.GetCurrentMerchantId(User);

            var query = _db.Terminal
                .AsNoTracking()
                .Include(t => t.ServiceProvider)
                .Include(t => t.Store)
                .Where(t => t.MerchantId == merchantId && t.Virtual == false)
                .OrderBy(t => t.SerialNumber)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter))
                query = query.Where(p => p.SerialNumber.Contains(filter) ||
                                    p.Name.Contains(filter) ||
                                    p.Store.Merchant.Name.Contains(filter));
									
            if (status != null)
                query = query.Where(p => p.Status == status);

            if (providerId != null)
                query = query.Where(p => p.Store.MerchantId == providerId);

            if (tagid != null)
            {
                query = query.Where(x => x.Tags.Any(y => y.TagId == tagid.Value));
            }

            var model = await PagingList.CreateAsync(query, AppConstant.PageSize, page, 
                sortExpression, nameof(Terminal.SerialNumber));

            model.RouteValue = new RouteValueDictionary
            {
                { "filter", filter},
                { "status", status },
                { "providerId", providerId },
                { "tagid", tagid }
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

            return View(terminal);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var terminal = await _db.Terminal.FindAsync(id);
            if (terminal == null)
                return NotFound();

            var model = new MerchantTerminalViewModel
            {
                Name = terminal.Name,
                Status = terminal.Status,
                StoreId = terminal.StoreId
            };

            var merchantId = await _userService.GetCurrentMerchantId(User);
            ViewData["storeId"] = new SelectList(_db.Store.Where(t => t.MerchantId == merchantId), "Id", "Name", model.StoreId);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, MerchantTerminalViewModel model)
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

                    if (terminal.Status == TerminalStatus.DisabledByServiceProvider &&
                        terminal.Status != model.Status)
                        model.Status = TerminalStatus.DisabledByServiceProvider;

                    terminal.Name = model.Name;
                    terminal.Status = model.Status;
                    terminal.StoreId = model.StoreId;

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
            ViewData["storeId"] = new SelectList(_db.Store.Where(t => t.MerchantId == merchantId), "Id", "Name", model.StoreId);
            return View(model);
        }

        private bool TerminalExists(int id)
        {
            return _db.Terminal.Any(e => e.Id == id);
        }

        private string GetUserId()
        {
            return HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
    }
}
