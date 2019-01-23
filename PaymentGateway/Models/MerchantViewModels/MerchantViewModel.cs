using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Models.MerchantViewModels
{
    public class MerchantViewModel
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        [Required]
        [Display(Name = "Merchant Name")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "Wallet Address")]
        public string WalletAddress { get; set; }

        [Display(Name = "Status")]
        public MerchantStatus Status { get; set; }

        [Display(Name = "Accepted")]
        public bool EmailConfirmed { get; set; }
    }
}
