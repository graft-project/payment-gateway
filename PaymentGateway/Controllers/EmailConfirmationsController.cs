using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Data;
using PaymentGateway.Models.EmailConfirmation;
using System.Threading.Tasks;

namespace PaymentGateway.Controllers
{
    public class EmailConfirmationsController : Controller
    {
        readonly UserManager<ApplicationUser> _userManager;
        readonly SignInManager<ApplicationUser> _signInManager;

        public EmailConfirmationsController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult ConfirmInvite(string userId, string code)
        {
            var model = new ConfirmInviteViewModel
            {
                Code = code
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmInvite(ConfirmInviteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return Redirect("/Identity/Account/ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmEmailResult = await _userManager.ConfirmEmailAsync(user, token);
                if (confirmEmailResult.Succeeded)
                {
                    var signInResult = await _signInManager.PasswordSignInAsync(model.Email, model.Password, true, lockoutOnFailure: true);
                    if (signInResult.Succeeded)
                    {
                        return Redirect("/Identity/Account/Manage");
                    }
                }

                return RedirectToAction("ConfirmInviteConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ConfirmInviteConfirmation(string userId, string code)
        {
            return View();
        }
    }
}