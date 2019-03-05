using Graft.DAPI;
using Graft.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using WalletRpc;

namespace PaymentGateway.Services
{
    public class GraftService : IGraftService
    {
        readonly PaymentsConfiguration _settings;
        readonly ILogger _logger;
        readonly GraftDapi _dapi;
        readonly Wallet _wallet;

        public GraftService(ILoggerFactory loggerFactory,
            IConfiguration configuration)
        {
            _settings = configuration
                .GetSection("PaymentService")
                .Get<PaymentsConfiguration>();

            _logger = loggerFactory.CreateLogger(nameof(GraftService));

            _dapi = new GraftDapi(_settings.DapiUrl);
            _wallet = new Wallet(_settings.GraftWalletUrl);
        }

        public async Task<int> Sale(string id, decimal amount)
        {
            var dapiParams = new DapiSaleParams
            {
                PaymentId = id,
                SaleDetails = null,
                Address = _settings.GraftWalletAddress,
                Amount = GraftConvert.ToAtomicUnits(amount)
            };

            var res = await _dapi.Sale(dapiParams);

            return res.BlockNumber;
        }

        public string GetQr(string id, decimal amount, int blockNumber)
        {
            return $"{id};{_settings.GraftWalletAddress};{amount:N8};{blockNumber}";
        }

        internal async Task<PaymentStatus> GetSaleStatus(string id, int blockNumber)
        {
            var dapiStatusParams = new DapiSaleStatusParams
            {
                PaymentId = id,
                BlockNumber = blockNumber
            };

            int count = 10;
            var saleStatusResult = await _dapi.GetSaleStatus(dapiStatusParams);
            while (saleStatusResult.Status < DapiSaleStatus.Success)
            {
                saleStatusResult = await _dapi.GetSaleStatus(dapiStatusParams);
                if (count-- < 0)
                    break;
                await Task.Delay(1000);
            }

            return saleStatusResult.GetPaymentStatus();
        }

        internal async Task Pay(string id, int blockNumber, string address, decimal amount)
        {
            // sale_details -----------------------------------------
            var dapiSaleDetailsParams = new DapiSaleDetailsParams
            {
                PaymentId = id,
                BlockNumber = blockNumber
            };
            var saleDetailsResult = await _dapi.SaleDetails(dapiSaleDetailsParams);


            // prepare payment
            var destinations = new List<Destination>();

            // add fee for each node in the AuthSample
            ulong totalAuthSampleFee = 0;
            foreach (var item in saleDetailsResult.AuthSample)
            {
                destinations.Add(new Destination { Amount = item.Fee, Address = item.Address });
                totalAuthSampleFee += item.Fee;
            }

            // destination - ServiceProvider
            destinations.Add(new Destination
            {
                Amount = GraftConvert.ToAtomicUnits(amount) - totalAuthSampleFee,
                Address = address
            });

            var transferParams = new TransferParams
            {
                Destinations = destinations.ToArray(),
                DoNotRelay = true,
                GetTxHex = true,
                GetTxMetadata = true,
                GetTxKey = true
            };

            var transferResult = await _wallet.TransferRta(transferParams);

            // DAPI pay
            var payParams = new DapiPayParams
            {
                Address = address,
                PaymentId = id,
                BlockNumber = blockNumber,
                Amount = GraftConvert.ToAtomicUnits(amount),
                Transactions = new string[] { transferResult.TxBlob }
            };

            var payResult = await _dapi.Pay(payParams);
        }
    }
}
