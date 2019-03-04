using Graft.Infrastructure;
using Graft.Infrastructure.Broker;
using Graft.Infrastructure.Gateway;
using Graft.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentGateway.Data;
using PaymentGateway.Services;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PaymentGateway.Controllers.Api
{
    [Route("v1.0")]
    [ApiController]
    public class ApiPosController : ControllerBase
    {
        readonly ILogger _logger;
        readonly ApplicationDbContext _db;
        readonly IExchangeBroker _broker;
        readonly IPaymentService _paymentService;

        public ApiPosController(
            ILoggerFactory loggerFactory,
            ApplicationDbContext db,
            IExchangeBroker broker,
            IPaymentService paymentService)
        {
            _logger = loggerFactory.CreateLogger(nameof(ApiPosController));
            _db = db;
            _broker = broker;
            _paymentService = paymentService;
        }

        [HttpPost]
        [Route("OnlineSale")]
        public IActionResult OnlineSale([FromForm] GatewayOnlineSaleParams model)
        {
            _logger.LogInformation("API Online Sale: {@params}", model);

            string apiKey = Request.Headers["Graft-Access-Key"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return Error(ErrorCode.InvalidApiKey);

            string timestamp = Request.Headers["Graft-Access-Timestamp"];
            if (string.IsNullOrWhiteSpace(timestamp))
                return Error(ErrorCode.InvalidApiKey);

            string sign = Request.Headers["Graft-Access-Sign"];
            if (string.IsNullOrWhiteSpace(sign))
                return Error(ErrorCode.InvalidApiKey);

            model.PosSn = apiKey;

            var res = _paymentService.PrepareOnlineSale(model, timestamp, sign);
            return Ok(res);
        }

        [HttpPost]
        [Route("Sale")]
        public async Task<IActionResult> Sale([FromBody] GatewaySaleParams model)
        {
            _logger.LogInformation("API Sale: {@params}", model);
            var res = await _paymentService.Sale(model);
            return Ok(res);
        }

        [HttpGet]
        [Route("OnlineGetSaleStatus")]
        public async Task<IActionResult> OnlineGetSaleStatus(string id)
        {
            _logger.LogInformation("API OnlineGetSaleStatus: {@params}", id);

            string apiKey = Request.Headers["Graft-Access-Key"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return Error(ErrorCode.InvalidApiKey);

            string timestamp = Request.Headers["Graft-Access-Timestamp"];
            if (string.IsNullOrWhiteSpace(timestamp))
                return Error(ErrorCode.InvalidApiKey);

            string sign = Request.Headers["Graft-Access-Sign"];
            if (string.IsNullOrWhiteSpace(sign))
                return Error(ErrorCode.InvalidApiKey);

            var terminal = _db.Terminal
                .Where(t => t.SerialNumber == apiKey)
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

            var res = await _paymentService.GetSaleStatus(id);
            return Ok(res);
        }

        [HttpPost]
        [Route("GetSaleStatus")]
        public async Task<IActionResult> GetSaleStatus([FromBody] GatewayGetSaleStatusParams model)
        {
            _logger.LogInformation("API SaleStatus: {@params}", model);
            var res = await _paymentService.GetSaleStatus(model.PaymentId);
            return Ok(res);
        }

        [HttpGet]
        [Route("GetParams")]
        public async Task<IActionResult> GetParams()
        {
            var brokerParams = await _broker.GetParams();

            var res = new GatewayParams()
            {
                AppVersion = typeof(ApiPosController).Assembly.GetName().Version.ToString(),

                BrokerParams = new BrokerParams()
                {
                    Version = brokerParams.Version,
                    Fee = brokerParams.Fee,
                    Network = brokerParams.Network,
                    Currencies = brokerParams.Currencies,
                    Cryptocurrencies = brokerParams.Cryptocurrencies
                }
            };

            _logger.LogInformation("API GetParams: {@params}", res);
            return Ok(res);
        }

        #region Helpers

        IActionResult Error(ErrorCode errorCode, object param = null)
        {
            var error = new ApiError(errorCode, param);
            _logger.LogError("API Error ({code}) {message}", error.Code, error.Message);
            return Ok(new ApiErrorResult(error));
        }

        #endregion
    }
}