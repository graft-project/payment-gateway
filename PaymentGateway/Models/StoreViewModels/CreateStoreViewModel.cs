using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Models.StoreViewModels
{
    public class CreateStoreViewModel
    {
        [Display(Name = "Store Name")]
        public string Name { get; set; }

        public string Address { get; set; }

        public StoreStatus Status { get; set; }
    }
}
