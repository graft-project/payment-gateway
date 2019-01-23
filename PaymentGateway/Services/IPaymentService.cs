using System.Threading.Tasks;
using Graft.Infrastructure.Gateway;
using PaymentGateway.Models;

namespace PaymentGateway.Services
{
    public interface IPaymentService
    {
        GatewayOnlineSaleResult PrepareOnlineSale(GatewayOnlineSaleParams model, string timestamp, string sign);
        Task<GatewaySaleResult> OnlineSale(Payment payment, string currency);
        Task<GatewaySaleResult> Sale(GatewaySaleParams model);
        Task<GatewayGetSaleStatusResult> GetSaleStatus(string id);
    }
}