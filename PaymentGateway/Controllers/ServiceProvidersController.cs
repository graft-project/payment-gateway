using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Data;
using PaymentGateway.Models.ServiceProviderViewModels;
using PaymentGateway.Models.UserViewModels;
using PaymentGateway.Services;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ServiceProvidersController : Controller
    {
        readonly ApplicationDbContext _context;
        readonly UserManager<ApplicationUser> _userManager;
        readonly IUserService _userService;

        public ServiceProvidersController(
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
            var model = await _context.ServiceProvider
                .Include(t => t.User)
                .Select(t => new ServiceProviderViewModel
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    UserName = t.User.UserName,
                    Email = t.User.Email,
                    Address = t.Address,
                    WalletAddress = t.WalletAddress,
                    TransactionFee = t.TransactionFee,
                    Status = t.Status,
                    EmailConfirmed = t.User.EmailConfirmed
                })
                .ToListAsync();

            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var serviceProvider = await _context.ServiceProvider
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (serviceProvider == null)
                return NotFound();

            return View(serviceProvider);
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
                var result = await _userService.Invite(model.Email, "ServiceProvider");
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

            var sp = await _context.ServiceProvider
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sp == null)
                return NotFound();

            var model = new ServiceProviderViewModel
            {
                Id = sp.Id,
                UserId = sp.UserId,
                UserName = sp.User.UserName,
                Email = sp.User.Email,
                Address = sp.Address,
                WalletAddress = sp.WalletAddress,
                TransactionFee = sp.TransactionFee,
                Status = sp.Status
            };

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id)
        {
            var sp = await _context.ServiceProvider.SingleOrDefaultAsync(s => s.Id == id);
            if (sp == null)
                return NotFound();

            var user = await _context.Users.SingleOrDefaultAsync(t => t.Id == sp.UserId);
            if (user == null)
                return NotFound();

            try
            {
                // update Users table
                if (await TryUpdateModelAsync(user, "",
                    s => s.UserName, s => s.Email))
                {
                    await _context.SaveChangesAsync();
                }

                // update ServiceProvider table
                if (await TryUpdateModelAsync(sp, "",
                    s => s.Address, s => s.WalletAddress, s => s.TransactionFee,
                    s => s.Status))
                {
                    await _context.SaveChangesAsync();
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
            var sp = await _context.ServiceProvider
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sp == null)
                return NotFound();

            var model = new DeleteServiceProviderViewModel
            {
                Id = sp.Id,
                Name = sp.Name,
                Address = sp.Address,
                WalletAddress = sp.WalletAddress,
                TransactionFee = sp.TransactionFee,
                Status = sp.Status,

                TerminalCount = await _context.Terminal.CountAsync(t => t.ServiceProviderId == sp.Id),
                PaymentCount = await _context.Payment.CountAsync(t => t.ServiceProviderId == sp.Id)
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sp = await _context.ServiceProvider.FindAsync(id);
            _context.ServiceProvider.Remove(sp);

            var user = await _userManager.FindByIdAsync(sp.UserId);
            await _userManager.DeleteAsync(user);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ResendInvite(int? id)
        {
            var sp = await _context.ServiceProvider
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sp == null)
                return NotFound();

            var model = new ResendInviteViewModel()
            {
                UserID = sp.User.Id,
                Email = sp.User.Email
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

        private bool ServiceProviderExists(int id)
        {
            return _context.ServiceProvider.Any(e => e.Id == id);
        }
    }
}
