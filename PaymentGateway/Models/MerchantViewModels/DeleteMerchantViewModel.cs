using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Models.MerchantViewModels
{
    public class DeleteMerchantViewModel
    {
        public int Id { get; set; }

        public bool CanDelete => StoresCount + TerminalCount + PaymentCount == 0;

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "Wallet Address")]
        public string WalletAddress { get; set; }

        [Display(Name = "Status")]
        public MerchantStatus Status { get; set; }

        [Display(Name = "Stores Count")]
        public int StoresCount { get; set; }

        [Display(Name = "Terminal Count")]
        public int TerminalCount { get; set; }

        [Display(Name = "Payment Count")]
        public int PaymentCount { get; set; }
    }
}
