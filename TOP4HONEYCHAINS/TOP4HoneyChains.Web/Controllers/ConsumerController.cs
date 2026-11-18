using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.Identity;
using TOP4HoneyChains.Web.Services.Messaging;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // D-87: the consumer area. Sign-up (public), the consumer home, saved honeys, feedback and the account. Every query is filtered
    // by the signed-in consumer; for anyone else the area is not found (404), and with Features:ConsumerAccounts off it does not
    // exist at all (404). The account-kind boundary keeps consumers out of every other page (403).
    [Authorize(Policy = AuthPolicies.SignedIn)]
    public class ConsumerController : Controller
    {
        public const string RateSignUp = "consumer-signup", RateSignIn = "signin", RateFeedback = "consumer-feedback";

        private readonly ConsumerService _consumers;
        private readonly UserManager<ApplicationUser> _users;
        private readonly SignInManager<ApplicationUser> _signIn;
        private readonly IDataProtector _formTime;
        private readonly TimeProvider _clock;
        private ConsumerAccount? _me;

        public ConsumerController(ConsumerService consumers, UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn,
            IDataProtectionProvider protection, TimeProvider clock)
        {
            _consumers = consumers;
            _users = users;
            _signIn = signIn;
            _formTime = protection.CreateProtector("D87.ConsumerSignUp.FormTime");
            _clock = clock;
        }

        private ConsumerAccount Me => _me!;

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var flags = HttpContext.RequestServices.GetRequiredService<IOptionsSnapshot<FeatureFlags>>().Value;
            if (!flags.ConsumerAccounts)
            {
                context.Result = NotFound();
                return;
            }
            var anonymous = context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any();
            if (!anonymous)
            {
                _me = await _consumers.OfAsync(_users.GetUserId(User));
                if (_me == null || _me.Status != ConsumerAccountStatus.Active)
                {
                    context.Result = NotFound();
                    return;
                }
                ViewBag.Consumer = _me;
            }
            await next();
        }

        // ---------------------------------------------------------------- sign-up

        private string FormTime() => _formTime.Protect(_clock.GetUtcNow().UtcTicks.ToString(CultureInfo.InvariantCulture));

        private bool FilledByAPerson(string? formTime)
        {
            try
            {
                var started = new DateTime(long.Parse(_formTime.Unprotect(formTime ?? ""), CultureInfo.InvariantCulture), DateTimeKind.Utc);
                return (_clock.GetUtcNow().UtcDateTime - started).TotalSeconds >= _consumers.Options.MinFillSeconds;
            }
            catch (Exception e) when (e is FormatException or System.Security.Cryptography.CryptographicException or OverflowException or ArgumentException)
            {
                return false;
            }
        }

        private async Task<IActionResult> SignUpView(ConsumerSignUp model, string? returnUrl)
        {
            ViewBag.ReturnUrl = AccountController.IsConsumerReturnUrl(returnUrl) ? returnUrl : null;
            ViewBag.FormTime = FormTime();
            ViewBag.Notice = await _consumers.NoticeAsync();
            ViewBag.Title = HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Localization.IStringLocalizer<SharedResource>>()["Consumer.SignUp.Title"].Value;
            return View("SignUp", model);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> SignUp(string? returnUrl)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction(nameof(Index));
            return await SignUpView(new ConsumerSignUp { Language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName }, returnUrl);
        }

        // Only the fields of ConsumerSignUp bind; a posted role, organization or admin flag has nothing to bind to.
        // Honeypot "website" (hidden from people) and a minimum fill time keep simple bots out; the rate limiter caps the rest.
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(RateSignUp)]
        public async Task<IActionResult> SignUp([Bind(nameof(ConsumerSignUp.UserName), nameof(ConsumerSignUp.Password), nameof(ConsumerSignUp.ConfirmPassword),
                nameof(ConsumerSignUp.Language), nameof(ConsumerSignUp.IsAdult), nameof(ConsumerSignUp.AcceptNotice), nameof(ConsumerSignUp.PhoneNumber))] ConsumerSignUp model,
            [FromForm(Name = "website")] string? website, [FromForm(Name = "formTime")] string? formTime, string? returnUrl)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction(nameof(Index));
            if (!string.IsNullOrEmpty(website) || !FilledByAPerson(formTime))
            {
                ModelState.AddModelError("", "The form was sent too quickly. Check the fields and send it again.");
                return await SignUpView(model, returnUrl);
            }
            var messages = HttpContext.RequestServices.GetRequiredService<IMessageService>();
            string? phone = null;
            if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                var (e164, phoneError) = messages.CheckPhone(model.PhoneNumber);
                if (e164 == null)
                {
                    ModelState.AddModelError(nameof(model.PhoneNumber), phoneError ?? "A mobile phone number is required.");
                    return await SignUpView(model, returnUrl);
                }
                phone = e164;
            }
            var (user, account, errors) = await _consumers.CreateAsync(model, HttpContext.Connection.RemoteIpAddress?.ToString());
            if (user == null)
            {
                foreach (var e in errors)
                    ModelState.AddModelError("", e);
                return await SignUpView(model, returnUrl);
            }
            await _signIn.SignInAsync(user, isPersistent: false);
            Response.Cookies.Append(ConsumerMarker.Name, "1", ConsumerMarker.Options(Request.IsHttps));
            SetCulture(account!.Language);
            if (phone != null && await SendPhoneCodeAsync(user.Id, phone, account.Language))
                return RedirectToAction(nameof(Account));
            TempData["Success"] = "Your consumer account is ready.";
            return AccountController.IsConsumerReturnUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction(nameof(Index));
        }

        private void SetCulture(string language) =>
            Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName, CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(language)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), Path = "/", IsEssential = true });

        // ---------------------------------------------------------------- home (D-87 part 4 fills it)

        public async Task<IActionResult> Index()
        {
            ViewBag.Section = "Home";
            ViewBag.Honeys = await _consumers.HoneysAsync(Me.UserId);
            ViewBag.Feedback = await _consumers.FeedbackOfAsync(Me.UserId);
            return View(Me);
        }

        // ---------------------------------------------------------------- my honeys

        // The consumer's own state for a public Honey Story: private, never stored by any cache. Anonymous visitors and other
        // account kinds get "not signed in" (the story script asks only when the consumer marker cookie is present).
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> State(Guid qr)
        {
            Response.Headers.CacheControl = "no-store, private";
            Response.Headers.Vary = "Cookie";
            var me = await _consumers.OfAsync(_users.GetUserId(User));
            if (me == null || me.Status != ConsumerAccountStatus.Active)
                return Json(new { signedIn = false });
            return Json(new { signedIn = true, saved = await _consumers.IsSavedAsync(me.UserId, qr) });
        }

        // saving is explicit: a confirmation page, then a POST (opening a story is never recorded)
        [HttpGet]
        public async Task<IActionResult> Save(Guid qr)
        {
            var batch = await _consumers.PublicBatchAsync(qr);
            if (batch == null)
                return NotFound();
            ViewBag.Section = "Home";
            ViewBag.Saved = await _consumers.IsSavedAsync(Me.UserId, qr);
            return View(batch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Save")]
        public async Task<IActionResult> SavePost(Guid qr)
        {
            if (!await _consumers.SaveAsync(Me.UserId, qr))
                return NotFound();
            TempData["Success"] = "The honey was saved to My honeys.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(Guid qr)
        {
            await _consumers.RemoveAsync(Me.UserId, qr);
            TempData["Success"] = "The honey was removed from My honeys.";
            return RedirectToAction(nameof(Index));
        }

        // ---------------------------------------------------------------- feedback to the producer

        [HttpGet]
        public async Task<IActionResult> Feedback(Guid qr)
        {
            var batch = await _consumers.PublicBatchAsync(qr);
            if (batch == null)
                return NotFound();
            ViewBag.Section = "Home";
            return View(batch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Feedback")]
        [EnableRateLimiting(RateFeedback)]
        public async Task<IActionResult> FeedbackPost(Guid qr, string? category, string? message)
        {
            var (feedback, error) = await _consumers.SendFeedbackAsync(Me, qr, category, message);
            if (feedback == null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Feedback), new { qr });
            }
            TempData["Success"] = "Your feedback was sent to the producer.";
            return RedirectToAction(nameof(Index));
        }

        // ---------------------------------------------------------------- account

        public async Task<IActionResult> Account()
        {
            ViewBag.Section = "Account";
            var user = (await _users.FindByIdAsync(Me.UserId))!;
            ViewBag.Phone = user.PhoneNumberConfirmed ? user.PhoneNumber : null;
            ViewBag.UserName = user.UserName;
            ViewBag.PendingPhone = TempData["PendingPhone"] as string;
            ViewBag.ShowAnimations = await HttpContext.RequestServices.GetRequiredService<Services.UiPreferences>().ShowAnimationsAsync();
            return View(Me);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Password(string? currentPassword, string? newPassword, string? confirmPassword)
        {
            var user = (await _users.FindByIdAsync(Me.UserId))!;
            if (string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
                TempData["Error"] = "The new passwords do not match.";
            else if ((await _users.ChangePasswordAsync(user, currentPassword ?? "", newPassword)) is { Succeeded: false } result)
                TempData["Error"] = result.Errors.First().Description;
            else
            {
                await _signIn.RefreshSignInAsync(user);
                TempData["Success"] = "Password changed.";
            }
            return RedirectToAction(nameof(Account));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Language(string? language)
        {
            if (language == null || !SmsTemplates.Languages.Contains(language))
                return RedirectToAction(nameof(Account));
            var context = HttpContext.RequestServices.GetRequiredService<Data.TOP4HoneyChainsContext>();
            Me.Language = language;
            var settings = await context.UserSettings.FindAsync(Me.UserId);
            if (settings == null)
                context.UserSettings.Add(new UserSetting { UserId = Me.UserId, Language = language });
            else
                settings.Language = language;
            await context.SaveChangesAsync();
            SetCulture(language);
            return RedirectToAction(nameof(Account));
        }

        private static string PhoneContent(string userId, string e164) => CodeRules.ContentHash("phone|" + userId + "|" + e164);

        private async Task<bool> SendPhoneCodeAsync(string userId, string e164, string language)
        {
            var sent = await HttpContext.RequestServices.GetRequiredService<IMessageService>().SendCodeAsync(new CodeRequest(SmsPurpose.PhoneVerify, "User", userId, e164,
                language, PhoneContent(userId, e164), new Dictionary<string, string>(), userId));
            if (!sent.Ok)
            {
                TempData["Error"] = sent.Error;
                return false;
            }
            TempData["PendingPhone"] = e164;
            TempData["Success"] = "A code was sent by SMS. Enter it below.";
            return true;
        }

        // a number is used only after the code sent to it is entered (the same rule as for members and beekeepers)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Phone(string? phone)
        {
            var (e164, error) = HttpContext.RequestServices.GetRequiredService<IMessageService>().CheckPhone(phone);
            if (e164 == null)
                TempData["Error"] = error ?? "A mobile phone number is required.";
            else
                await SendPhoneCodeAsync(Me.UserId, e164, Me.Language);
            return RedirectToAction(nameof(Account));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhone(string? phone, string? code)
        {
            var messages = HttpContext.RequestServices.GetRequiredService<IMessageService>();
            var (e164, error) = messages.CheckPhone(phone);
            if (e164 == null)
            {
                TempData["Error"] = error ?? "A mobile phone number is required.";
                return RedirectToAction(nameof(Account));
            }
            var verified = await messages.VerifyCodeAsync(SmsPurpose.PhoneVerify, "User", Me.UserId, code, PhoneContent(Me.UserId, e164), Me.UserId);
            if (!verified.Ok)
            {
                TempData["Error"] = verified.Error;
                TempData["PendingPhone"] = e164;
                return RedirectToAction(nameof(Account));
            }
            var user = (await _users.FindByIdAsync(Me.UserId))!;
            user.PhoneNumber = e164;
            user.PhoneNumberConfirmed = true;
            await _users.UpdateAsync(user);
            await HttpContext.RequestServices.GetRequiredService<CredentialService>().MarkPhoneVerifiedAsync(Me.UserId, verified.CodeId);
            TempData["Success"] = "Phone number verified.";
            return RedirectToAction(nameof(Account));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePhone()
        {
            var user = (await _users.FindByIdAsync(Me.UserId))!;
            user.PhoneNumber = null;
            user.PhoneNumberConfirmed = false;
            await _users.UpdateAsync(user);
            var context = HttpContext.RequestServices.GetRequiredService<Data.TOP4HoneyChainsContext>();
            if (await context.UserSettings.FindAsync(Me.UserId) is { } settings)
            {
                settings.PhoneVerifiedAt = null;
                settings.PhoneCodeId = null;
                await context.SaveChangesAsync();
            }
            TempData["Success"] = "The phone number was removed. Without it a forgotten password cannot be recovered.";
            return RedirectToAction(nameof(Account));
        }

        // my data: the account, the saved honeys and the feedback with the answers, as JSON
        public async Task<IActionResult> Download()
        {
            var data = await _consumers.ExportAsync(Me.UserId);
            Response.Headers.CacheControl = "no-store";
            return File(JsonSerializer.SerializeToUtf8Bytes(data, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }), "application/json",
                $"top4honeychains-my-data-{Me.Pseudonym}.json");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string? password)
        {
            var user = (await _users.FindByIdAsync(Me.UserId))!;
            if (string.IsNullOrEmpty(password) || !await _users.CheckPasswordAsync(user, password))
            {
                TempData["Error"] = "The password is not correct; the account was not deleted.";
                return RedirectToAction(nameof(Account));
            }
            await _consumers.DeleteAsync(Me.UserId, null);
            await _signIn.SignOutAsync();
            Response.Cookies.Delete(ConsumerMarker.Name);
            TempData[AccountController.LoginSuccessKey] = HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Localization.IStringLocalizer<SharedResource>>()["Consumer.Deleted"].Value;
            return RedirectToAction("Login", "Account");
        }
    }
}
