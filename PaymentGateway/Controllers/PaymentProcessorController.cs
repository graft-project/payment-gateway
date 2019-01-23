using Graft.Infrastructure.Broker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PaymentGateway.Data;
using PaymentGateway.Models;
using PaymentGateway.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PaymentGateway.Controllers
{
    public class PaymentProcessorController : Controller
    {
        readonly ILogger _logger;
        readonly ApplicationDbContext _db;
        readonly IMemoryCache _cache;
        readonly IExchangeBroker _broker;
        readonly IPaymentService _paymentService;

        public PaymentProcessorController(
            ILoggerFactory loggerFactory,
            ApplicationDbContext db,
            IExchangeBroker broker,
            IMemoryCache cache,
            IPaymentService paymentService)
        {
            _logger = loggerFactory.CreateLogger(nameof(PaymentProcessorController));
            _db = db;
            _cache = cache;
            _broker = broker;
            _paymentService = paymentService;
        }

        public async Task<IActionResult> PayCurrencySelect(string id)
        {
            var payment = await GetPayment(id);
            if (payment == null)
                return View("Error");

            return View(payment);
        }

        public async Task<IActionResult> Invoice(string id, string currency)
        {
            var payment = await GetPayment(id);
            if (payment == null)
                return View("Error");

            await _paymentService.OnlineSale(payment, currency);

            return View(payment);
        }

        public async Task<IActionResult> Cancel(string id)
        {
            var payment = await GetPayment(id);
            if (payment == null)
                return View("Error");

            return Redirect(payment.CancelUrl);
        }

        public async Task<IActionResult> Success(string id)
        {
            var payment = await GetPayment(id);
            if (payment == null)
                return View("Error");

            return Redirect(payment.CompleteUrl);
        }

        public async Task<IActionResult> PaymentReceived(string id)
        {
            var payment = await GetPayment(id);
            if (payment == null)
                    return View("Error");
            return View(payment);
        }

        async Task<Payment> GetPayment(string id)
        {
            _cache.TryGetValue(id, out Payment payment);
            if (payment == null)
                payment = await _db.Payment.FirstOrDefaultAsync(t => t.Id == id);
            return payment;
        }

        public async Task<IActionResult> GetStatus(string id)
        {
            var prms = new BrokerSaleStatusParams()
            {
                PaymentId = id
            };

            try
            {
                var brokerResult = await _broker.GetSaleStatus(prms);

                var payment = await GetPayment(id);
                if (payment?.CallbackUrl != null)
                {
                    using (var httpClient = new HttpClient())
                    {
                        await httpClient.GetAsync(new Uri(payment.CallbackUrl + $"&id={payment.Id}&order_id={payment.ExternalOrderId}"));
                    }
                }

                return Json((int)brokerResult.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(ex.Message);
            }
        }
       
    }
}