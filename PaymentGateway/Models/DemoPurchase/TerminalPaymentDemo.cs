using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentGateway.Models
{
    public class TerminalPaymentDemo : Terminal
    {
        [NotMapped]
        public decimal SaleAmount { get; set; } = 0.0002m;

        [NotMapped]
        public string PayCurrency { get; set; } = "BTC";

        public static TerminalPaymentDemo FromTerminal(Terminal terminal)
        {
            return new TerminalPaymentDemo()
            {
                Id = terminal.Id,
                StoreId = terminal.StoreId,
                MerchantId = terminal.MerchantId,
                ServiceProviderId = terminal.ServiceProviderId,
                SerialNumber = terminal.SerialNumber,
                Name = terminal.Name,
                Status = terminal.Status,
                ServiceProvider = terminal.ServiceProvider,
                Store = terminal.Store,
                Merchant = terminal.Merchant
            };
        }

        public static Terminal ToTerminal(TerminalPaymentDemo terminal)
        {
            return new Terminal()
            {
                Id = terminal.Id,
                StoreId = terminal.StoreId,
                MerchantId = terminal.MerchantId,
                ServiceProviderId = terminal.ServiceProviderId,
                SerialNumber = terminal.SerialNumber,
                Name = terminal.Name,
                Status = terminal.Status,
                ServiceProvider = terminal.ServiceProvider,
                Store = terminal.Store,
                Merchant = terminal.Merchant
            };
        }
    }
}
