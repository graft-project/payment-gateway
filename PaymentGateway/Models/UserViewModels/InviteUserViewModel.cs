using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Models.UserViewModels
{
    public class InviteUserViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        //[Required]
        //[Display(Name = "Role")]
        //public string ApplicationRoleName { get; set; }
    }
}
