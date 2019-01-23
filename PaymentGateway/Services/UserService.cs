using Graft.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Data;
using PaymentGateway.Models;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PaymentGateway.Services
{
    public class UserService : IUserService
    {
        readonly ApplicationDbContext _db;
        readonly UserManager<ApplicationUser> _userManager;
        readonly RoleManager<IdentityRole> _roleManager;
        readonly IEmailSender _emailService;
        readonly SignInManager<ApplicationUser> _signInManager;
        readonly IHttpContextAccessor _context;

        public UserService(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailService,
            IHttpContextAccessor context)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _context = context;
        }

        public async Task<IdentityResult> Invite(string email, string role)
        {
            var applicationRole = await _roleManager.FindByNameAsync(role);
            if (applicationRole == null || string.IsNullOrWhiteSpace(email))
                throw new ArgumentException();

            ApplicationUser user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = false,
                AspNetRoleId = applicationRole.Id
            };

            IdentityResult result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                result = await _userManager.AddToRoleAsync(user, role);
                if (result.Succeeded)
                {
                    var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await SendInviteEmail(user.Id, user.Email, code);

                    if (role == "Merchant")
                    {
                        var merchant = new Merchant
                        {
                            UserId = user.Id,
                            Name = email
                        };

                        _db.Add(merchant);
                        await _db.SaveChangesAsync();

                        var store = new Store
                        {
                            MerchantId = merchant.Id,
                            Name = "Default"
                        };

                        _db.Add(store);
                        await _db.SaveChangesAsync();
                    }
                    else if (role == "ServiceProvider")
                    {
                        var sp = new ServiceProvider
                        {
                            UserId = user.Id,
                            Name = email
                        };
                        _db.Add(sp);
                        await _db.SaveChangesAsync();
                    }
                }
            }
            return result;
        }

        public async Task<bool> ResendInvite(string userId)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            await SendInviteEmail(user.Id, user.Email, code);

            return true;
        }

        public async Task<int> GetDefaultStoreId(int merchantId)
        {
            var store = await _db.Store
                .Where(t => t.MerchantId == merchantId)
                .FirstOrDefaultAsync();

            if (store == null)
            {
                store = new Store
                {
                    MerchantId = merchantId,
                    Name = "Default"
                };

                _db.Add(store);
                await _db.SaveChangesAsync();
            }

            return store.Id;
        }

        public async Task<int> GetCurrentServiceProviderId(ClaimsPrincipal principal)
        {
            var user = await _userManager.GetUserAsync(principal);
            var provider = await _db.ServiceProvider
                .Where(t => t.UserId == user.Id)
                .FirstOrDefaultAsync();

            if (provider == null)
                throw new ApplicationException("Service Provider for current user is not found");

            return provider.Id;
        }

        public async Task<int> GetCurrentMerchantId(ClaimsPrincipal principal)
        {
            var user = await _userManager.GetUserAsync(principal);
            var merchant = await _db.Merchant
                .Where(t => t.UserId == user.Id)
                .FirstOrDefaultAsync();

            if (merchant == null)
                throw new ApplicationException("Merchant for current user is not found");

            return merchant.Id;
        }

        public Task SendInviteEmail(string userId, string email, string code)
        {
            var req = _context.HttpContext.Request;
            var url = $"{req.Scheme}://{req.Host}/EmailConfirmations/ConfirmInvite?userId={userId}&code={WebUtility.UrlEncode(code)}";

            return _emailService.SendEmailAsync(email,
                "You have been invited to the GRAFT Payment Gateway",
                $"Please confirm your account by <a href='{url}'>clicking here</a>.");
        }
    }
}
