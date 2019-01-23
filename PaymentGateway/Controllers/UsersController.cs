using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PaymentGateway.Data;
using PaymentGateway.Models.UserViewModels;
using PaymentGateway.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        readonly UserManager<ApplicationUser> _userManager;
        readonly RoleManager<IdentityRole> _roleManager;
        readonly SignInManager<ApplicationUser> _signInManager;
        readonly IUserService _userService;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IUserService userService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _userService = userService;
        }

        public ActionResult Index()
        {
            List<UserListViewModel> model = new List<UserListViewModel>();
            model = _userManager.Users.Where(u => u.UserName != "admin").Select(u => new UserListViewModel
            {
                Id = u.Id,
                Name = u.UserName,
                Email = u.Email,
                RoleName = (_roleManager.Roles.SingleOrDefault(r => r.Id == u.AspNetRoleId).Name)
            }).ToList();
            return View(model);
        }

        [HttpGet]
        public IActionResult Invite()
        {
            var model = new InviteUserViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Invite(InviteUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.Invite(model.Email, "ServiceProvider");
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var item in result.Errors)
                        ModelState.AddModelError(string.Empty, item.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var model = new EditUserViewModel()
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName
            };

            var role = await _roleManager.FindByIdAsync(user.AspNetRoleId);
            if (role != null)
                model.ApplicationRoleName = role.Name;

            ViewData["ApplicationRoles"] = new SelectList(_roleManager.Roles);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    user.Email = model.Email;

                    var newRole = await _roleManager.FindByNameAsync(model.ApplicationRoleName);
                    var oldRole = await _roleManager.FindByIdAsync(user.AspNetRoleId);

                    user.AspNetRoleId = newRole.Id;

                    IdentityResult result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        if (newRole != oldRole)
                        {
                            if (oldRole != null)
                                await _userManager.RemoveFromRoleAsync(user, oldRole.Name);

                            if (newRole != null)
                                await _userManager.AddToRoleAsync(user, newRole.Name);
                        }

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        foreach (var item in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, item.Description);
                        }
                    }
                }
            }

            ViewData["ApplicationRoles"] = new SelectList(_roleManager.Roles);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new DeleteUserViewModel()
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                ApplicationRoleName = _roleManager.FindByIdAsync(user.AspNetRoleId).Result.Name
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user != null && user.UserName != "admin")
                {
                    IdentityResult result = await _userManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        foreach (var item in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, item.Description);
                        }
                    }
                }
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ResendInvite(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var model = new ResendInviteViewModel()
            {
                UserID = user.Id,
                Email = user.Email
            };

            return View(model);
        }

        [HttpPost, ActionName("ResendInvite")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendInviteConfirmed(string id)
        {
            if (ModelState.IsValid)
            {
                if (!await _userService.ResendInvite(id))
                    return NotFound();
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}