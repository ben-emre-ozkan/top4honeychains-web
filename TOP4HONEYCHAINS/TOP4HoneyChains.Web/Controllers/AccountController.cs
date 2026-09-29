using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TOP4HoneyChains.Web.Models.Identity;
using TOP4HoneyChains.Web.Models.ViewModels;
using TOP4HoneyChains.Web.Services;

namespace TOP4HoneyChains.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailService = emailService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
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
                return RedirectToAction("Index", "Dashboard");

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Role yoksa oluştur
                if (!await _roleManager.RoleExistsAsync(model.Role))
                    await _roleManager.CreateAsync(new IdentityRole(model.Role));

                await _userManager.AddToRoleAsync(user, model.Role);
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

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

            if (user == null)
            {
                ViewBag.Message =
                    "If an account with this email exists, password reset instructions have been sent.";
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

            await _emailService.SendAsync(model.Email, "Reset your TOP4HoneyChains password", mailBody);

            ViewBag.Message = "Password reset instructions have been sent to your email.";

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
                ViewBag.Message = "Your password has been reset successfully.";
                return RedirectToAction("Login");
            }

            var result = await _userManager.ResetPasswordAsync(
                user,
                model.Token,
                model.Password);

            if (result.Succeeded)
            {
                ViewBag.Message = "Your password has been reset successfully.";
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
        public IActionResult AccessDenied()
        {
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
