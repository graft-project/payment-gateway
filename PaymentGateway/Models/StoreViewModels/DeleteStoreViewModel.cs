using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Models.StoreViewModels
{
    public class DeleteStoreViewModel
    {
        public int Id { get; set; }

        public bool CanDelete => TerminalCount + PaymentCount == 0;

        [Display(Name = "Merchant Name")]
        public string MerchantName { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "Status")]
        public StoreStatus Status { get; set; }


        [Display(Name = "Terminal Count")]
        public int TerminalCount { get; set; }

        [Display(Name = "Payment Count")]
        public int PaymentCount { get; set; }
    }
}
