using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Models.UserViewModels
{
    public class DeleteUserViewModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        public string Email { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string ApplicationRoleName { get; set; }
    }
}
