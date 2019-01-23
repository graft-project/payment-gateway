using Graft.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Models.PaymentViewModels
{
    public class PaymentViewModel
    {
        public string Id { get; set; }

        public int TerminalId { get; set; }

        public int StoreId { get; set; }

        public int ServiceProviderId { get; set; }


        [Display(Name = "Date")]
        public DateTime TransactionDate { get; set; }

        [Display(Name = "Status")]
        public PaymentStatus Status { get; set; }

        [Display(Name = "Sale Amount")]
        [DisplayFormat(DataFormatString = "{0:N7}", ApplyFormatInEditMode = true)]
        public decimal SaleAmount { get; set; }

        [Display(Name = "Sale Currency")]
        public string SaleCurrency { get; set; }


        [Display(Name = "Pay To Sale Rate")]
        [DisplayFormat(DataFormatString = "{0:N7}", ApplyFormatInEditMode = true)]
        public decimal PayToSaleRate { get; set; }

        [Display(Name = "GRAFT To Sale Rate")]
        [DisplayFormat(DataFormatString = "{0:N7}", ApplyFormatInEditMode = true)]
        public decimal GraftToSaleRate { get; set; }


        [Display(Name = "Pay Currency")]
        public string PayCurrency { get; set; }

        [Display(Name = "Pay Amount")]
        [DisplayFormat(DataFormatString = "{0:N7}", ApplyFormatInEditMode = true)]
        public decimal PayAmount { get; set; }

        [Display(Name = "Pay Wallet Address")]
        public string PayWalletAddress { get; set; }


        [Display(Name = "Service Provider Fee")]
        [DisplayFormat(DataFormatString = "{0:N7}", ApplyFormatInEditMode = true)]
        public decimal ServiceProviderFee { get; set; }

        [Display(Name = "Exchange Broker Fee")]
        [DisplayFormat(DataFormatString = "{0:N7}", ApplyFormatInEditMode = true)]
        public decimal ExchangeBrokerFee { get; set; }


        [Display(Name = "Merchant Amount")]
        [DisplayFormat(DataFormatString = "{0:N7}", ApplyFormatInEditMode = true)]
        public decimal MerchantAmount { get; set; }

        [Display(Name = "Sale Details")]
        public string SaleDetails { get; set; }

        [Display(Name = "Merchant Name")]
        public string MerchantName { get; internal set; }

        [Display(Name = "Terminal Name")]
        public string TerminalName { get; internal set; }

        // --------------------------------------
        public ServiceProvider ServiceProvider { get; set; }
        public Store Store { get; set; }
        public Terminal Terminal { get; set; }

    }
}
