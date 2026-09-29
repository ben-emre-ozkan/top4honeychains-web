using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TOP4HoneyChains.Web.Models.Identity;
using TOP4HoneyChains.Web.Models.ViewModels;
using TOP4HoneyChains.Web.Services;

namespace TOP4HoneyChains.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        // Only administrators use the web panel in this release (docs/DECISIONS.md, D-01)
        private const string PanelRole = "Administrator";

        // Flash messages shown on the login page
        public const string LoginSuccessKey = "LoginSuccess";
        public const string LoginErrorKey = "LoginError";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            IStringLocalizer<SharedResource> localizer,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _localizer = localizer;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole(PanelRole))
                    return RedirectToAction("Index", "Dashboard");

                // A non-admin session (e.g. from before D-01) would loop via AccessDenied
                await _signInManager.SignOutAsync();
                ModelState.AddModelError("", _localizer["Login.NoPanelAccess"]);
            }
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.UserName,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                if (user != null && await _userManager.IsInRoleAsync(user, PanelRole))
                    return RedirectToAction("Index", "Dashboard");

                // Valid credentials, but no access to the panel: don't keep the session
                await _signInManager.SignOutAsync();
                ModelState.AddModelError("", _localizer["Login.NoPanelAccess"]);
                return View(model);
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            // Same message whether or not the account exists (no account enumeration)
            if (user == null)
            {
                ViewBag.Message = _localizer["ForgotPassword.Sent"].Value;
                return View();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var resetLink = Url.Action(
                "ResetPassword",
                "Account",
                new { email = model.Email, token = token },
                Request.Scheme);

            var mailBody = $@"<p>Hello,</p>
                              <p>You requested a password reset.</p>
                              <p><a href='{resetLink}'>Click here to reset your password</a></p>
                              <p>If you did not request this, please ignore this email.</p>
                              <br />
                              <p>TOP4HoneyChains</p>";

            try
            {
                await _emailService.SendAsync(model.Email, "Reset your TOP4HoneyChains password", mailBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password reset e-mail could not be sent for user {UserId}", user.Id);
                ModelState.AddModelError("", _localizer["ForgotPassword.SendFailed"]);
                return View(model);
            }

            ViewBag.Message = _localizer["ForgotPassword.Sent"].Value;

            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
                return RedirectToAction("Login");

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Güvenlik: kullanıcı yoksa bile aynı mesaj
                TempData[LoginSuccessKey] = _localizer["ResetPassword.Success"].Value;
                return RedirectToAction("Login");
            }

            var result = await _userManager.ResetPasswordAsync(
                user,
                model.Token,
                model.Password);

            if (result.Succeeded)
            {
                TempData[LoginSuccessKey] = _localizer["ResetPassword.Success"].Value;
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public async Task<IActionResult> AccessDenied()
        {
            // Signed-in non-admins have nothing to open in the panel (D-01): sign them out
            // instead of showing a page whose only link leads back here
            if (User.Identity?.IsAuthenticated == true && !User.IsInRole(PanelRole))
            {
                await _signInManager.SignOutAsync();
                TempData[LoginErrorKey] = _localizer["Login.NoPanelAccess"].Value;
                return RedirectToAction(nameof(Login));
            }

            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult LockScreen()
        {
            return View(new UnlockViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(UnlockViewModel model)
        {
            if (!ModelState.IsValid)
                return View("LockScreen", model);

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login");

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (result.Succeeded)
                return RedirectToAction("Index", "Dashboard");

            ModelState.AddModelError("", "Invalid password");

            return View("LockScreen", model);
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            var model = new ProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };


            ViewBag.Controller = "Account";
            ViewBag.Action = "My Profile";

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdatePhone(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            user.PhoneNumber = model.PhoneNumber;

            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Phone number updated.";

            return RedirectToAction("Profile");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ProfileChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Profile");

            var user = await _userManager.GetUserAsync(User);

            var result = await _userManager.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword);

            if (result.Succeeded)
            {
                TempData["Success"] = "Password changed.";
            }
            else
            {
                TempData["Error"] = result.Errors.First().Description;
            }

            return RedirectToAction("Profile");
        }
    }
}
