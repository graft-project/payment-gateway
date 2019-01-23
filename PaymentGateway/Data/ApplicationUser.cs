using Microsoft.AspNetCore.Identity;

namespace PaymentGateway.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string AspNetRoleId { get; set; }
    }
}
