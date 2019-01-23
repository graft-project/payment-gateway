using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Models.UserViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public string UserName { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string ApplicationRoleName { get; set; }
    }
}
