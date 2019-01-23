using Graft.DAPI;
using Graft.Infrastructure;
using Graft.Infrastructure.Broker;
using Graft.Infrastructure.Gateway;
using Graft.Infrastructure.Rate;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentGateway.Data;
using PaymentGateway.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PaymentGateway.Services
{
    public class PosApiConfiguration
    {
        public int PaymentTimeout { get; set; }
    }

    public class PaymentService : IPaymentService
    {
        readonly PosApiConfiguration _settings;
        readonly ILogger _logger;
        readonly ApplicationDbContext _db;
        readonly IRateCache _rateCache;
        readonly IMemoryCache _cache;
        readonly IExchangeBroker _broker;
        readonly IHttpContextAccessor _context;
        readonly IGraftDapiService _dapi;

        public PaymentService(ILoggerFactory loggerFactory,
            IConfiguration configuration,
            ApplicationDbContext db,
            IRateCache rateCache,
            IMemoryCache cache,
            IExchangeBroker broker,
            IHttpContextAccessor context,
            IGraftDapiService dapi)
        {
            _settings = configuration
                .GetSection("PosApi")
                .Get<PosApiConfiguration>();

            _logger = loggerFactory.CreateLogger(nameof(PaymentService));
            _db = db;
            _rateCache = rateCache;
            _cache = cache;
            _broker = broker;
            _context = context;
            _dapi = dapi;
        }

        public GatewayOnlineSaleResult PrepareOnlineSale(GatewayOnlineSaleParams model, string timestamp, string sign)
        {
            _logger.LogInformation("API Prepare Online Sale: {@params}", model);

            var terminal = _db.Terminal
                .Where(t => t.SerialNumber == model.PosSn)
                .Include(t => t.ServiceProvider)
                .Include(t => t.Store).ThenInclude(t => t.Merchant)
                .FirstOrDefault();

            if (terminal == null)
                throw new ApiException(ErrorCode.InvalidApiKey);

            using (var hmac = new HMACSHA256(Encoding.ASCII.GetBytes(terminal.ApiSecret)))
            {
                string text = $"{timestamp}{terminal.SerialNumber}";
                var hash = hmac.ComputeHash(Encoding.ASCII.GetBytes(text));

                var signBytes = sign.HexStringToBytes();

                if (!hash.ByteArrayCompare(signBytes))
                    throw new ApiException(ErrorCode.InvalidApiKey);
            }

            var payment = new Payment()
            {
                Id = Guid.NewGuid().ToString(),
                TransactionDate = DateTime.UtcNow,
                Status = PaymentStatus.New,

                TerminalId = terminal.Id,
                StoreId = terminal.StoreId,
                ServiceProviderId = terminal.ServiceProviderId,

                SaleAmount = model.SaleAmount,
                SaleCurrency = model.SaleCurrency,

                ExternalOrderId = model.ExternalOrderId,
                CompleteUrl = model.CompleteUrl,
                CancelUrl = model.CancelUrl,
                CallbackUrl = model.CallbackUrl,
            };

            _cache.Set(payment.Id, payment, DateTimeOffset.Now.AddMinutes(_settings.PaymentTimeout));


            var req = _context.HttpContext.Request;
            var res = new GatewayOnlineSaleResult()
            {
                PaymentUrl = $"{req.Scheme}://{req.Host}/PaymentProcessor/PayCurrencySelect/{payment.Id}"
            };

            return res;
        }

        public async Task<GatewaySaleResult> OnlineSale(Payment payment, string currency)
        {
            _logger.LogInformation("API Online Sale: {@params}", payment);

            if (payment.Status == PaymentStatus.New)
            {
                var terminal = _db.Terminal
                    .Where(t => t.Id == payment.TerminalId)
                    .Include(t => t.ServiceProvider)
                    .Include(t => t.Store).ThenInclude(t => t.Merchant)
                    .FirstOrDefault();

                if (terminal == null)
                    throw new ApiException(ErrorCode.InvalidApiKey);

                if (currency == "GRFT")
                {
                    var graftRate = await _rateCache.GetRateToUsd("GRFT");
                    payment.PayAmount = payment.SaleAmount / graftRate;
                    payment.PayCurrency = currency;

                    var dapiParams = new DapiSaleParams
                    {
                        PaymentId = payment.Id,
                        SaleDetails = payment.SaleDetails,
                        Address = terminal.Merchant.WalletAddress,
                        Amount = GraftConvert.ToAtomicUnits(payment.PayAmount)
                    };

                    var dapiResult = await _dapi.Sale(dapiParams);

                    payment.Status = PaymentStatus.Waiting;
                    payment.BlockNumber = dapiResult.BlockNumber;
                    payment.PayWalletAddress = $"{payment.Id};{terminal.Merchant.WalletAddress};{payment.PayAmount:N8};{dapiResult.BlockNumber}";
                }
                else
                {
                    var brokerParams = new BrokerSaleParams
                    {
                        PaymentId = payment.Id,
                        SaleAmount = payment.SaleAmount,
                        SaleCurrency = payment.SaleCurrency,
                        PayCurrency = currency,

                        ServiceProviderFee = terminal.ServiceProvider.TransactionFee,
                        ServiceProviderWallet = terminal.ServiceProvider.WalletAddress,
                        MerchantWallet = terminal.Merchant.WalletAddress
                    };

                    var brokerResult = await _broker.Sale(brokerParams);

                    payment.PayToSaleRate = brokerResult.PayToSaleRate;
                    payment.GraftToSaleRate = brokerResult.GraftToSaleRate;

                    payment.PayCurrency = brokerResult.PayCurrency;
                    payment.PayAmount = brokerResult.PayAmount;
                    payment.PayWalletAddress = brokerResult.PayWalletAddress;

                    payment.ServiceProviderFee = brokerResult.ServiceProviderFee;
                    payment.ExchangeBrokerFee = brokerResult.ExchangeBrokerFee;
                    payment.MerchantAmount = brokerResult.MerchantAmount;
                }

                _db.Payment.Add(payment);
                await _db.SaveChangesAsync();
            }

            var res = new GatewaySaleResult()
            {
                PaymentId = payment.Id,
                WalletAddress = payment.PayWalletAddress,

                SaleAmount = payment.SaleAmount,
                SaleCurrency = payment.SaleCurrency,

                PayToSaleRate = payment.PayToSaleRate,
                GraftToSaleRate = payment.GraftToSaleRate,

                PayCurrency = payment.PayCurrency,
                PayAmount = payment.PayAmount,

                ServiceProviderFee = payment.ServiceProviderFee,
                ExchangeBrokerFee = payment.ExchangeBrokerFee,
                MerchantAmount = payment.MerchantAmount
            };
            return res;
        }

        public async Task<GatewayGetSaleStatusResult> GetSaleStatus(string id)
        {
            _logger.LogInformation("API Online Sale Status: {@params}", id);

            _cache.TryGetValue(id, out Payment payment);
            if (payment == null)
            {
                payment = await _db.Payment.FirstOrDefaultAsync(t => t.Id == id);
                if (payment == null)
                    throw new ApiException(ErrorCode.PaymentNotFoundOrExpired);
            }

            GatewayGetSaleStatusResult res = null;

            if (payment.PayCurrency == "GRFT")
            {
                var dapiParams = new DapiSaleStatusParams
                {
                    PaymentId = id,
                    BlockNumber = payment.BlockNumber
                };

                var dapiResult = await _dapi.GetSaleStatus(dapiParams);

                res = new GatewayGetSaleStatusResult()
                {
                    PaymentId = id,
                    Status = dapiResult.GetPaymentStatus()
                };
            }
            else
            {
                var brokerParams = new BrokerSaleStatusParams
                {
                    PaymentId = id
                };

                var brokerResult = await _broker.GetSaleStatus(brokerParams);

                res = new GatewayGetSaleStatusResult()
                {
                    PaymentId = brokerResult.PaymentId,
                    Status = brokerResult.Status
                };
            }

            _logger.LogInformation("API GetSaleStatus Result: {@params}", res);
            return res;
        }

        public async Task<GatewaySaleResult> Sale(GatewaySaleParams model)
        {
            _logger.LogInformation("API Sale: {@params}", model);

            if (model.SaleAmount <= 0) throw new ApiException(ErrorCode.InvalidAmount);
            if (string.IsNullOrWhiteSpace(model.SaleCurrency)) throw new ApiException(ErrorCode.SaleCurrencyEmpty);
            if (string.IsNullOrWhiteSpace(model.PayCurrency)) throw new ApiException(ErrorCode.PayCurrencyEmpty);


            var terminal = _db.Terminal
                .Where(t => t.SerialNumber == model.PosSn)
                .Include(t => t.ServiceProvider)
                .Include(t => t.Store).ThenInclude(t => t.Merchant)
                .FirstOrDefault();

            if (terminal == null) throw new ApiException(ErrorCode.TerminalNotFound);
            if (terminal.Store == null) throw new ApiException(ErrorCode.StoreNotFound);
            if (terminal.Store.Merchant == null) throw new ApiException(ErrorCode.MerchantNotFound);
            if (terminal.ServiceProvider == null) throw new ApiException(ErrorCode.ServiceProviderNotFound);

            if (terminal.Status == TerminalStatus.Disabled) throw new ApiException(ErrorCode.TerminalDisabled);
            if (terminal.Status == TerminalStatus.DisabledByServiceProvider) throw new ApiException(ErrorCode.TerminalDisabledByServiceProvider);

            if (terminal.Store.Status == StoreStatus.Disabled) throw new ApiException(ErrorCode.StoreDisabled);
            if (terminal.Merchant.Status == MerchantStatus.Disabled) throw new ApiException(ErrorCode.MerchantDisabled);
            if (terminal.ServiceProvider.Status == ServiceProviderStatus.Disabled) throw new ApiException(ErrorCode.ServiceProviderDisabled);

            Payment payment = null;

            if (model.PayCurrency == "GRFT")
            {
                payment = await GraftSale(model, terminal);
            }
            else
            {
                payment = await AltcoinSale(model, terminal);
            }

            _cache.Set(payment.Id, payment, DateTimeOffset.Now.AddMinutes(_settings.PaymentTimeout));

            _db.Payment.Add(payment);
            await _db.SaveChangesAsync();

            GatewaySaleResult res = GetSaleResult(payment);
            _logger.LogInformation("API Sale Result: {@params}", res);
            return res;
        }

        async Task<Payment> AltcoinSale(GatewaySaleParams model, Terminal terminal)
        {
            var brokerParams = new BrokerSaleParams
            {
                SaleAmount = model.SaleAmount,
                SaleCurrency = model.SaleCurrency,
                PayCurrency = model.PayCurrency,
                ServiceProviderFee = terminal.ServiceProvider.TransactionFee,
                ServiceProviderWallet = terminal.ServiceProvider.WalletAddress,
                MerchantWallet = terminal.Merchant.WalletAddress
            };

            var brokerResult = await _broker.Sale(brokerParams);

            var payment = new Payment()
            {
                Id = brokerResult.PaymentId,
                TransactionDate = DateTime.UtcNow,
                Status = PaymentStatus.New,

                TerminalId = terminal.Id,
                StoreId = terminal.StoreId,
                ServiceProviderId = terminal.ServiceProviderId,

                SaleAmount = brokerResult.SaleAmount,
                SaleCurrency = brokerResult.SaleCurrency,

                PayToSaleRate = brokerResult.PayToSaleRate,
                GraftToSaleRate = brokerResult.GraftToSaleRate,

                PayCurrency = brokerResult.PayCurrency,
                PayAmount = brokerResult.PayAmount,
                PayWalletAddress = brokerResult.PayWalletAddress,

                ServiceProviderFee = brokerResult.ServiceProviderFee,
                ExchangeBrokerFee = brokerResult.ExchangeBrokerFee,
                MerchantAmount = brokerResult.MerchantAmount,
                SaleDetails = model.SaleDetails,
            };

            return payment;
        }

        async Task<Payment> GraftSale(GatewaySaleParams model, Terminal terminal)
        {
            var graftRate = await _rateCache.GetRateToUsd("GRFT");
            var graftAmount = model.SaleAmount / graftRate;

            var payment = new Payment
            {
                Id = Guid.NewGuid().ToString(),
                TransactionDate = DateTime.UtcNow,
                Status = PaymentStatus.New,

                TerminalId = terminal.Id,
                StoreId = terminal.StoreId,
                ServiceProviderId = terminal.ServiceProviderId,

                SaleAmount = model.SaleAmount,
                SaleCurrency = model.SaleCurrency,

                PayToSaleRate = graftRate,
                GraftToSaleRate = graftRate,

                PayCurrency = model.PayCurrency,
                PayAmount = graftAmount,

                ServiceProviderFee = 0,
                ExchangeBrokerFee = 0,
                MerchantAmount = graftAmount,

                SaleDetails = model.SaleDetails
            };

            var dapiParams = new DapiSaleParams
            {
                PaymentId = payment.Id,
                SaleDetails = model.SaleDetails,
                Address = terminal.Merchant.WalletAddress,
                Amount = GraftConvert.ToAtomicUnits(payment.PayAmount)
            };

            var dapiResult = await _dapi.Sale(dapiParams);

            payment.Status = PaymentStatus.Waiting;
            payment.BlockNumber = dapiResult.BlockNumber;
            payment.PayWalletAddress = $"{payment.Id};{terminal.Merchant.WalletAddress};{payment.PayAmount:N8};{dapiResult.BlockNumber}";

            return payment;
        }

        GatewaySaleResult GetSaleResult(Payment payment)
        {
            return new GatewaySaleResult()
            {
                PaymentId = payment.Id,
                WalletAddress = payment.PayWalletAddress,

                SaleAmount = payment.SaleAmount,
                SaleCurrency = payment.SaleCurrency,

                PayToSaleRate = payment.PayToSaleRate,
                GraftToSaleRate = payment.GraftToSaleRate,

                PayCurrency = payment.PayCurrency,
                PayAmount = payment.PayAmount,

                ServiceProviderFee = payment.ServiceProviderFee,
                ExchangeBrokerFee = payment.ExchangeBrokerFee,
                MerchantAmount = payment.MerchantAmount
            };
        }
    }
}
