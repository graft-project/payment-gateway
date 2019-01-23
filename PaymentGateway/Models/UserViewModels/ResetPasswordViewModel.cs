using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Models.UserViewModels
{
    public class ResetPasswordViewModel
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
