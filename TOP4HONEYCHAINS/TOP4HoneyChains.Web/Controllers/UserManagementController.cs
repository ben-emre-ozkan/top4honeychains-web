using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TOP4HoneyChains.Web.Models.Identity;
using TOP4HoneyChains.Web.Models.UsersManagement;
using TOP4HoneyChains.Web.Models.ViewModels;

namespace TOP4HoneyChains.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // USER LIST
        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();

            ViewBag.Controller = "User Management";
            ViewBag.Action = "Users";

            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            ViewBag.Controller = "User Management";
            ViewBag.Action = "Create New User";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Role) && !await _roleManager.RoleExistsAsync(model.Role))
                ModelState.AddModelError(nameof(model.Role), "Select a valid role.");

            if (!ModelState.IsValid)
                return CreateView(model);

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return CreateView(model);
        }

        // The role dropdown must be reloaded whenever the form is shown again
        private IActionResult CreateView(RegisterViewModel model)
        {
            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            ViewBag.Controller = "User Management";
            ViewBag.Action = "Create New User";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
                await _userManager.DeleteAsync(user);

            return RedirectToAction("Index");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Password update failed: enter the new password twice (identical).";
                return RedirectToAction("Index");
            }

            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                TempData["error"] = "Password update failed: user not found.";
                return RedirectToAction("Index");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(
                user,
                token,
                model.NewPassword);

            if (result.Succeeded)
            {
                await _userManager.UpdateSecurityStampAsync(user);

                TempData["success"] = "Password updated successfully.";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Password update failed.";
            return RedirectToAction("Index");
        }
    }
}
