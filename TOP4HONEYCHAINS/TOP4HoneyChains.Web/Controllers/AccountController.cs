using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TOP4HoneyChains.Web.Models.Identity;
using TOP4HoneyChains.Web.Models.ViewModels;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.SignedIn)]
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
        private readonly ICurrentOrg _currentOrg;
        private readonly AuditWriter _audit;
        private readonly TOP4HoneyChains.Web.Data.TOP4HoneyChainsContext _context;
        private readonly CredentialService _credentials;
        private readonly TOP4HoneyChains.Web.Services.Messaging.IMessageService _messages;
        private readonly IConfiguration _config;

        // D-78 P1 (D78-SPEC 4.3, 4.10): password reset by SMS code (default) or, with Account:PasswordResetChannel=Email, by e-mail
        private bool ResetBySms => !string.Equals(_config["Account:PasswordResetChannel"], "Email", StringComparison.OrdinalIgnoreCase);

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            IStringLocalizer<SharedResource> localizer,
            ILogger<AccountController> logger,
            ICurrentOrg currentOrg,
            AuditWriter audit,
            TOP4HoneyChains.Web.Data.TOP4HoneyChainsContext context,
            CredentialService credentials,
            TOP4HoneyChains.Web.Services.Messaging.IMessageService messages,
            IConfiguration config)
        {
            _credentials = credentials;
            _messages = messages;
            _config = config;
            _currentOrg = currentOrg;
            _audit = audit;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _localizer = localizer;
            _logger = logger;
        }

        // With Features:OrgRoleModel, a user with an active membership may use the panel as well (ROLE-MODEL P1)
        private async Task<bool> IsMemberAsync(string userId) =>
            _currentOrg.Enabled && await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(
                _context.Memberships, m => m.UserId == userId && m.Status == Models.Entities.MembershipStatus.Active);

        // D-78 (O-15): an active beekeeper account, while Features:BeekeeperAccounts is on (it opens only the beekeeper area /Me)
        private async Task<bool> IsBeekeeperAsync(string userId) =>
            HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Options.IOptionsSnapshot<FeatureFlags>>().Value.BeekeeperAccounts
            && await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(
                _context.BeekeeperAccounts, a => a.UserId == userId && a.Status == Models.Entities.BeekeeperAccountStatus.Active);

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole(PanelRole))
                    return RedirectToAction("Index", "Dashboard");
                if (await _currentOrg.GetAsync() != null)
                    return RedirectToAction("Index", "OrgWorkspace");
                if (_userManager.GetUserId(User) is string signedIn && await IsBeekeeperAsync(signedIn))
                    return RedirectToAction("Index", "Me");

                // A non-admin session (e.g. from before D-01) would loop via AccessDenied.
                // Redirect after signing out so the form's anti-forgery token is issued anonymously.
                await _signInManager.SignOutAsync();
                TempData[LoginErrorKey] = _localizer["Login.NoPanelAccess"].Value;
                return RedirectToAction(nameof(Login));
            }
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.UserName,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                // D-78: an expired temporary password does not open a session
                if (user != null && await _credentials.PendingAsync(user.Id) is { Expired: true })
                {
                    await _signInManager.SignOutAsync();
                    ModelState.AddModelError("", _localizer["Login.TemporaryExpired"]);
                    return View(model);
                }
                if (user != null && await _userManager.IsInRoleAsync(user, PanelRole))
                {
                    await _audit.WriteAsync("Login", "AspNetUsers", user.Id, userId: user.Id, userName: user.UserName);
                    if (Url.IsLocalUrl(returnUrl))
                        return LocalRedirect(returnUrl!);   // only within the site; anything else goes to the start page
                    return _currentOrg.Enabled ? RedirectToAction("Index", "MyWork") : RedirectToAction("Index", "Dashboard");
                }
                if (user != null && await IsMemberAsync(user.Id))
                {
                    await _audit.WriteAsync("Login", "AspNetUsers", user.Id, userId: user.Id, userName: user.UserName);
                    if (Url.IsLocalUrl(returnUrl))
                        return LocalRedirect(returnUrl!);
                    return RedirectToAction("Index", "MyWork");   // D-44
                }

                if (user != null && await IsBeekeeperAsync(user.Id))
                {
                    await _audit.WriteAsync("Login", "AspNetUsers", user.Id, userId: user.Id, userName: user.UserName);
                    return RedirectToAction("Index", "Me");
                }

                // Valid credentials, but no access to the panel: don't keep the session
                await _signInManager.SignOutAsync();
                ModelState.AddModelError("", _localizer["Login.NoPanelAccess"]);
                return View(model);
            }

            // Same message for a wrong password and a locked-out account, so it does not reveal
            // which user names exist or are locked (non-admin accounts are locked, D-09)
            ModelState.AddModelError("", _localizer["Login.Invalid"]);
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            // D-78 (D78-SPEC 4.10): e-mail stays disconnected; the reset goes by SMS code to a verified phone
            return ResetBySms ? View("ForgotPasswordSms") : View();
        }

        // Same answer whether or not the account exists or has a verified phone (no account enumeration)
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendResetCode(string? userName)
        {
            userName = userName?.Trim();
            if (string.IsNullOrEmpty(userName))
            {
                TempData["Error"] = "Enter your user name.";
                return RedirectToAction(nameof(ForgotPassword));
            }
            var user = await _userManager.FindByNameAsync(userName);
            if (user != null && user.PhoneNumberConfirmed && !string.IsNullOrEmpty(user.PhoneNumber))
            {
                var sent = await _messages.SendCodeAsync(new TOP4HoneyChains.Web.Services.Messaging.CodeRequest(
                    Models.Entities.SmsPurpose.PasswordReset, "User", user.Id, user.PhoneNumber, NoticeService.Language,
                    ResetContent(user.Id), new Dictionary<string, string>()));
                if (!sent.Ok)
                    _logger.LogInformation("Password reset code not sent for {UserId}: {Error}", user.Id, sent.Error);
            }
            TempData["Success"] = "If the account exists and has a verified mobile phone, a code was sent by SMS.";
            return RedirectToAction(nameof(ResetWithCode), new { userName });
        }

        private static string ResetContent(string userId) => TOP4HoneyChains.Web.Services.Messaging.CodeRules.ContentHash("reset|" + userId);

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetWithCode(string? userName)
        {
            ViewBag.UserName = userName;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetWithCode(string? userName, string? code, string? password, string? confirmPassword)
        {
            ViewBag.UserName = userName;
            if (string.IsNullOrEmpty(password) || password != confirmPassword)
            {
                ModelState.AddModelError("", _localizer["Enter the new password twice."]);
                return View();
            }
            var user = string.IsNullOrWhiteSpace(userName) ? null : await _userManager.FindByNameAsync(userName.Trim());
            if (user == null)
            {
                ModelState.AddModelError("", _localizer["No valid code. Send a new code."]);
                return View();
            }
            var verified = await _messages.VerifyCodeAsync(Models.Entities.SmsPurpose.PasswordReset, "User", user.Id, code, ResetContent(user.Id), user.Id);
            if (!verified.Ok)
            {
                ModelState.AddModelError("", verified.Error!);
                return View();
            }
            var result = await _userManager.ResetPasswordAsync(user, await _userManager.GeneratePasswordResetTokenAsync(user), password);
            if (!result.Succeeded)
            {
                // the code is used; the user asks for a new one and chooses a valid password
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);
                ModelState.AddModelError("", _localizer["No valid code. Send a new code."]);
                return View();
            }
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);
            await _credentials.CompleteAsync(user);
            await _audit.WriteAsync("PasswordReset", "AspNetUsers", user.Id, null, new { Method = "SmsCode" }, userId: user.Id, userName: user.UserName);
            TempData[LoginSuccessKey] = _localizer["ResetPassword.Success"].Value;
            return RedirectToAction(nameof(Login));
        }

        // ---------------------------------------------------------------- D-78: forced change of an issued password (D78-SPEC 4.3 step 4)

        [HttpGet]
        public async Task<IActionResult> FirstPassword()
        {
            if (await _credentials.PendingAsync(_userManager.GetUserId(User)!) == null)
                return RedirectToAction(nameof(Login));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FirstPassword(string? password, string? confirmPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            var pending = user == null ? null : await _credentials.PendingAsync(user.Id);
            if (user == null || pending == null)
                return RedirectToAction(nameof(Login));
            if (pending.Expired)
            {
                await _signInManager.SignOutAsync();
                TempData[LoginErrorKey] = _localizer["Login.TemporaryExpired"].Value;
                return RedirectToAction(nameof(Login));
            }
            if (string.IsNullOrEmpty(password) || password != confirmPassword)
            {
                ModelState.AddModelError("", _localizer["Enter the new password twice."]);
                return View();
            }
            var result = await _userManager.ResetPasswordAsync(user, await _userManager.GeneratePasswordResetTokenAsync(user), password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);
                return View();
            }
            await _credentials.CompleteAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "Your new password is set.";
            return await IsBeekeeperAsync(user.Id) ? RedirectToAction("Index", "Me") : LocalRedirect("/MyWork/Index");
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

            var resetLink = PublicUrl.Build(HttpContext, Url.Action("ResetPassword", "Account", new { email = model.Email, token = token })!);
            if (resetLink == null)
            {
                _logger.LogError("App:PublicBaseUrl is not configured; password reset e-mail not sent");
                ModelState.AddModelError("", _localizer["ForgotPassword.SendFailed"]);
                return View(model);
            }

            // D-51: the e-mail is written in the language selected when the reset was requested
            var mailBody = $@"<p>{System.Net.WebUtility.HtmlEncode(_localizer["Hello,"])}</p>
                              <p>{System.Net.WebUtility.HtmlEncode(_localizer["You requested a password reset."])}</p>
                              <p><a href='{resetLink}'>{System.Net.WebUtility.HtmlEncode(_localizer["Click here to reset your password"])}</a></p>
                              <p>{System.Net.WebUtility.HtmlEncode(_localizer["If you did not request this, please ignore this email."])}</p>
                              <br />
                              <p>TOP4HoneyChains</p>";

            try
            {
                await _emailService.SendAsync(model.Email, _localizer["Reset your TOP4HoneyChains password"], mailBody);
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

        [Authorize(Policy = AuthPolicies.SignedIn)]
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
            if (User.Identity?.IsAuthenticated == true && !User.IsInRole(PanelRole) && await _currentOrg.GetAsync() == null
                && !(_userManager.GetUserId(User) is string uid && await IsBeekeeperAsync(uid)))
            {
                await _signInManager.SignOutAsync();
                TempData[LoginErrorKey] = _localizer["Login.NoPanelAccess"].Value;
                return RedirectToAction(nameof(Login));
            }

            return View();
        }

        // D-75: the lock screen was removed: it hid the page but did not lock the session. Old links go to My work.
        [Authorize(Policy = AuthPolicies.SignedIn)]
        [HttpGet]
        public IActionResult LockScreen() => LocalRedirect("/MyWork/Index");

        [Authorize(Policy = AuthPolicies.SignedIn)]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            var model = new ProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                PhoneConfirmed = user.PhoneNumberConfirmed
            };


            ViewBag.Controller = "Account";
            ViewBag.Action = "My Profile";

            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = AuthPolicies.SignedIn)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePhone(ProfileViewModel model)
        {
            // D-78: a new number is used only after the code sent to it is entered (VerifyPhone)
            var user = await _userManager.GetUserAsync(User);
            var (e164, error) = _messages.CheckPhone(model.PhoneNumber);
            if (user == null || e164 == null)
            {
                TempData["Error"] = error ?? "A mobile phone number is required.";
                return RedirectToAction("Profile");
            }
            var sent = await _messages.SendCodeAsync(new TOP4HoneyChains.Web.Services.Messaging.CodeRequest(
                Models.Entities.SmsPurpose.PhoneVerify, "User", user.Id, e164, NoticeService.Language,
                PhoneContent(user.Id, e164), new Dictionary<string, string>(), user.Id));
            if (!sent.Ok)
            {
                TempData["Error"] = sent.Error;
                return RedirectToAction("Profile");
            }
            TempData["PendingPhone"] = e164;
            TempData["Success"] = "A code was sent by SMS. Enter it below.";
            return RedirectToAction("Profile");
        }

        private static string PhoneContent(string userId, string e164) =>
            TOP4HoneyChains.Web.Services.Messaging.CodeRules.ContentHash("phone|" + userId + "|" + e164);

        [HttpPost]
        [Authorize(Policy = AuthPolicies.SignedIn)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhone(string? phone, string? code)
        {
            var user = await _userManager.GetUserAsync(User);
            var (e164, error) = _messages.CheckPhone(phone);
            if (user == null || e164 == null)
            {
                TempData["Error"] = error ?? "A mobile phone number is required.";
                return RedirectToAction("Profile");
            }
            var verified = await _messages.VerifyCodeAsync(Models.Entities.SmsPurpose.PhoneVerify, "User", user.Id, code, PhoneContent(user.Id, e164), user.Id);
            if (!verified.Ok)
            {
                TempData["Error"] = verified.Error;
                TempData["PendingPhone"] = e164;
                return RedirectToAction("Profile");
            }
            user.PhoneNumber = e164;
            user.PhoneNumberConfirmed = true;
            await _userManager.UpdateAsync(user);
            await _credentials.MarkPhoneVerifiedAsync(user.Id, verified.CodeId);
            TempData["Success"] = "Phone number verified.";
            return RedirectToAction("Profile");
        }

        // D-78 P4 (D78-SPEC 7.2): the explanatory scenes on or off, stored per user (UserSettings.ShowAnimations)
        [HttpPost]
        [Authorize(Policy = AuthPolicies.SignedIn)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Animations(bool show, string? returnUrl)
        {
            await HttpContext.RequestServices.GetRequiredService<Services.UiPreferences>().SetShowAnimationsAsync(_userManager.GetUserId(User)!, show);
            return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
        }

        [HttpPost]
        [Authorize(Policy = AuthPolicies.SignedIn)]
        [ValidateAntiForgeryToken]
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
                await _credentials.CompleteAsync(user);
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
