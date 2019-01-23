using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Models.TerminalViewModels
{
    public class DeleteApiKeyViewModel
    {
        public int Id { get; set; }

        [Display(Name = "API Key")]
        public string SerialNumber { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Status")]
        public TerminalStatus Status { get; set; }

        [Display(Name = "Service Provider")]
        public string ServiceProviderName { get; set; }

        [Display(Name = "Store")]
        public string StoreName { get; set; }

        [Display(Name = "Payment Count")]
        public int PaymentCount { get; set; }

        public bool CanDelete => PaymentCount == 0;
    }
}
