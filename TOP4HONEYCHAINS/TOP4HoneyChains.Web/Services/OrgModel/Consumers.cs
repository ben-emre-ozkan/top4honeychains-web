using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.Identity;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-87: settings of the consumer accounts, section "Consumers"
    public class ConsumerOptions
    {
        // the sign-up form must be open at least this long (bots post at once)
        public int MinFillSeconds { get; set; } = 3;
        public int FeedbackPerBatchPerDay { get; set; } = 1;
        public int FeedbackPerDay { get; set; } = 5;
        // retention (dry run, D-84): accounts after the last sign-in (O-19), feedback after it was closed
        public int AccountRetentionYears { get; set; } = 2;
        public int FeedbackRetentionYears { get; set; } = 3;
        // global ceilings of the rate limiter (requests per minute), the same for every client: behind Cloudflare and IIS the app
        // does not resolve forwarded headers, so it cannot tell clients apart by address (GO-LIVE)
        public int SignUpPerMinute { get; set; } = 10;
        public int SignInPerMinute { get; set; } = 120;
        public int FeedbackPerMinute { get; set; } = 20;
        // per account, on top of the daily limits
        public int FeedbackPerAccountPerMinute { get; set; } = 3;
    }

    // what the sign-up page may set; any other posted field has nothing to bind to
    public class ConsumerSignUp
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public string? Language { get; set; }
        public bool IsAdult { get; set; }
        public bool AcceptNotice { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public record ConsumerHoney(Guid Qr, string Title, string? Plant, DateTime? ProductionDate, DateTime SavedAt);

    public record ConsumerFeedbackView(int FeedbackId, Guid Qr, string BatchTitle, string Category, string Message, DateTime CreatedAt, string Status,
        string? Answer, DateTime? AnsweredAt);

    public class ConsumerService
    {
        public const int MessageLimit = 1000;
        private const string PseudonymAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        private readonly TOP4HoneyChainsContext _context;
        private readonly UserManager<ApplicationUser> _users;
        private readonly NoticeService _notices;
        private readonly AuditWriter _audit;
        private readonly TimeProvider _clock;
        private readonly ConsumerOptions _options;

        public ConsumerService(TOP4HoneyChainsContext context, UserManager<ApplicationUser> users, NoticeService notices, AuditWriter audit,
            TimeProvider clock, IOptionsSnapshot<ConsumerOptions> options)
        {
            _context = context;
            _users = users;
            _notices = notices;
            _audit = audit;
            _clock = clock;
            _options = options.Value;
        }

        public ConsumerOptions Options => _options;
        private DateTime Now => _clock.GetUtcNow().UtcDateTime;

        public Task<ConsumerAccount?> OfAsync(string? userId) => userId == null ? Task.FromResult<ConsumerAccount?>(null)
            : _context.ConsumerAccounts.FirstOrDefaultAsync(c => c.UserId == userId);

        public static string NewPseudonym()
        {
            Span<char> code = stackalloc char[6];
            for (var i = 0; i < code.Length; i++)
                code[i] = PseudonymAlphabet[RandomNumberGenerator.GetInt32(PseudonymAlphabet.Length)];
            return "C-" + new string(code);
        }

        private async Task<string> UniquePseudonymAsync()
        {
            while (true)
            {
                var code = NewPseudonym();
                if (!await _context.ConsumerAccounts.AnyAsync(c => c.Pseudonym == code) && !await _context.ConsumerFeedback.AnyAsync(f => f.Pseudonym == code))
                    return code;
            }
        }

        public Task<Notice?> NoticeAsync(string? language = null) =>
            _notices.CurrentAsync(NoticeKind.PrivacyNotice, NoticeAudience.Consumer, null, null, language);

        // user names: 3 to 40 letters, digits, dot, dash or underscore (no e-mail addresses, no spaces)
        public static bool ValidUserName(string? name) =>
            name != null && name.Length is >= 3 and <= 40 && name.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '-' or '_');

        // Creates the account and returns its errors (English, translated at display time); the caller signs the user in
        public async Task<(ApplicationUser? User, ConsumerAccount? Account, List<string> Errors)> CreateAsync(ConsumerSignUp model, string? ip)
        {
            var errors = new List<string>();
            var userName = model.UserName?.Trim();
            if (!ValidUserName(userName))
                errors.Add("Choose a user name of 3 to 40 letters, digits, dots, dashes or underscores.");
            if (string.IsNullOrEmpty(model.Password) || model.Password != model.ConfirmPassword)
                errors.Add("The new passwords do not match.");
            if (!model.IsAdult)
                errors.Add("Consumer accounts are for people aged 18 or older.");
            if (!model.AcceptNotice)
                errors.Add("Read and accept the privacy notice for consumers.");
            var language = Messaging.SmsTemplates.Languages.Contains(model.Language) ? model.Language! : Messaging.SmsTemplates.DefaultLanguage;
            var notice = await NoticeAsync(language);
            if (notice == null)
                errors.Add("The privacy notice for consumers is not available; sign-up is closed.");
            if (errors.Count > 0)
                return (null, null, errors);

            var user = new ApplicationUser { UserName = userName, Email = null, EmailConfirmed = false, LockoutEnabled = true };
            var created = await _users.CreateAsync(user, model.Password!);
            if (!created.Succeeded)
                return (null, null, created.Errors.Select(e => e.Description).ToList());

            var now = Now;
            var account = new ConsumerAccount
            {
                UserId = user.Id, Pseudonym = await UniquePseudonymAsync(), Language = language, AgeConfirmedAt = now,
                NoticeId = notice!.NoticeId, NoticeVersion = notice.Version, CreatedAt = now, LastSignInAt = now, Status = ConsumerAccountStatus.Active
            };
            _context.ConsumerAccounts.Add(account);
            _context.UserSettings.Add(new UserSetting { UserId = user.Id, Language = language });
            await _context.SaveChangesAsync();
            await _notices.AcceptAsync(user.Id, new[] { new PendingNotice(notice, null) }, AcceptanceMethod.Click, null, ip);
            await _audit.WriteAsync("Create", "ConsumerAccounts", account.Pseudonym, userId: user.Id, userName: account.Pseudonym);
            return (user, account, errors);
        }

        // ---------------------------------------------------------------- my honeys

        // a finalized batch by its public QR id (never by internal id)
        public Task<FinalHoneyProduction?> PublicBatchAsync(Guid qr) =>
            _context.FinalHoneyProductions.AsNoTracking().FirstOrDefaultAsync(f => f.FinalHoneyProductionQR == qr && f.FinalHoneyProductionIsFinalized);

        public async Task<List<ConsumerHoney>> HoneysAsync(string userId)
        {
            var rows = await _context.ConsumerSavedBatches.AsNoTracking().Where(s => s.ConsumerUserId == userId)
                .OrderByDescending(s => s.SavedAt)
                .Select(s => new { s.SavedAt, s.Batch!.FinalHoneyProductionQR, s.Batch.FinalHoneyProductionTitle, s.Batch.FinalHoneyProductionDate,
                    s.Batch.OwnerOrgId, s.Batch.FinalHoneyProductionWholesalerId, Wholesaler = s.Batch.FinalHoneyProductionWholesaler!.WholesalerTitle })
                .ToListAsync();
            var orgIds = rows.Where(r => r.OwnerOrgId != null).Select(r => r.OwnerOrgId!.Value).Distinct().ToList();
            var orgs = await _context.Organizations.AsNoTracking().Where(o => orgIds.Contains(o.OrgId)).ToDictionaryAsync(o => o.OrgId, o => o.DisplayName);
            return rows.Select(r => new ConsumerHoney(r.FinalHoneyProductionQR, r.FinalHoneyProductionTitle ?? "",
                r.OwnerOrgId is int id && orgs.TryGetValue(id, out var name) ? name : r.Wholesaler, r.FinalHoneyProductionDate, r.SavedAt)).ToList();
        }

        public async Task<bool> IsSavedAsync(string userId, Guid qr) =>
            await _context.ConsumerSavedBatches.AnyAsync(s => s.ConsumerUserId == userId && s.Batch!.FinalHoneyProductionQR == qr);

        // false when the batch is not public
        public async Task<bool> SaveAsync(string userId, Guid qr)
        {
            var batch = await PublicBatchAsync(qr);
            if (batch == null)
                return false;
            if (!await _context.ConsumerSavedBatches.AnyAsync(s => s.ConsumerUserId == userId && s.FinalHoneyProductionId == batch.FinalHoneyProductionId))
            {
                _context.ConsumerSavedBatches.Add(new ConsumerSavedBatch { ConsumerUserId = userId, FinalHoneyProductionId = batch.FinalHoneyProductionId, SavedAt = Now });
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task RemoveAsync(string userId, Guid qr)
        {
            var rows = await _context.ConsumerSavedBatches.Where(s => s.ConsumerUserId == userId && s.Batch!.FinalHoneyProductionQR == qr).ToListAsync();
            _context.ConsumerSavedBatches.RemoveRange(rows);
            await _context.SaveChangesAsync();
        }

        // ---------------------------------------------------------------- feedback

        // the organization that owns the batch (the same rule as the custody model: owner, else the legacy wholesaler's organization)
        public Task<int?> OwnerOrgAsync(int batchId) =>
            _context.FinalHoneyProductions.AsNoTracking().Where(f => f.FinalHoneyProductionId == batchId)
                .Select(f => f.OwnerOrgId ?? _context.Organizations.Where(o => o.LegacyWholesalerId == f.FinalHoneyProductionWholesalerId).Select(o => (int?)o.OrgId).FirstOrDefault())
                .FirstOrDefaultAsync();

        // English error (translated at display time) or null when the feedback was recorded
        public async Task<(ConsumerFeedback? Feedback, string? Error)> SendFeedbackAsync(ConsumerAccount consumer, Guid qr, string? category, string? message)
        {
            var batch = await PublicBatchAsync(qr);
            if (batch == null)
                return (null, "This honey has no public story.");
            if (category == null || !ConsumerFeedbackCategory.All.Contains(category))
                return (null, "Choose what your feedback is about.");
            message = message?.Trim();
            if (string.IsNullOrEmpty(message))
                return (null, "Write your message.");
            if (message.Length > MessageLimit)
                return (null, "The message is longer than 1000 characters.");
            var dayStart = Now.AddDays(-1);
            var recent = await _context.ConsumerFeedback.AsNoTracking()
                .Where(f => f.ConsumerUserId == consumer.UserId && f.CreatedAt > dayStart).Select(f => f.FinalHoneyProductionId).ToListAsync();
            if (recent.Count(id => id == batch.FinalHoneyProductionId) >= _options.FeedbackPerBatchPerDay)
                return (null, "You have already sent feedback about this honey today.");
            if (recent.Count >= _options.FeedbackPerDay)
                return (null, "You have reached the limit of feedback messages for today.");
            var orgId = await OwnerOrgAsync(batch.FinalHoneyProductionId);
            if (orgId == null)
                return (null, "This honey has no producer on the platform to receive feedback.");

            var feedback = new ConsumerFeedback
            {
                ConsumerUserId = consumer.UserId, Pseudonym = consumer.Pseudonym, FinalHoneyProductionId = batch.FinalHoneyProductionId, OrgId = orgId.Value,
                Category = category, Message = message, Language = consumer.Language, CreatedAt = Now, Status = ConsumerFeedbackStatus.New
            };
            _context.ConsumerFeedback.Add(feedback);
            await _context.SaveChangesAsync();
            await _audit.WriteAsync("Create", "ConsumerFeedback", feedback.FeedbackId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                null, new { feedback.FeedbackId, feedback.OrgId, feedback.FinalHoneyProductionId, feedback.Category }, userId: consumer.UserId, userName: consumer.Pseudonym);
            return (feedback, null);
        }

        public async Task<List<ConsumerFeedbackView>> FeedbackOfAsync(string userId) =>
            (await _context.ConsumerFeedback.AsNoTracking().Where(f => f.ConsumerUserId == userId).OrderByDescending(f => f.CreatedAt)
                .Select(f => new { f.FeedbackId, f.Batch!.FinalHoneyProductionQR, f.Batch.FinalHoneyProductionTitle, f.Category, f.Message, f.CreatedAt, f.Status, f.Answer, f.AnsweredAt })
                .ToListAsync())
            .Select(f => new ConsumerFeedbackView(f.FeedbackId, f.FinalHoneyProductionQR, f.FinalHoneyProductionTitle ?? "", f.Category, f.Message, f.CreatedAt, f.Status, f.Answer, f.AnsweredAt))
            .ToList();

        // ---------------------------------------------------------------- my data and deletion

        public async Task<object> ExportAsync(string userId)
        {
            var user = (await _users.FindByIdAsync(userId))!;
            var account = (await OfAsync(userId))!;
            return new
            {
                exportedAt = Now,
                account = new
                {
                    userName = user.UserName, pseudonym = account.Pseudonym, language = account.Language, phone = user.PhoneNumberConfirmed ? user.PhoneNumber : null,
                    ageConfirmedAt = account.AgeConfirmedAt, noticeVersion = account.NoticeVersion, createdAt = account.CreatedAt, lastSignInAt = account.LastSignInAt
                },
                savedHoneys = (await HoneysAsync(userId)).Select(h => new { qr = h.Qr, title = h.Title, producer = h.Plant, h.ProductionDate, h.SavedAt }),
                feedback = (await FeedbackOfAsync(userId)).Select(f => new { f.FeedbackId, honey = f.Qr, f.BatchTitle, f.Category, f.Message, f.CreatedAt, f.Status, f.Answer, f.AnsweredAt })
            };
        }

        // The consumer row and the saved list are deleted; the sign-in account is anonymised (no name, no phone, no password,
        // locked for good); feedback stays with the organizations under the pseudonym only.
        public async Task DeleteAsync(string userId, string? byUserId)
        {
            var account = await OfAsync(userId);
            if (account == null)
                return;
            await using var tx = await _context.Database.BeginTransactionAsync();
            await _context.ConsumerFeedback.Where(f => f.ConsumerUserId == userId).ExecuteUpdateAsync(s => s.SetProperty(f => f.ConsumerUserId, (string?)null));
            await _context.ConsumerSavedBatches.Where(s => s.ConsumerUserId == userId).ExecuteDeleteAsync();
            await _context.UserSettings.Where(s => s.UserId == userId).ExecuteDeleteAsync();
            _context.ConsumerAccounts.Remove(account);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            var user = await _users.FindByIdAsync(userId);
            if (user != null)
            {
                user.UserName = "deleted-" + account.Pseudonym.ToLowerInvariant();
                user.Email = null;
                user.PhoneNumber = null;
                user.PhoneNumberConfirmed = false;
                user.PasswordHash = null;
                user.TwoFactorEnabled = false;
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;
                await _users.UpdateAsync(user);
                await _users.UpdateSecurityStampAsync(user);
            }
            await _audit.WriteAsync("Delete", "ConsumerAccounts", account.Pseudonym, userId: byUserId ?? userId, userName: byUserId == null ? account.Pseudonym : null);
        }

        public static bool IsDeletedConsumer(string? userName) => userName?.StartsWith("deleted-c-", StringComparison.Ordinal) == true;
    }
}
