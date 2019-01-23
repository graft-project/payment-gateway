using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Models.TerminalViewModels
{
    public class DeleteTerminalViewModel
    {
        public int Id { get; set; }

        public bool CanDelete => PaymentCount == 0;

        [Display(Name = "Store Name")]
        public string StoreName { get; set; }

        [Display(Name = "Merchant Name")]
        public string MerchantName { get; set; }

        [Display(Name = "Service Provider Name")]
        public string ServiceProviderName { get; set; }

        [Display(Name = "Serial Number")]
        public string SerialNumber { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Status")]
        public TerminalStatus Status { get; set; }

        [Display(Name = "Payment Count")]
        public int PaymentCount { get; set; }
    }
}
