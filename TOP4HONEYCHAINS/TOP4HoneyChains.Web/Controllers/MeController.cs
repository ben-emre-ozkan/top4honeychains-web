using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.Identity;
using TOP4HoneyChains.Web.Services.Messaging;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // D-78 P3 (D78-SPEC 2.4, 4.7, O-15, D-82): the beekeeper portal. Read-only screens of the beekeeper's own data; the writes are
    // the asking price of their lots, their consents, phone, language and password. Every query is filtered by the beekeeper of the
    // signed-in account (BeekeeperPortalService); for everyone else the BeekeeperSelf policy fails and the area is not found (404).
    [Authorize(Policy = AuthPolicies.SignedIn)]
    public class MeController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly UserManager<ApplicationUser> _users;
        private readonly BeekeeperPortalService _portal;

        public MeController(TOP4HoneyChainsContext context, UserManager<ApplicationUser> users, BeekeeperPortalService portal)
        {
            _context = context;
            _users = users;
            _portal = portal;
        }

        private string UserId => _users.GetUserId(User)!;

        public override async Task OnActionExecutionAsync(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context, Microsoft.AspNetCore.Mvc.Filters.ActionExecutionDelegate next)
        {
            var allowed = await HttpContext.RequestServices.GetRequiredService<IAuthorizationService>().AuthorizeAsync(User, AuthPolicies.BeekeeperSelf);
            if (!allowed.Succeeded)
            {
                context.Result = NotFound();
                return;
            }
            await next();
        }

        private async Task<Beekeeper> MeAsync(string section)
        {
            var me = (await _portal.BeekeeperOfAsync(UserId))!;   // the policy guarantees an active beekeeper
            ViewBag.Me = me;
            ViewBag.Section = section;
            return me;
        }

        public async Task<IActionResult> Index()
        {
            var me = await MeAsync("Home");
            var deliveries = await _portal.DeliveriesAsync(me.BeekeeperId!);
            var samples = await _portal.SamplesAsync(me.BeekeeperId!);
            ViewBag.Deliveries = deliveries;
            ViewBag.NoPrice = deliveries.Count(d => d.PriceDenied == null && d.Prices.Count == 0);
            ViewBag.LastReport = samples.Where(s => s.ApprovedReport != null).OrderByDescending(s => s.ApprovedReport!.ApprovedAt).FirstOrDefault();
            ViewBag.BatchCount = (await _portal.BatchesAsync(me.BeekeeperId!)).Count;
            ViewBag.PendingNotices = (await HttpContext.RequestServices.GetRequiredService<NoticeService>().PendingForCurrentUserAsync(HttpContext, UserId)).Count;
            return View(me);
        }

        public async Task<IActionResult> Apiaries()
        {
            var me = await MeAsync("Apiaries");
            var apiaries = await _context.Apiaries.AsNoTracking().Where(a => a.ApiaryBeekeeperId == me.BeekeeperId).OrderBy(a => a.ApiaryTitle).ToListAsync();
            var districtIds = apiaries.Where(a => a.DistrictId != null).Select(a => a.DistrictId!.Value).ToList();
            ViewBag.Districts = await _context.Districts.AsNoTracking().Where(d => districtIds.Contains(d.DistrictId)).ToDictionaryAsync(d => d.DistrictId, d => d.Name);
            return View(apiaries);
        }

        public async Task<IActionResult> Deliveries()
        {
            var me = await MeAsync("Deliveries");
            return View(await _portal.DeliveriesAsync(me.BeekeeperId!));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Price(int lotId, decimal? pricePerKg, string? currency, string? note)
        {
            var me = await MeAsync("Deliveries");
            var (found, error) = await _portal.SetPriceAsync(me.BeekeeperId!, UserId, lotId, pricePerKg, currency, note, HttpContext.Connection.RemoteIpAddress?.ToString());
            if (!found)
                return NotFound();
            TempData[error == null ? "Success" : "Error"] = error ?? "Price saved.";
            return Redirect($"/Me/Deliveries#lot-{lotId}");
        }

        public async Task<IActionResult> Samples()
        {
            var me = await MeAsync("Samples");
            return View(await _portal.SamplesAsync(me.BeekeeperId!));
        }

        public async Task<IActionResult> Batches()
        {
            var me = await MeAsync("Batches");
            return View(await _portal.BatchesAsync(me.BeekeeperId!));
        }

        public async Task<IActionResult> Stories()
        {
            var me = await MeAsync("Stories");
            ViewBag.Shown = (await HttpContext.RequestServices.GetRequiredService<PublicNameService>().NamesAsync(new[] { me.BeekeeperId! })).GetValueOrDefault(me.BeekeeperId!);
            return View((await _portal.BatchesAsync(me.BeekeeperId!)).Where(b => b.Batch.FinalHoneyProductionIsFinalized).ToList());
        }

        public async Task<IActionResult> Privacy()
        {
            var me = await MeAsync("Privacy");
            ViewBag.Consents = await _portal.ConsentsAsync(me.BeekeeperId!);
            ViewBag.Notices = await _portal.AcceptedNoticesAsync(me.BeekeeperId!, UserId);
            ViewBag.Controllers = await _portal.ControllersAsync(me.BeekeeperId!);
            ViewBag.Shown = (await HttpContext.RequestServices.GetRequiredService<PublicNameService>().NamesAsync(new[] { me.BeekeeperId! })).GetValueOrDefault(me.BeekeeperId!);
            return View(me);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Consent(string? purpose, bool give)
        {
            var me = await MeAsync("Privacy");
            purpose ??= "";
            if (give)
            {
                var notices = HttpContext.RequestServices.GetRequiredService<NoticeService>();
                var error = await _portal.GiveConsentAsync(me.BeekeeperId!, UserId, purpose,
                    () => notices.CurrentAsync(NoticeKind.ConsentText, NoticeAudience.Beekeeper, null, purpose, me.PreferredLanguage));
                TempData[error == null ? "Success" : "Error"] = error ?? "Consent given.";
            }
            else
            {
                var withdrawn = await _portal.WithdrawConsentAsync(me.BeekeeperId!, UserId, purpose);
                TempData[withdrawn ? "Success" : "Error"] = withdrawn ? "Consent withdrawn; it takes effect at once." : "There is no such consent to withdraw.";
            }
            return RedirectToAction(nameof(Privacy));
        }

        public async Task<IActionResult> Account()
        {
            var me = await MeAsync("Account");
            ViewBag.PendingPhone = TempData["PendingPhone"] as string;
            return View(me);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Password(string? currentPassword, string? newPassword, string? confirmPassword)
        {
            await MeAsync("Account");
            var user = (await _users.FindByIdAsync(UserId))!;
            if (string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
                TempData["Error"] = "The new passwords do not match.";
            else if ((await _users.ChangePasswordAsync(user, currentPassword ?? "", newPassword)) is { Succeeded: false } result)
                TempData["Error"] = result.Errors.First().Description;
            else
            {
                await HttpContext.RequestServices.GetRequiredService<CredentialService>().CompleteAsync(user);
                TempData["Success"] = "Password changed.";
            }
            return RedirectToAction(nameof(Account));
        }

        private static string PhoneContent(string userId, string e164) => CodeRules.ContentHash("phone|" + userId + "|" + e164);

        // a new number is used only after the code sent to it is entered (the same rule as for members)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Phone(string? phone)
        {
            var me = await MeAsync("Account");
            var messages = HttpContext.RequestServices.GetRequiredService<IMessageService>();
            var (e164, error) = messages.CheckPhone(phone);
            if (e164 == null)
            {
                TempData["Error"] = error ?? "A mobile phone number is required.";
                return RedirectToAction(nameof(Account));
            }
            var sent = await messages.SendCodeAsync(new CodeRequest(SmsPurpose.PhoneVerify, "User", UserId, e164, me.PreferredLanguage,
                PhoneContent(UserId, e164), new Dictionary<string, string>(), UserId));
            if (!sent.Ok)
                TempData["Error"] = sent.Error;
            else
            {
                TempData["PendingPhone"] = e164;
                TempData["Success"] = "A code was sent by SMS. Enter it below.";
            }
            return RedirectToAction(nameof(Account));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhone(string? phone, string? code)
        {
            var me = await MeAsync("Account");
            var messages = HttpContext.RequestServices.GetRequiredService<IMessageService>();
            var (e164, error) = messages.CheckPhone(phone);
            if (e164 == null)
            {
                TempData["Error"] = error ?? "A mobile phone number is required.";
                return RedirectToAction(nameof(Account));
            }
            var verified = await messages.VerifyCodeAsync(SmsPurpose.PhoneVerify, "User", UserId, code, PhoneContent(UserId, e164), UserId);
            if (!verified.Ok)
            {
                TempData["Error"] = verified.Error;
                TempData["PendingPhone"] = e164;
                return RedirectToAction(nameof(Account));
            }
            var user = (await _users.FindByIdAsync(UserId))!;
            user.PhoneNumber = e164;
            user.PhoneNumberConfirmed = true;
            await _users.UpdateAsync(user);
            // the beekeeper's number for the delivery and information messages
            var beekeeper = await _context.Beekeepers.FirstAsync(b => b.BeekeeperId == me.BeekeeperId);
            beekeeper.BeekeeperPhoneNumber = e164;
            await _context.SaveChangesAsync();
            await HttpContext.RequestServices.GetRequiredService<CredentialService>().MarkPhoneVerifiedAsync(UserId, verified.CodeId);
            TempData["Success"] = "Phone number verified.";
            return RedirectToAction(nameof(Account));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Language(string? language)
        {
            var me = await MeAsync("Account");
            if (language == null || !SmsTemplates.Languages.Contains(language))
                return RedirectToAction(nameof(Account));
            var beekeeper = await _context.Beekeepers.FirstAsync(b => b.BeekeeperId == me.BeekeeperId);
            beekeeper.PreferredLanguage = language;
            await _context.SaveChangesAsync();
            return RedirectToAction("SetLanguage", "Home", new { culture = language, returnUrl = "/Me/Account" });
        }
    }
}
