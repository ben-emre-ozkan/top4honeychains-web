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
        private readonly Services.OrgModel.FeatureFlags _flags;

        // A global role that was found in the received database and is used nowhere in the code (an organization role
        // of the same name exists, given per organization); not offered for new users
        public const string RetiredSampleAcceptanceRole = "SampleAcceptanceOfficer";

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            Microsoft.Extensions.Options.IOptions<Services.OrgModel.FeatureFlags> flags)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _flags = flags.Value;
        }

        // With the organization model only the platform administrator is a global role; organization roles are
        // memberships (Organizations -> Add member) and beekeepers and consumers do not log in (D-22, D-23).
        // In admin mode (flag off) the stored roles are offered, without the retired one.
        private List<string> AllowedRoles() => _flags.OrgRoleModel
            ? new List<string> { Services.OrgModel.CurrentOrg.AdminRole }
            : _roleManager.Roles.Select(r => r.Name!).Where(n => n != RetiredSampleAcceptanceRole).ToList();

        // USER LIST
        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();

            ViewBag.Controller = "User Management";
            ViewBag.Action = "Users";

            return View(users);
        }

        [HttpGet]
        public IActionResult Create() => CreateView(new RegisterViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            if (string.IsNullOrEmpty(model.Role))
            {
                // a user without a global role gets access only through organization memberships
                if (!_flags.OrgRoleModel)
                    ModelState.AddModelError(nameof(model.Role), "Select a valid role.");
            }
            else if (!AllowedRoles().Contains(model.Role) || !await _roleManager.RoleExistsAsync(model.Role))
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
                if (!string.IsNullOrEmpty(model.Role))
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
                    if (!roleResult.Succeeded)
                    {
                        foreach (var error in roleResult.Errors)
                            ModelState.AddModelError("", error.Description);
                        return CreateView(model);
                    }
                }
                else
                    TempData["Success"] = "User created without a global role. Give the organization role under Organizations → Add member.";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return CreateView(model);
        }

        // The role dropdown must be reloaded whenever the form is shown again
        private IActionResult CreateView(RegisterViewModel model)
        {
            ViewBag.Roles = AllowedRoles();
            ViewBag.OrgModel = _flags.OrgRoleModel;
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
