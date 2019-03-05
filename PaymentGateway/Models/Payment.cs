using Graft.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentGateway.Models
{
    public class Payment
    {
        [Key]
        [Column(TypeName = "varchar(64)")]
        public string Id { get; set; }

        [Required]
        public int TerminalId { get; set; }

        [Required]
        public int StoreId { get; set; }

        [Required]
        public int ServiceProviderId { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; }

        [Required]
        public PaymentStatus Status { get; set; }
        public string StatusMessage { get; set; }

        [Required]
        public decimal SaleAmount { get; set; }

        [Required]
        [Column(TypeName = "char(3)")]
        public string SaleCurrency { get; set; }


        [Required]
        public decimal PayToSaleRate { get; set; }

        [Required]
        public decimal GraftToSaleRate { get; set; }


        [Required]
        [Column(TypeName = "char(6)")]
        public string PayCurrency { get; set; }

        [Required]
        public decimal PayAmount { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string PayWalletAddress { get; set; }


        [Required]
        public decimal ServiceProviderFee { get; set; }

        [Required]
        public decimal ExchangeBrokerFee { get; set; }


        [Required]
        public decimal MerchantAmount { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string SaleDetails { get; set; }

        public int BlockNumber { get; set; }

        // for online sells
        public string ExternalOrderId { get; set; }
        public string CallbackUrl { get; set; }
        public string CancelUrl { get; set; }
        public string CompleteUrl { get; set; }

        // conversion to stable coin
        public string ConvertToStableTxId { get; set; }
        public PaymentStatus ConvertToStableTxStatus { get; set; }
        public int ConvertToStableBlockNumber { get; set; }
        public string BrokerGraftWallet { get; set; }
        public string ConvertToStableTxStatusDescription { get; set; }

        // --------------------------------------
        [ForeignKey("ServiceProviderId")]
        public ServiceProvider ServiceProvider { get; set; }

        [ForeignKey("StoreId")]
        public Store Store { get; set; }

        [ForeignKey("TerminalId")]
        public Terminal Terminal { get; set; }
    }
}
