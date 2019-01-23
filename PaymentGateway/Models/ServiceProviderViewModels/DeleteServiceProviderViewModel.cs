using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Models.ServiceProviderViewModels
{
    public class DeleteServiceProviderViewModel
    {
        public int Id { get; set; }

        public bool CanDelete => TerminalCount + PaymentCount == 0;

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "Wallet Address")]
        public string WalletAddress { get; set; }

        [Display(Name = "Transaction Fee")]
        public float TransactionFee { get; set; }

        [Display(Name = "Status")]
        public ServiceProviderStatus Status { get; set; }

        [Display(Name = "Terminal Count")]
        public int TerminalCount { get; set; }

        [Display(Name = "Payment Count")]
        public int PaymentCount { get; set; }
    }
}
