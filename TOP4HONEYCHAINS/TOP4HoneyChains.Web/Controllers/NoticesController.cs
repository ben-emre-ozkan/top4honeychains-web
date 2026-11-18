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
    // D-78 P1 (D78-SPEC 4.9, 6.1-6.2): the information notices. Signed-in users accept the current versions (click, or an SMS
    // code for the signing roles, O-12); the public page shows the texts (the link in the registration SMS); platform
    // administrators publish new versions. Texts are versioned and never edited in place.
    [Authorize(Policy = AuthPolicies.SignedIn)]
    public class NoticesController : Controller
    {
        public const string PhoneKey = "NoticePhone";

        // Orgs: the organizations the user works for; the platform notice names them as data controllers of their records
        public record AcceptModel(List<PendingNotice> Pending, bool NeedsSms, string? Phone, string? ReturnUrl, Organization? Platform, bool CodeSent, List<Organization> Orgs);
        public record PublicModel(Notice? Notice, List<Notice> Consents, Organization? Org, Organization? Platform, string Audience);

        private readonly TOP4HoneyChainsContext _context;
        private readonly NoticeService _notices;
        private readonly IMessageService _messages;
        private readonly UserManager<ApplicationUser> _users;

        public NoticesController(TOP4HoneyChainsContext context, NoticeService notices, IMessageService messages, UserManager<ApplicationUser> users)
        {
            _context = context;
            _notices = notices;
            _messages = messages;
            _users = users;
        }

        private string UserId => _users.GetUserId(User)!;

        private Task<Organization?> PlatformAsync() => _context.Organizations.AsNoTracking().FirstOrDefaultAsync(o => o.IsPlatform);

        private IActionResult Back(string? returnUrl) =>
            Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : LocalRedirect("/MyWork/Index");

        // ---------------------------------------------------------------- accept (every signed-in user)

        [HttpGet]
        public async Task<IActionResult> Accept(string? returnUrl)
        {
            var pending = await _notices.PendingForUserAsync(UserId);
            var phone = TempData.Peek(PhoneKey) as string ?? (await _users.GetUserAsync(User))?.PhoneNumber;
            var orgs = await _context.Memberships.AsNoTracking().Where(m => m.UserId == UserId && m.Status == MembershipStatus.Active)
                .Select(m => m.Org!).Distinct().OrderBy(o => o.DisplayName).ToListAsync();
            return View(new AcceptModel(pending, await _notices.NeedsSmsAsync(UserId), phone, returnUrl, await PlatformAsync(), TempData.Peek(PhoneKey) != null, orgs));
        }

        // O-12: the signing roles get a code by SMS (M5) that confirms exactly the versions shown
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCode(string? phone, string? returnUrl)
        {
            var pending = await _notices.PendingForUserAsync(UserId);
            if (pending.Count == 0)
                return Back(returnUrl);
            var (e164, error) = _messages.CheckPhone(phone);
            if (e164 == null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Accept), new { returnUrl });
            }
            var first = pending[0];
            var values = new Dictionary<string, string>
            {
                ["org"] = first.Org?.DisplayName ?? (await PlatformAsync())?.DisplayName ?? "TOP4HoneyChains",
                ["version"] = first.Notice.Version
            };
            var sent = await _messages.SendCodeAsync(new CodeRequest(SmsPurpose.NoticeConfirm, "User", UserId, e164, NoticeService.Language,
                NoticeService.ContentHash(UserId, pending), values, UserId));
            if (!sent.Ok)
                TempData["Error"] = sent.Error;
            else
            {
                TempData[PhoneKey] = e164;
                TempData["Success"] = "A code was sent by SMS. Enter it below.";
            }
            return RedirectToAction(nameof(Accept), new { returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Accept")]
        public async Task<IActionResult> AcceptPost(bool read, string? code, string? returnUrl)
        {
            var pending = await _notices.PendingForUserAsync(UserId);
            if (pending.Count == 0)
                return Back(returnUrl);
            if (!read)
            {
                TempData["Error"] = "Tick the box to confirm that you have read the notice.";
                return RedirectToAction(nameof(Accept), new { returnUrl });
            }

            var method = AcceptanceMethod.Click;
            long? codeId = null;
            if (await _notices.NeedsSmsAsync(UserId))
            {
                var verified = await _messages.VerifyCodeAsync(SmsPurpose.NoticeConfirm, "User", UserId, code, NoticeService.ContentHash(UserId, pending), UserId);
                if (!verified.Ok)
                {
                    TempData["Error"] = verified.Error;
                    TempData.Keep(PhoneKey);
                    return RedirectToAction(nameof(Accept), new { returnUrl });
                }
                method = AcceptanceMethod.SmsCode;
                codeId = verified.CodeId;
                // the code proves the phone: it becomes the user's verified number
                if (TempData[PhoneKey] is string phone && await _users.GetUserAsync(User) is ApplicationUser user)
                {
                    user.PhoneNumber = phone;
                    user.PhoneNumberConfirmed = true;
                    await _users.UpdateAsync(user);
                }
            }
            await _notices.AcceptAsync(UserId, pending, method, codeId, HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData["Success"] = "Thank you. Your acceptance of the notice is recorded.";
            return Back(returnUrl);
        }

        // ---------------------------------------------------------------- public texts (link in the registration SMS)

        [AllowAnonymous]
        [HttpGet("/privacy")]
        public async Task<IActionResult> Public(string? @for, int? org)
        {
            var audience = string.Equals(@for, "member", StringComparison.OrdinalIgnoreCase) ? NoticeAudience.Member
                : string.Equals(@for, "consumer", StringComparison.OrdinalIgnoreCase) ? NoticeAudience.Consumer   // D-87
                : NoticeAudience.Beekeeper;
            var notice = await _notices.CurrentAsync(NoticeKind.PrivacyNotice, audience, null, null);
            var consents = new List<Notice>();
            if (audience == NoticeAudience.Beekeeper)
                foreach (var purpose in new[] { ConsentPurpose.PublicName, ConsentPurpose.PortalNotifications })
                    if (await _notices.CurrentAsync(NoticeKind.ConsentText, audience, null, purpose) is Notice c)
                        consents.Add(c);
            var organization = org == null ? null : await _context.Organizations.AsNoTracking().FirstOrDefaultAsync(o => o.OrgId == org && !o.IsPlatform);
            return View(new PublicModel(notice, consents, organization, await PlatformAsync(), audience));
        }

        // ---------------------------------------------------------------- platform administrators

        [Authorize(Policy = AuthPolicies.PlatformAdmin)]
        public async Task<IActionResult> Index()
        {
            var notices = await _context.Notices.AsNoTracking().Include(n => n.Org)
                .OrderBy(n => n.RetiredAt != null).ThenBy(n => n.Kind).ThenBy(n => n.Audience).ThenBy(n => n.OrgId).ThenBy(n => n.Purpose)
                .ThenByDescending(n => n.PublishedAt).ThenBy(n => n.Language)
                .ToListAsync();
            ViewBag.Accepted = await _context.NoticeAcceptances.GroupBy(a => a.NoticeId).Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);
            ViewBag.Controller = "Notices";
            return View(notices);
        }

        [Authorize(Policy = AuthPolicies.PlatformAdmin)]
        public async Task<IActionResult> Text(int id)
        {
            var notice = await _context.Notices.AsNoTracking().Include(n => n.Org).FirstOrDefaultAsync(n => n.NoticeId == id);
            if (notice == null)
                return NotFound();
            ViewBag.Controller = "Notices";
            return View(notice);
        }

        [Authorize(Policy = AuthPolicies.PlatformAdmin)]
        [HttpGet]
        public async Task<IActionResult> Publish(int? from)
        {
            var source = from == null ? null : await _context.Notices.AsNoTracking().FirstOrDefaultAsync(n => n.NoticeId == from);
            await FillPublishAsync();
            return View(source ?? new Notice { Kind = NoticeKind.PrivacyNotice, Audience = NoticeAudience.Member, Language = NoticeService.Language, Version = "", Body = "", IsDraft = true });
        }

        [Authorize(Policy = AuthPolicies.PlatformAdmin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(string kind, string audience, int? orgId, string? purpose, string? version, string language, string? body, bool isDraft)
        {
            version = version?.Trim();
            body = body?.Replace("\r\n", "\n").Trim();
            purpose = string.IsNullOrWhiteSpace(purpose) ? null : purpose.Trim();
            string? error = null;
            if (!NoticeKind.All.Contains(kind) || !NoticeAudience.Values.Contains(audience) || !SmsTemplates.Languages.Contains(language))
                error = "Choose the kind, the audience and the language.";
            else if (string.IsNullOrEmpty(version) || version.Length > 40 || string.IsNullOrEmpty(body))
                error = "Enter the version (at most 40 characters) and the text.";
            else if (kind == NoticeKind.ConsentText && purpose == null)
                error = "A consent text needs its purpose.";
            else if (orgId != null && !await _context.Organizations.AnyAsync(o => o.OrgId == orgId))
                error = "Choose an organization.";
            else if (await _context.Notices.AnyAsync(n => n.Kind == kind && n.Audience == audience && n.OrgId == orgId && n.Purpose == purpose && n.Version == version && n.Language == language))
                error = "This version already exists in this language. Publish a new version.";
            if (error != null)
            {
                TempData["Error"] = error;
                await FillPublishAsync();
                return View(new Notice { Kind = kind, Audience = audience, OrgId = orgId, Purpose = purpose, Version = version ?? "", Language = language, Body = body ?? "", IsDraft = isDraft });
            }
            var notice = await _notices.PublishAsync(kind, audience, orgId, purpose, version!, language, body!, isDraft, UserId);
            TempData["Success"] = "Notice published; the previous version of this text is retired.";
            return RedirectToAction(nameof(Text), new { id = notice.NoticeId });
        }

        private async Task FillPublishAsync()
        {
            ViewBag.Orgs = await _context.Organizations.AsNoTracking().Where(o => !o.IsPlatform).OrderBy(o => o.DisplayName)
                .Select(o => new { o.OrgId, o.DisplayName }).ToListAsync();
            ViewBag.Controller = "Notices";
        }
    }
}
