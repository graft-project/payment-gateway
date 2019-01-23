using Graft.Infrastructure.Broker;
using Graft.Infrastructure.Rate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Data;
using System.Threading.Tasks;

namespace PaymentGateway.Models
{
    [Authorize]
    public class TerminalsDemoPurchaseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExchangeBroker _exchangeBroker;
        private readonly IRateCache _rateCache;

        public TerminalsDemoPurchaseController(
            ApplicationDbContext context, 
            IExchangeBroker exchangeBroker,
            IRateCache rateCache)
        {
            _context = context;
            _exchangeBroker = exchangeBroker;
            _rateCache = rateCache;
        }

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Terminal
                .Include(t => t.Merchant)
                .Include(t => t.ServiceProvider)
                .Include(t => t.Store);

            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> Pay(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var terminal = await _context.Terminal.FindAsync(id);
            if (terminal == null)
            {
                return NotFound();
            }

            ViewData["PayCurrency"] = new SelectList(_rateCache.GetSupportedCurrencies(), "CurrencyCode", "CurrencyName", "BTC");
            return View(TerminalPaymentDemo.FromTerminal(terminal));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int id, [Bind("Id,SaleAmount,PayCurrency")] TerminalPaymentDemo terminalPayment)
        {
            var terminal = await _context.Terminal.FindAsync(id);
            if (terminal == null)
            {
                return NotFound();
            }

            var merchant = await _context.Merchant.FindAsync(terminal.MerchantId);
            if (merchant == null)
            {
                return NotFound();
            }

            var serviceProvider = await _context.ServiceProvider.FindAsync(terminal.ServiceProviderId);
            if (serviceProvider == null)
            {
                return NotFound();
            }

            var result = await _exchangeBroker.Sale(
                new BrokerSaleParams()
                {
                    MerchantWallet = merchant.WalletAddress,
                    SaleAmount = terminalPayment.SaleAmount,
                    PayCurrency = terminalPayment.PayCurrency,
                    SaleCurrency = "USD",
                    ServiceProviderFee = serviceProvider.TransactionFee,
                    ServiceProviderWallet = serviceProvider.WalletAddress
                });

            if (result != null)
            {
                return RedirectToAction(nameof(CheckTransactionStatus), new CheckTransactionModel { PaymentId = result.PaymentId, Address = result.PayWalletAddress });
            }

            return View(terminal);
        }

        public Task<IActionResult> CheckTransactionStatus(CheckTransactionModel checkTransactionModel)
        {
            return Task.FromResult((IActionResult)View(checkTransactionModel));
        }

        public async Task<IActionResult> GetPaymentStatus(string paymentId)
        {
            var result = await _exchangeBroker.GetSaleStatus(new BrokerSaleStatusParams() { PaymentId = paymentId });

            return Json(result.Status.ToString());
        }
    }
}
