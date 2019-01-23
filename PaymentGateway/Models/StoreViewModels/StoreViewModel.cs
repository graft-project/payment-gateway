using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Models.StoreViewModels
{
    public class StoreViewModel
    {
        public int ID { get; set; }

        [Display(Name = "Merchant Name")]
        public int MerchantName { get; set; }

        [Display(Name = "MerchantEmail")]
        public int MerchantEmail { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public bool Active { get; set; } = true;
    }
}
