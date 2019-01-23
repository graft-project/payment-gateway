using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Models.TerminalViewModels
{
    public class MerchantTerminalViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Store")]
        public int StoreId { get; set; }

        [Display(Name = "Terminal Name")]
        public string Name { get; set; }

        [Display(Name = "Status")]
        public TerminalStatus Status { get; set; }

        [Display(Name = "Currency Code")]
        public string CurrencyCode { get; set; }
    }
}
