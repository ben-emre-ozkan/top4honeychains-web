using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.Messaging;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-78 P1 (D78-SPEC 4.9, 6.1-6.2): settings of the notice gate, section "Notices"
    public class NoticeOptions
    {
        // O-14: while false the gate does not block anyone; users with a pending notice see a banner
        public bool Enforce { get; set; }
        // O-12: roles that confirm a notice with an SMS code; everyone else confirms by click
        public string SmsConfirmRoles { get; set; } = string.Join(",", OrgRoles.Signing);

        public IReadOnlySet<string> SmsRoles =>
            SmsConfirmRoles.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.Ordinal);
    }

    // A notice a user still has to accept, with the organization that publishes it (null = the platform)
    public record PendingNotice(Notice Notice, Organization? Org);

    public class NoticeService
    {
        public const string ItemsKey = "D78.PendingNotices";
        private static readonly string[] FallbackLanguages = { "en", "tr" };

        private readonly TOP4HoneyChainsContext _context;
        private readonly NoticeOptions _options;
        private readonly TimeProvider _clock;

        // options per request (IOptionsSnapshot): a changed Notices:Enforce applies without a restart
        public NoticeService(TOP4HoneyChainsContext context, IOptionsSnapshot<NoticeOptions> options, TimeProvider clock)
        {
            _context = context;
            _options = options.Value;
            _clock = clock;
        }

        public NoticeOptions Options => _options;

        public static string Language => SmsTemplates.Language(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);

        // the key of a version, independent of the language it is shown in
        private static (string, string, int?, string?, string) Key(Notice n) => (n.Kind, n.Audience, n.OrgId, n.Purpose, n.Version);

        // the current (not retired) text in the language, else English, else Turkish, else any language
        public async Task<Notice?> CurrentAsync(string kind, string audience, int? orgId, string? purpose, string? language = null)
        {
            var rows = await _context.Notices.AsNoTracking()
                .Where(n => n.Kind == kind && n.OrgId == orgId && n.Purpose == purpose && n.RetiredAt == null
                            && (n.Audience == audience || n.Audience == NoticeAudience.All))
                .ToListAsync();
            return Pick(rows, language ?? Language);
        }

        private static Notice? Pick(IReadOnlyCollection<Notice> rows, string language)
        {
            foreach (var lang in new[] { language }.Concat(FallbackLanguages))
                if (rows.FirstOrDefault(r => r.Language == lang) is Notice n)
                    return n;
            return rows.OrderByDescending(r => r.PublishedAt).FirstOrDefault();
        }

        // The platform's notice for members and the notice of every organization the user works for, minus the versions
        // already accepted (in any language)
        public async Task<List<PendingNotice>> PendingForUserAsync(string userId, string audience = NoticeAudience.Member)
        {
            var orgIds = await _context.Memberships.AsNoTracking()
                .Where(m => m.UserId == userId && m.Status == MembershipStatus.Active)
                .Select(m => m.OrgId).Distinct().ToListAsync();
            var current = await _context.Notices.AsNoTracking()
                .Where(n => n.Kind == NoticeKind.PrivacyNotice && n.Purpose == null && n.RetiredAt == null
                            && (n.Audience == audience || n.Audience == NoticeAudience.All)
                            && (n.OrgId == null || orgIds.Contains(n.OrgId.Value)))
                .ToListAsync();
            if (current.Count == 0)
                return new();

            var accepted = (await _context.NoticeAcceptances.AsNoTracking().Where(a => a.UserId == userId)
                    .Select(a => new { a.Notice!.Kind, a.Notice.Audience, a.Notice.OrgId, a.Notice.Purpose, a.Notice.Version }).ToListAsync())
                .Select(a => (a.Kind, a.Audience, a.OrgId, a.Purpose, a.Version)).ToHashSet();
            var orgs = await _context.Organizations.AsNoTracking().Where(o => orgIds.Contains(o.OrgId)).ToDictionaryAsync(o => o.OrgId);

            return current.GroupBy(Key)
                .Where(g => !accepted.Contains(g.Key))
                .Select(g => Pick(g.ToList(), Language)!)
                .OrderBy(n => n.OrgId.HasValue).ThenBy(n => n.OrgId)
                .Select(n => new PendingNotice(n, n.OrgId is int id && orgs.TryGetValue(id, out var o) ? o : null))
                .ToList();
        }

        // cached for the request: the gate and the banner ask on every page
        public async Task<List<PendingNotice>> PendingForCurrentUserAsync(HttpContext http, string? userId)
        {
            if (userId == null)
                return new();
            if (http.Items[ItemsKey] is List<PendingNotice> cached)
                return cached;
            var pending = await PendingForUserAsync(userId);
            http.Items[ItemsKey] = pending;
            return pending;
        }

        // O-12: a user holding a signing role confirms with an SMS code
        public async Task<bool> NeedsSmsAsync(string userId)
        {
            var roles = _options.SmsRoles;
            var held = await _context.Memberships.AsNoTracking()
                .Where(m => m.UserId == userId && m.Status == MembershipStatus.Active).Select(m => m.OrgRole).ToListAsync();
            return held.Any(roles.Contains);
        }

        // what the SMS code confirms: these notice versions, for this user
        public static string ContentHash(string userId, IEnumerable<PendingNotice> pending) =>
            CodeRules.ContentHash("notice|" + userId + "|" + string.Join(";", pending.Select(p => p.Notice.NoticeId).OrderBy(i => i)));

        public async Task AcceptAsync(string userId, IEnumerable<PendingNotice> pending, string method, long? codeId, string? ip)
        {
            var now = _clock.GetUtcNow().UtcDateTime;
            foreach (var p in pending)
                _context.NoticeAcceptances.Add(new NoticeAcceptance
                {
                    NoticeId = p.Notice.NoticeId, UserId = userId, OrgId = p.Notice.OrgId, Method = method, CodeId = codeId,
                    RecordedByUserId = userId, At = now, ClientIp = ip?.Length > 45 ? ip[..45] : ip
                });
            await _context.SaveChangesAsync();
        }

        // Publishing a new version retires the current text of the same kind, audience, organization, purpose and language
        public async Task<Notice> PublishAsync(string kind, string audience, int? orgId, string? purpose, string version, string language,
            string body, bool isDraft, string? userId)
        {
            var now = _clock.GetUtcNow().UtcDateTime;
            var previous = await _context.Notices
                .Where(n => n.Kind == kind && n.Audience == audience && n.OrgId == orgId && n.Purpose == purpose && n.Language == language && n.RetiredAt == null)
                .ToListAsync();
            foreach (var p in previous)
                p.RetiredAt = now;
            var notice = new Notice
            {
                Kind = kind, Audience = audience, OrgId = orgId, Purpose = purpose, Version = version, Language = language,
                Body = body, BodySha256 = Notice.Hash(body), IsDraft = isDraft, PublishedAt = now, PublishedByUserId = userId
            };
            _context.Notices.Add(notice);
            await _context.SaveChangesAsync();
            return notice;
        }
    }
}
