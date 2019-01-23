using Graft.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Data;
using PaymentGateway.Models;
using PaymentGateway.Models.PaymentViewModels;
using PaymentGateway.Services;
using ReflectionIT.Mvc.Paging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Controllers
{
    [Authorize]
    public class PaymentsController : Controller
    {
        readonly ApplicationDbContext _db;
        readonly IUserService _userService;
        readonly UserManager<ApplicationUser> _userManager;

        public PaymentsController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IUserService userService)
        {
            _db = context;
            _userManager = userManager;
            _userService = userService;
        }

        public async Task<IActionResult> Index(string filter,
           PaymentStatus? status, 
           int? terminalId, int? storeId, int? merchantId, int? providerId,
           DateTime? fromDate, DateTime? toDate,
           int page = 1, string sortExpression = "-TransactionDate")
        {
            var query = _db.Payment
                .AsNoTracking()
                .Include(p => p.Store).ThenInclude(p => p.Merchant)
                .Include(p => p.Terminal)
                .OrderByDescending(p => p.TransactionDate)
                .Select(t => new PaymentViewModel
                {
                    Id = t.Id,
                    TerminalId = t.TerminalId,
                    StoreId = t.StoreId,
                    ServiceProviderId = t.ServiceProviderId,
                    TransactionDate = t.TransactionDate,
                    Status = t.Status,
                    SaleAmount = t.SaleAmount,
                    SaleCurrency = t.SaleCurrency,
                    PayToSaleRate = t.PayToSaleRate,
                    GraftToSaleRate = t.GraftToSaleRate,
                    PayCurrency = t.PayCurrency,
                    PayAmount = t.PayAmount,
                    PayWalletAddress = StringExtensions.EllipsisString(t.PayWalletAddress, 15, 0),
                    ServiceProviderFee = t.ServiceProviderFee,
                    ExchangeBrokerFee = t.ExchangeBrokerFee,
                    MerchantAmount = t.MerchantAmount,
                    SaleDetails = t.SaleDetails,
                    ServiceProvider = t.ServiceProvider,
                    Store = t.Store,
                    Terminal = t.Terminal,
                    TerminalName = t.Terminal.Name,
                    MerchantName = t.Store.Merchant.Name
                })
                .AsQueryable();

            query = await PerUserFilter(query);

            if (!string.IsNullOrWhiteSpace(filter))
                query = query.Where(p => p.PayWalletAddress.Contains(filter) ||
                                p.Store.Merchant.Name.Contains(filter) ||
                                p.Terminal.SerialNumber.Contains(filter) ||
                                p.Terminal.Name.Contains(filter));

            if (status != null)
                query = query.Where(p => p.Status == status);

            if (terminalId != null)
                query = query.Where(p => p.TerminalId == terminalId);

            if (storeId != null)
                query = query.Where(p => p.StoreId == storeId);

            if (merchantId != null)
                query = query.Where(p => p.Store.MerchantId == merchantId);

            if (providerId != null)
                query = query.Where(p => p.ServiceProviderId == providerId);

            if (fromDate != null)
                query = query.Where(p => p.TransactionDate >= fromDate);

            if (toDate != null)
                query = query.Where(p => p.TransactionDate <= toDate);

            var model = await PagingList.CreateAsync(query, AppConstant.PageSize, page, sortExpression, "-TransactionDate");

            model.RouteValue = new RouteValueDictionary
            {
                { "filter", filter},
                { "status", status },
                { "from", fromDate },
                { "to", toDate },
                { "terminalId", terminalId },
                { "storeId", storeId },
                { "merchantId", merchantId },
                { "providerId", providerId },
            };

            ViewData["terminalId"] = new SelectList(await PerUserFilter(_db.Terminal), "Id", "Name", null);
            ViewData["storeId"] = new SelectList(await PerUserFilter(_db.Store), "Id", "Name", null);
            ViewData["merchantId"] = new SelectList(await PerUserFilter(_db.Merchant), "Id", "Name", null);
            ViewData["providerId"] = new SelectList(_db.ServiceProvider, "Id", "Name", null);

            return View(model);
        }

        public async Task<IActionResult> Details(string id)
        {
            IQueryable<Payment> query = _db.Payment
                .Include(p => p.Store).ThenInclude(p => p.Merchant)
                .Include(p => p.Terminal)
                .Include(p => p.ServiceProvider);

            query = await PerUserFilter(query);

            var payment = await query.FirstOrDefaultAsync(m => m.Id == id);

            if (payment == null)
                return NotFound();

            return View(new PaymentViewModel
            {
                Id = payment.Id,
                TerminalId = payment.TerminalId,
                StoreId = payment.StoreId,
                ServiceProviderId = payment.ServiceProviderId,
                TransactionDate = payment.TransactionDate,
                Status = payment.Status,
                SaleAmount = payment.SaleAmount,
                SaleCurrency = payment.SaleCurrency,
                PayToSaleRate = payment.PayToSaleRate,
                GraftToSaleRate = payment.GraftToSaleRate,
                PayCurrency = payment.PayCurrency,
                PayAmount = payment.PayAmount,
                PayWalletAddress = payment.PayWalletAddress,
                ServiceProviderFee = payment.ServiceProviderFee,
                ExchangeBrokerFee = payment.ExchangeBrokerFee,
                MerchantAmount = payment.MerchantAmount,
                SaleDetails = payment.SaleDetails,
                ServiceProvider = payment.ServiceProvider,
                Store = payment.Store,
                Terminal = payment.Terminal,
                MerchantName = payment.Store.Merchant.Name
            });
        }

        bool PaymentExists(string id)
        {
            return _db.Payment.Any(e => e.Id == id);
        }

        async Task<IQueryable<Payment>> PerUserFilter(IQueryable<Payment> query)
        {
            if (User.IsInRole("ServiceProvider"))
            {
                var spId = await _userService.GetCurrentServiceProviderId(User);
                query = query.Where(p => p.ServiceProviderId == spId);
            }
            else if (User.IsInRole("Merchant"))
            {
                var MerchantId = await _userService.GetCurrentMerchantId(User);
                query = query.Where(p => p.Store.MerchantId == MerchantId);
            }
            else if (!User.IsInRole("Admin"))
            {
                throw new ApplicationException("Wrong user's role");
            }
            return query;
        }

        async Task<IQueryable<PaymentViewModel>> PerUserFilter(IQueryable<PaymentViewModel> query)
        {
            if (User.IsInRole("ServiceProvider"))
            {
                var spId = await _userService.GetCurrentServiceProviderId(User);
                query = query.Where(p => p.ServiceProviderId == spId);
            }
            else if (User.IsInRole("Merchant"))
            {
                var MerchantId = await _userService.GetCurrentMerchantId(User);
                query = query.Where(p => p.Store.MerchantId == MerchantId);
            }
            else if (!User.IsInRole("Admin"))
            {
                throw new ApplicationException("Wrong user's role");
            }
            return query;
        }

        async Task<IQueryable<Terminal>> PerUserFilter(IQueryable<Terminal> query)
        {
            if (User.IsInRole("ServiceProvider"))
            {
                var spId = await _userService.GetCurrentServiceProviderId(User);
                query = query.Where(p => p.ServiceProviderId == spId);
            }
            else if (User.IsInRole("Merchant"))
            {
                var MerchantId = await _userService.GetCurrentMerchantId(User);
                query = query.Where(p => p.Store.MerchantId == MerchantId);
            }
            else if (!User.IsInRole("Admin"))
            {
                throw new ApplicationException("Wrong user's role");
            }
            return query;
        }

        async Task<IQueryable<Store>> PerUserFilter(IQueryable<Store> query)
        {
            if (User.IsInRole("Merchant"))
            {
                var MerchantId = await _userService.GetCurrentMerchantId(User);
                query = query.Where(p => p.MerchantId == MerchantId);
            }
            return query;
        }

        async Task<IQueryable<Merchant>> PerUserFilter(IQueryable<Merchant> query)
        {
            if (User.IsInRole("Merchant"))
            {
                var MerchantId = await _userService.GetCurrentMerchantId(User);
                query = query.Where(p => p.Id == MerchantId);
            }
            return query;
        }

    }
}
