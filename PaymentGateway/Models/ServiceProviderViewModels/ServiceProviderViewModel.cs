using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Models.ServiceProviderViewModels
{
    public class ServiceProviderViewModel
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        [Required]
        [Display(Name = "Provider Name")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "Wallet Address")]
        public string WalletAddress { get; set; }

        [Display(Name = "Transaction Fee")]
        public float TransactionFee { get; set; }

        [Display(Name = "Status")]
        public ServiceProviderStatus Status { get; set; }

        [Display(Name = "Accepted")]
        public bool EmailConfirmed { get; set; }
    }
}
