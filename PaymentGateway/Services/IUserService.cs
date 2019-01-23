using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PaymentGateway.Services
{
    public interface IUserService
    {
        Task<IdentityResult> Invite(string email, string role);
        Task<bool> ResendInvite(string userId);
        Task<int> GetDefaultStoreId(int merchantId);
        Task<int> GetCurrentServiceProviderId(ClaimsPrincipal principal);
        Task<int> GetCurrentMerchantId(ClaimsPrincipal principal);
    }
}