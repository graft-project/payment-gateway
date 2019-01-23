using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Data;
using PaymentGateway.Models;
using PaymentGateway.Models.MerchantViewModels;
using PaymentGateway.Models.UserViewModels;
using PaymentGateway.Services;
using ReflectionIT.Mvc.Paging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PaymentGateway.Controllers
{
    [Authorize(Roles = "Admin, ServiceProvider")]
    public class MerchantsController : Controller
    {
        readonly ApplicationDbContext _db;
        readonly UserManager<ApplicationUser> _userManager;
        readonly IUserService _userService;

        public MerchantsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IUserService userService)
        {
            _db = context;
            _userManager = userManager;
            _userService = userService;
        }

        public async Task<IActionResult> Index(string filter, MerchantStatus? status,
           int page = 1, string sortExpression = nameof(MerchantViewModel.Name), int? tagid = null)
        {
            var preQuery = _db.Merchant
                .AsNoTracking()
                .Include(t => t.User)
                .Include(t => t.Tags)
                .AsQueryable();

            if (tagid != null)
            {
                preQuery = preQuery.Where(x => x.Tags.Any(y => y.TagId == tagid.Value));
            }

            var query = preQuery.Select(t => new MerchantViewModel
            {
                Id = t.Id,
                UserId = t.UserId,
                Name = t.Name,
                Email = t.User.Email,
                Address = t.Address,
                WalletAddress = t.WalletAddress,
                Status = t.Status,
                EmailConfirmed = t.User.EmailConfirmed
            });


            if (!string.IsNullOrWhiteSpace(filter))
                query = query.Where(p => p.Name.Contains(filter) ||
                                    p.Email.Contains(filter) ||
                                    p.Address.Contains(filter) ||
                                    p.WalletAddress.Contains(filter));

            if (status != null)
                query = query.Where(p => p.Status == status);

            var model = await PagingList.CreateAsync(query, AppConstant.PageSize, page, 
                sortExpression, nameof(MerchantViewModel.Name));

            model.RouteValue = new RouteValueDictionary
            {
                { "filter", filter},
                { "status", status },
                { "tagid", tagid }
            };

            ViewBag.Tags = await _db.TagMerchant.Include(x=>x.User).Where(x=>x.User.Id == GetUserId()).ToArrayAsync();

            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var merchant = await _db.Merchant
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (merchant == null)
                return NotFound();

            return View(merchant);
        }

        [HttpGet]
        public IActionResult Invite()
        {
            var model = new InviteUserViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Invite(InviteUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.Invite(model.Email, "Merchant");
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var item in result.Errors)
                        ModelState.AddModelError(string.Empty, item.Description);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var merchant = await _db.Merchant
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (merchant == null)
                return NotFound();

            var model = new MerchantViewModel
            {
                Id = merchant.Id,
                UserId = merchant.UserId,
                Name = merchant.Name,
                Email = merchant.User.Email,
                Address = merchant.Address,
                WalletAddress = merchant.WalletAddress,
                Status = merchant.Status
            };

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id)
        {
            var merchant = await _db.Merchant.SingleOrDefaultAsync(s => s.Id == id);
            if (merchant == null)
                return NotFound();

            var user = await _db.Users.SingleOrDefaultAsync(t => t.Id == merchant.UserId);
            if (user == null)
                return NotFound();

            try
            {
                // update Users table
                if (await TryUpdateModelAsync(user, "",
                    s => s.Email))
                {
                    await _db.SaveChangesAsync();
                }

                // update Merchants table
                if (await TryUpdateModelAsync(merchant, "",
                    s => s.Name, s => s.Address, s => s.WalletAddress, 
                    s => s.Status))
                {
                    await _db.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return RedirectToAction("Error");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var merchant = await _db.Merchant
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (merchant == null)
                return NotFound();

            var model = new DeleteMerchantViewModel
            {
                Id = merchant.Id,
                Name = merchant.Name,
                Address = merchant.Address,
                WalletAddress = merchant.WalletAddress,
                Status = merchant.Status,

                StoresCount = await _db.Store.CountAsync(t => t.MerchantId == merchant.Id),
                TerminalCount = await _db.Terminal.CountAsync(t => t.MerchantId == merchant.Id),
                PaymentCount = await _db.Payment.Include(t => t.Store).CountAsync(t => t.Store.MerchantId == merchant.Id)
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var merchant = await _db.Merchant.FindAsync(id);
            _db.Merchant.Remove(merchant);

            var user = await _userManager.FindByIdAsync(merchant.UserId);
            await _userManager.DeleteAsync(user);

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ResendInvite(int? id)
        {
            var merchant = await _db.Merchant
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (merchant == null)
                return NotFound();

            var model = new ResendInviteViewModel()
            {
                UserID = merchant.User.Id,
                Email = merchant.User.Email
            };

            return View(model);
        }

        [HttpPost, ActionName("ResendInvite")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendInviteConfirmed(int id, string userId)
        {
            if (ModelState.IsValid)
            {
                if (!await _userService.ResendInvite(userId))
                    return NotFound();
                return RedirectToAction("Index");
            }
            return View();
        }

        bool MerchantExists(int id)
        {
            return _db.Merchant.Any(e => e.Id == id);
        }

        private string GetUserId()
        {
            return HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
    }
}
