using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Models.TerminalViewModels
{
    public class ApiKeyViewModel
    {
        public int Id { get; set; }

        [Display(Name = "API Key")]
        public string SerialNumber { get; set; }

        [Display(Name = "API Secret")]
        public string ApiSecret { get; set; }

        public int StoreId { get; set; }

        public int MerchantId { get; set; }

        public int ServiceProviderId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Status")]
        public TerminalStatus Status { get; set; }

        public bool Virtual { get; set; }


        [Display(Name = "Service Provider")]
        public string ServiceProviderName { get; set; }

        [Display(Name = "Store")]
        public string StoreName { get; set; }
    }
}
