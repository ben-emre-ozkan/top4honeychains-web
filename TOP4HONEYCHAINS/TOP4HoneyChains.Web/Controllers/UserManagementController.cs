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
        private readonly Services.OrgModel.AuditWriter _audit;
        private readonly Services.OrgModel.CredentialService _credentials;

        // A global role that was found in the received database and is used nowhere in the code (an organization role
        // of the same name exists, given per organization); not offered for new users
        public const string RetiredSampleAcceptanceRole = "SampleAcceptanceOfficer";

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            Microsoft.Extensions.Options.IOptions<Services.OrgModel.FeatureFlags> flags,
            Services.OrgModel.AuditWriter audit,
            Services.OrgModel.CredentialService credentials)
        {
            _credentials = credentials;
            _userManager = userManager;
            _roleManager = roleManager;
            _flags = flags.Value;
            _audit = audit;
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

            // D-78 (O-10, D78-SPEC 4.3): sign-in details by SMS (default), or an initial password set here; both are changed at first sign-in
            var manual = model.Delivery == Services.OrgModel.CredentialDelivery.Manual || (model.Delivery == null && !string.IsNullOrEmpty(model.Password));
            if (manual && string.IsNullOrEmpty(model.Password))
                ModelState.AddModelError(nameof(model.Password), "Enter the initial password twice.");
            if (!manual && string.IsNullOrWhiteSpace(model.PhoneNumber))
                ModelState.AddModelError(nameof(model.PhoneNumber), "A mobile phone number is required.");

            if (!ModelState.IsValid)
                return CreateView(model);

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim()
            };

            var result = await _userManager.CreateAsync(user, manual ? model.Password! : Services.OrgModel.CredentialService.Unusable());

            if (result.Succeeded)
            {
                if (manual)
                    await _credentials.RecordManualAsync(user.Id, _userManager.GetUserId(User));
                else
                {
                    var issued = await _credentials.IssueBySmsAsync(user, model.PhoneNumber!, System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
                        _userManager.GetUserId(User), Services.OrgModel.CredentialService.SignInLink(HttpContext));
                    TempData[issued.Ok ? "Success" : "Error"] = issued.Ok
                        ? $"Sign-in details for {user.UserName} were sent by SMS."
                        : $"The user {user.UserName} was created, but the SMS could not be sent: {issued.Error} Send the sign-in details again from the user list.";
                }
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
                else if (TempData["Error"] == null)
                    TempData["Info"] = "User created without a global role. Give the organization role under Organizations → Add member.";
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

            // D-75: never your own account; every deletion is in the audit log
            if (user != null && user.Id == _userManager.GetUserId(User))
            {
                TempData["error"] = "You cannot delete your own account.";
                return RedirectToAction("Index");
            }
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    await _audit.WriteAsync("Delete", "AspNetUsers", user.Id, new { user.UserName, user.Email, Roles = string.Join(", ", roles) });
                    TempData["success"] = "User deleted.";
                }
                else
                    TempData["error"] = "The user could not be deleted.";
            }

            return RedirectToAction("Index");
        }

        // D-78 (D78-SPEC 4.3 step 5): new sign-in details by SMS; the earlier temporary password stops working
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IssueCredentials(string id, string? phone)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();
            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["error"] = "Use your profile to change your own password.";
                return RedirectToAction("Index");
            }
            var issued = await _credentials.IssueBySmsAsync(user, string.IsNullOrWhiteSpace(phone) ? user.PhoneNumber ?? "" : phone,
                System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, _userManager.GetUserId(User),
                Services.OrgModel.CredentialService.SignInLink(HttpContext));
            if (issued.Ok)
                TempData["success"] = $"Sign-in details for {user.UserName} were sent by SMS.";
            else
                TempData["error"] = issued.Error;
            return RedirectToAction("Index");
        }

        // D-75: ends a lockout (after too many failed sign-ins) and resets the failed-attempt counter; audited
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();
            var lockedUntil = user.LockoutEnd;
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);
            await _audit.WriteAsync("Unlock", "AspNetUsers", user.Id, new { user.UserName, LockoutEnd = lockedUntil }, new { LockoutEnd = (DateTimeOffset?)null });
            TempData["success"] = "Account unlocked.";
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
                // D-78: a password set by an administrator is changed by the user at the next sign-in
                await _credentials.RecordManualAsync(user.Id, _userManager.GetUserId(User));

                TempData["success"] = "Password updated successfully.";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Password update failed.";
            return RedirectToAction("Index");
        }
    }
}
