using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Models.UserViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string ApplicationRoleName { get; set; }
    }
}
