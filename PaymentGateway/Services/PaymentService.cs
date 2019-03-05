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
    public class PaymentsConfiguration
    {
        public int PaymentTimeout { get; set; }
        public string GraftWalletAddress { get; set; }
        public string GraftWalletUrl { get; set; }
        public string DapiUrl { get; set; }
    }

    public class PaymentService : IPaymentService
    {
        readonly PaymentsConfiguration _settings;
        readonly ILogger _logger;
        readonly ApplicationDbContext _db;
        readonly IRateCache _rateCache;
        readonly IMemoryCache _cache;
        readonly IExchangeBroker _broker;
        readonly IHttpContextAccessor _context;
        readonly GraftService _graft;

        public PaymentService(ILoggerFactory loggerFactory,
            IConfiguration configuration,
            ApplicationDbContext db,
            IRateCache rateCache,
            IMemoryCache cache,
            IExchangeBroker broker,
            IHttpContextAccessor context,
            GraftService graft)
        {
            _settings = configuration
                .GetSection("PaymentService")
                .Get<PaymentsConfiguration>();

            _logger = loggerFactory.CreateLogger(nameof(PaymentService));
            _db = db;
            _rateCache = rateCache;
            _cache = cache;
            _broker = broker;
            _context = context;
            _graft = graft;
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

                Terminal = terminal,
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

            try
            {
                if (payment.Status == PaymentStatus.New)
                {
                    payment.Status = PaymentStatus.Fail;

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

                        int blockNumber = await _graft.Sale(payment.Id, payment.PayAmount);

                        payment.BlockNumber = blockNumber;
                        payment.PayWalletAddress = _graft.GetQr(payment.Id, payment.PayAmount, blockNumber);
                    }
                    else
                    {
                        // calculate exchange
                        var brokerParams = new BrokerExchangeParams
                        {
                            PaymentId = payment.Id,
                            FiatCurrency = payment.SaleCurrency,
                            SellFiatAmount = payment.SaleAmount,
                            SellCurrency = currency,
                            BuyCurrency = "GRFT",
                            WalletAddress = _settings.GraftWalletAddress

                            //PaymentId = payment.Id,
                            //SaleAmount = payment.SaleAmount,
                            //SaleCurrency = payment.SaleCurrency,
                            //PayCurrency = currency,

                            //ServiceProviderFee = terminal.ServiceProvider.TransactionFee,
                            //ServiceProviderWallet = terminal.ServiceProvider.WalletAddress,
                            //MerchantWallet = terminal.Merchant.WalletAddress
                        };

                        var calcExchange = await _broker.CalcExchange(brokerParams);

                        payment.PayToSaleRate = calcExchange.SellToUsdRate;
                        payment.GraftToSaleRate = calcExchange.GraftToUsdRate;

                        payment.PayCurrency = calcExchange.SellCurrency;
                        payment.PayAmount = calcExchange.SellAmount;
                        payment.PayWalletAddress = calcExchange.PayWalletAddress;

                        payment.ExchangeBrokerFee = calcExchange.ExchangeBrokerFeeAmount;
                        payment.MerchantAmount = calcExchange.BuyAmount - calcExchange.ExchangeBrokerFeeAmount;

                        // create RTA sale
                        int blockNumber = await _graft.Sale(payment.Id, calcExchange.BuyAmount);
                        payment.BlockNumber = blockNumber;
                        brokerParams.BlockNumber = blockNumber;

                        // initiate exchange
                        var brokerResult = await _broker.Exchange(brokerParams);
                    }

                    payment.Status = PaymentStatus.Waiting;
                }
            }
            catch(Exception ex)
            {
                payment.Status = PaymentStatus.Fail;
                payment.StatusMessage = ex.Message;
            }

            if (!_db.Payment.Any(e => e.Id == payment.Id))
                _db.Payment.Add(payment);
            else
                _db.Payment.Update(payment);
            await _db.SaveChangesAsync();

            var res = GetSaleResult(payment);
            return res;
        }

        public async Task<GatewayGetSaleStatusResult> GetSaleStatus(string id)
        {
            _logger.LogInformation("API Online Sale Status: {@params}", id);

            Payment payment = await GetPayment(id);

            await _broker.ExchangeStatus(new BrokerExchangeStatusParams() { ExchangeId = id });
            PaymentStatus status = await _graft.GetSaleStatus(payment.Id, payment.BlockNumber);

            if (status >= PaymentStatus.Received && payment.ConvertToStableTxId == null)
            {
                payment.Status = status;
                await ExchangeToStable(payment);

                _db.Payment.Add(payment);
                await _db.SaveChangesAsync();
            }

            var res = new GatewayGetSaleStatusResult()
            {
                PaymentId = payment.Id,
                Status = status
            };
            _logger.LogInformation("API GetSaleStatus Result: {@params}", res);
            return res;
        }

        async Task<Payment> GetPayment(string id)
        {
            _cache.TryGetValue(id, out Payment payment);
            if (payment == null)
            {
                payment = await _db.Payment.FirstOrDefaultAsync(t => t.Id == id);
                if (payment == null)
                    throw new ApiException(ErrorCode.PaymentNotFoundOrExpired);
            }

            return payment;
        }

        async Task ExchangeToStable(Payment payment)
        {
            try
            {
                var terminal = _db.Terminal
                    .Where(t => t.Id == payment.TerminalId)
                    .Include(t => t.ServiceProvider)
                    .Include(t => t.Store).ThenInclude(t => t.Merchant)
                    .FirstOrDefault();

                if (terminal == null)
                    throw new ApiException(ErrorCode.InvalidApiKey);

                var prm = new BrokerExchangeToStableParams()
                {
                    SellCurrency = "GRFT",
                    SellAmount = payment.MerchantAmount,
                    WalletAddress = terminal.ServiceProvider.WalletAddress
                };

                var exchangeToStableRes = await _broker.ExchangeToStable(prm);

                payment.BrokerGraftWallet = exchangeToStableRes.PayWalletAddress;
                payment.ConvertToStableTxId = exchangeToStableRes.GraftPaymentId;
                payment.ConvertToStableBlockNumber = exchangeToStableRes.GraftBlockNumber;

                // pay GRFT to the broker
                await _graft.Pay(payment.ConvertToStableTxId, payment.ConvertToStableBlockNumber,
                    payment.BrokerGraftWallet, payment.MerchantAmount);
                //await PayToBroker(payment);

                // check status
                await _broker.ExchangeToStableStatus(new BrokerExchangeStatusParams { ExchangeId = exchangeToStableRes.ExchangeId });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
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
            var brokerParams = new BrokerExchangeParams
            {
                FiatCurrency = model.SaleCurrency,
                SellFiatAmount = model.SaleAmount,
                SellCurrency = model.PayCurrency,
                BuyCurrency = "GRFT",
                WalletAddress = terminal.ServiceProvider.WalletAddress
            };

            var brokerResult = await _broker.Exchange(brokerParams);

            var payment = new Payment()
            {
                Id = brokerResult.ExchangeId,
                TransactionDate = DateTime.UtcNow,
                Status = PaymentStatus.New,

                TerminalId = terminal.Id,
                StoreId = terminal.StoreId,
                ServiceProviderId = terminal.ServiceProviderId,

                SaleAmount = model.SaleAmount,
                SaleCurrency = model.SaleCurrency,

                PayToSaleRate = brokerResult.SellToUsdRate,
                GraftToSaleRate = brokerResult.GraftToUsdRate,

                PayCurrency = brokerResult.SellCurrency,
                PayAmount = brokerResult.SellAmount,
                PayWalletAddress = brokerResult.PayWalletAddress,

                //ServiceProviderFee = brokerResult.ServiceProviderFee,
                ExchangeBrokerFee = brokerResult.ExchangeBrokerFeeAmount,
                //MerchantAmount = brokerResult.MerchantAmount,
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

            //todo
            //var dapiResult = await _dapi.Sale(dapiParams);

            //payment.Status = PaymentStatus.Waiting;
            //payment.BlockNumber = dapiResult.BlockNumber;
            //payment.PayWalletAddress = $"{payment.Id};{terminal.Merchant.WalletAddress};{payment.PayAmount:N8};{dapiResult.BlockNumber}";

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
