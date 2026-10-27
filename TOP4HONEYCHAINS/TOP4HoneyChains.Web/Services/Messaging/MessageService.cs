using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.Messaging
{
    // D-78 P1 (D78-SPEC 5.2): the fixed one-time code rules
    public static class CodeRules
    {
        public const int Digits = 6;
        public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);
        public const int MaxAttempts = 3;
        public static readonly TimeSpan ResendAfter = TimeSpan.FromSeconds(60);
        public const int PerSubjectPerHour = 3;     // also "3 password-reset codes per account per hour"
        public const int PerPhonePerHour = 5;
        public const int PerPhonePerDay = 10;
        public const int PerSenderPerHour = 30;
        public const int FailedPerSenderPerHour = 10;
        public static readonly TimeSpan SenderPause = TimeSpan.FromMinutes(15);

        // purposes still sent after the platform's daily cap is reached
        public static readonly string[] AfterDailyLimit = { SmsPurpose.PasswordReset, SmsPurpose.IntakeConfirm };

        public static string NewCode()
        {
            var n = RandomNumberGenerator.GetInt32(0, 1_000_000);
            return n.ToString("D6", System.Globalization.CultureInfo.InvariantCulture);
        }

        public static bool IsWellFormed(string? code) => code != null && code.Length == Digits && code.All(char.IsAsciiDigit);

        public static string Hash(byte[] key, byte[] salt, string code) =>
            Convert.ToHexString(HMACSHA256.HashData(key, salt.Concat(Encoding.UTF8.GetBytes(code)).ToArray())).ToLowerInvariant();

        public static bool Matches(byte[] key, byte[] salt, string code, string storedHash) =>
            CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(Hash(key, salt, code)), Encoding.ASCII.GetBytes(storedHash));

        public static string PhoneHash(byte[] key, string e164) =>
            Convert.ToHexString(HMACSHA256.HashData(key, Encoding.UTF8.GetBytes("phone:" + e164))).ToLowerInvariant();

        // SHA-256 of the canonical text of what a person confirms with a code
        public static string ContentHash(string canonical) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();

        // The stored body: with a real provider every secret in the text is replaced by ******
        public static string Masked(string text, IEnumerable<string> secrets) =>
            secrets.Where(s => !string.IsNullOrEmpty(s)).Aggregate(text, (t, s) => t.Replace(s, "******"));
    }

    public record SendResult(bool Ok, string? Error, long? SmsId = null, long? CodeId = null);
    public record VerifyResult(bool Ok, string? Error, long? CodeId = null);

    public record CodeRequest(
        string Purpose, string SubjectType, string SubjectKey, string Phone, string? Language, string ContentSha256,
        IReadOnlyDictionary<string, string> Values, string? SenderUserId = null, int? OrgId = null);

    public interface IMessageService
    {
        // sends a code message; the code is bound to the purpose, the subject and the content hash
        Task<SendResult> SendCodeAsync(CodeRequest request);
        Task<VerifyResult> VerifyCodeAsync(string purpose, string subjectType, string subjectKey, string? code, string contentSha256, string? userId);
        // a message without a code; secrets (a temporary password) are masked in the stored body with a real provider
        Task<SendResult> SendTextAsync(string purpose, string? subjectType, string? subjectKey, string phone, string? language,
            IReadOnlyDictionary<string, string> values, IEnumerable<string>? secrets = null, string? senderUserId = null, int? orgId = null);
        // the number in E.164 when it may receive SMS, or the error message
        (string? E164, string? Error) CheckPhone(string? phone);
        bool DailyLimitReached();
    }

    // Renders the 5.5 templates, applies the 5.2 rules, writes SmsMessages / OneTimeCodes and calls the configured gateway.
    // Messages returned to the caller are English and translated at display time (MessageTranslator).
    public class MessageService : IMessageService
    {
        private readonly TOP4HoneyChainsContext _db;
        private readonly SmsGatewayFactory _gateways;
        private readonly SmsOptions _options;
        private readonly TimeProvider _clock;
        private readonly ILogger<MessageService> _logger;

        public MessageService(TOP4HoneyChainsContext db, SmsGatewayFactory gateways, IOptions<SmsOptions> options, TimeProvider clock, ILogger<MessageService> logger)
        {
            _db = db;
            _gateways = gateways;
            _options = options.Value;
            _clock = clock;
            _logger = logger;
        }

        private DateTime Now => _clock.GetUtcNow().UtcDateTime;
        private byte[] Key => _options.KeyBytes();

        public (string? E164, string? Error) CheckPhone(string? phone)
        {
            var e164 = PhoneNumberPolicy.Normalize(phone, out var error);
            if (e164 == null)
                return (null, error ?? "A mobile phone number is required.");
            try
            {
                var util = PhoneNumbers.PhoneNumberUtil.GetInstance();
                var region = util.GetRegionCodeForNumber(util.Parse(e164, null));
                if (!_options.AllowedCountrySet.Contains(region ?? ""))
                    return (null, "SMS to numbers outside the allowed countries is not enabled.");
            }
            catch (PhoneNumbers.NumberParseException)
            {
                return (null, PhoneNumberPolicy.InvalidMessage);
            }
            return (e164, null);
        }

        private int SegmentsToday()
        {
            var day = Now.Date;
            return _db.SmsMessages.Where(m => m.CreatedAt >= day && m.Status != SmsStatus.Failed).Sum(m => (int?)m.Segments) ?? 0;
        }

        public bool DailyLimitReached() => SegmentsToday() >= _options.DailyLimit;

        private string? DailyLimitError(string purpose, int segments)
        {
            if (SegmentsToday() + segments <= _options.DailyLimit || CodeRules.AfterDailyLimit.Contains(purpose))
                return null;
            _logger.LogWarning("SMS daily limit of {Limit} segments reached; a {Purpose} message was not sent", _options.DailyLimit, purpose);
            return "The daily SMS limit of the platform is reached. Try again tomorrow or use the signed-photo path.";
        }

        public async Task<SendResult> SendCodeAsync(CodeRequest r)
        {
            var (e164, phoneError) = CheckPhone(r.Phone);
            if (e164 == null)
                return new SendResult(false, phoneError);
            var now = Now;
            var phoneHash = CodeRules.PhoneHash(Key, e164);

            // rate limits (D78-SPEC 5.2)
            var subjectCodes = await _db.OneTimeCodes.AsNoTracking()
                .Where(c => c.Purpose == r.Purpose && c.SubjectType == r.SubjectType && c.SubjectKey == r.SubjectKey && c.CreatedAt > now.AddHours(-1))
                .Select(c => c.CreatedAt).ToListAsync();
            if (subjectCodes.Count > 0 && now - subjectCodes.Max() < CodeRules.ResendAfter)
                return new SendResult(false, "Wait a minute before sending a new code.");
            if (subjectCodes.Count >= CodeRules.PerSubjectPerHour)
                return new SendResult(false, "Too many codes for this record in the last hour. Try again later.");
            var toPhone = await _db.SmsMessages.AsNoTracking()
                .Where(m => m.ToPhoneHash == phoneHash && m.CreatedAt > now.AddDays(-1) && _db.OneTimeCodes.Any(c => c.SmsId == m.SmsId))
                .Select(m => m.CreatedAt).ToListAsync();
            if (toPhone.Count(t => t > now.AddHours(-1)) >= CodeRules.PerPhonePerHour || toPhone.Count >= CodeRules.PerPhonePerDay)
                return new SendResult(false, "Too many codes to this phone number. Try again later.");
            if (r.SenderUserId != null && await _db.SmsMessages.CountAsync(m => m.SentByUserId == r.SenderUserId && m.CreatedAt > now.AddHours(-1)
                                                                               && _db.OneTimeCodes.Any(c => c.SmsId == m.SmsId)) >= CodeRules.PerSenderPerHour)
                return new SendResult(false, "You have sent too many codes in the last hour. Try again later.");

            var code = CodeRules.NewCode();
            var values = new Dictionary<string, string>(r.Values) { ["code"] = code };
            var text = SmsTemplates.Render(r.Purpose, r.Language, values);
            var segments = SmsSegments.Count(text);
            if (DailyLimitError(r.Purpose, segments) is string limit)
                return new SendResult(false, limit);

            // a new code voids the earlier codes of the same purpose and subject
            var earlier = await _db.OneTimeCodes
                .Where(c => c.Purpose == r.Purpose && c.SubjectType == r.SubjectType && c.SubjectKey == r.SubjectKey && c.UsedAt == null && c.VoidedAt == null)
                .ToListAsync();
            foreach (var c in earlier)
            {
                c.VoidedAt = now;
                c.VoidReason = CodeVoidReason.Resent;
            }

            var sms = await SendAsync(r.Purpose, r.SubjectType, r.SubjectKey, e164, phoneHash, r.Language, text, segments, new[] { code }, isCode: true, r.SenderUserId, r.OrgId);
            if (sms.Status == SmsStatus.Failed)
            {
                await _db.SaveChangesAsync();
                return new SendResult(false, "The SMS could not be sent. Try again, or use the signed-photo path.", sms.SmsId);
            }

            var salt = RandomNumberGenerator.GetBytes(16);
            var otp = new OneTimeCode
            {
                SmsId = sms.SmsId, Purpose = r.Purpose, SubjectType = r.SubjectType, SubjectKey = r.SubjectKey,
                ContentSha256 = r.ContentSha256, CodeHash = CodeRules.Hash(Key, salt, code), Salt = salt,
                ExpiresAt = now + CodeRules.Lifetime, CreatedAt = now
            };
            _db.OneTimeCodes.Add(otp);
            await _db.SaveChangesAsync();
            return new SendResult(true, null, sms.SmsId, otp.CodeId);
        }

        public async Task<SendResult> SendTextAsync(string purpose, string? subjectType, string? subjectKey, string phone, string? language,
            IReadOnlyDictionary<string, string> values, IEnumerable<string>? secrets = null, string? senderUserId = null, int? orgId = null)
        {
            var (e164, phoneError) = CheckPhone(phone);
            if (e164 == null)
                return new SendResult(false, phoneError);
            var text = SmsTemplates.Render(purpose, language, values);
            var segments = SmsSegments.Count(text);
            if (DailyLimitError(purpose, segments) is string limit)
                return new SendResult(false, limit);
            var sms = await SendAsync(purpose, subjectType, subjectKey, e164, CodeRules.PhoneHash(Key, e164), language, text, segments,
                secrets ?? Array.Empty<string>(), isCode: false, senderUserId, orgId);
            await _db.SaveChangesAsync();
            return sms.Status == SmsStatus.Failed
                ? new SendResult(false, "The SMS could not be sent. Try again later.", sms.SmsId)
                : new SendResult(true, null, sms.SmsId);
        }

        // stores the message, calls the provider and records its answer (the caller saves)
        private async Task<SmsMessage> SendAsync(string purpose, string? subjectType, string? subjectKey, string e164, string phoneHash, string? language,
            string text, int segments, IEnumerable<string> secrets, bool isCode, string? senderUserId, int? orgId)
        {
            var gateway = _gateways.Current();
            var sms = new SmsMessage
            {
                Purpose = purpose, SubjectType = subjectType, SubjectKey = subjectKey,
                ToPhoneMasked = PhoneMask.Mask(e164), ToPhoneHash = phoneHash, Language = SmsTemplates.Language(language),
                BodyStored = gateway is FakeSmsGateway ? text : CodeRules.Masked(text, secrets),
                Segments = (byte)Math.Min(segments, 255), Provider = gateway.Name, Status = SmsStatus.Queued,
                SentByUserId = senderUserId, OrgId = orgId, CreatedAt = Now
            };
            _db.SmsMessages.Add(sms);
            await _db.SaveChangesAsync();

            var result = await gateway.SendAsync(e164, text, isCode);
            sms.Status = result.Status;
            sms.StatusAt = Now;
            sms.ProviderMessageId = result.ProviderMessageId;
            sms.Error = result.Error?.Length > 400 ? result.Error[..400] : result.Error;
            return sms;
        }

        public async Task<VerifyResult> VerifyCodeAsync(string purpose, string subjectType, string subjectKey, string? code, string contentSha256, string? userId)
        {
            var now = Now;
            if (userId != null)
            {
                // at most 10 failed entries per hour on the codes an officer sent, then a 15-minute pause
                var failures = await _db.OneTimeCodes.AsNoTracking()
                    .Where(c => c.LastAttemptAt > now.AddHours(-1) && c.Attempts > 0 && _db.SmsMessages.Any(m => m.SmsId == c.SmsId && m.SentByUserId == userId))
                    .Select(c => new { c.Attempts, c.LastAttemptAt }).ToListAsync();
                if (failures.Sum(f => f.Attempts) >= CodeRules.FailedPerSenderPerHour && failures.Max(f => f.LastAttemptAt) > now - CodeRules.SenderPause)
                    return new VerifyResult(false, "Too many wrong codes. Wait 15 minutes before trying again.");
            }

            var otp = await _db.OneTimeCodes
                .Where(c => c.Purpose == purpose && c.SubjectType == subjectType && c.SubjectKey == subjectKey && c.UsedAt == null && c.VoidedAt == null)
                .OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync();
            if (otp == null)
                return new VerifyResult(false, "No valid code. Send a new code.");
            if (now > otp.ExpiresAt)
            {
                Void(otp, CodeVoidReason.Expired);
                await _db.SaveChangesAsync();
                return new VerifyResult(false, "The code has expired. Send a new code.");
            }
            if (!string.Equals(otp.ContentSha256, contentSha256, StringComparison.OrdinalIgnoreCase))
            {
                Void(otp, CodeVoidReason.ContentChanged);
                await _db.SaveChangesAsync();
                return new VerifyResult(false, "The details changed after the code was sent. Send a new code.");
            }
            code = code?.Trim();
            if (!CodeRules.IsWellFormed(code) || !CodeRules.Matches(Key, otp.Salt, code!, otp.CodeHash))
            {
                otp.Attempts++;
                otp.LastAttemptAt = now;
                var left = CodeRules.MaxAttempts - otp.Attempts;
                if (left <= 0)
                    Void(otp, CodeVoidReason.TooManyAttempts);
                await _db.SaveChangesAsync();
                return new VerifyResult(false, left <= 0 ? "Wrong code. The code is no longer valid; send a new code." : $"Wrong code. {left} tries left.");
            }
            otp.UsedAt = now;
            otp.UsedByUserId = userId;
            await _db.SaveChangesAsync();
            return new VerifyResult(true, null, otp.CodeId);
        }

        private void Void(OneTimeCode otp, string reason)
        {
            otp.VoidedAt = Now;
            otp.VoidReason = reason;
        }
    }

    // Polls the delivery reports of a real provider for messages of the last two days (D78-SPEC 5.1)
    public class SmsStatusPoller : BackgroundService
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly SmsOptions _options;
        private readonly ILogger<SmsStatusPoller> _logger;

        public SmsStatusPoller(IServiceScopeFactory scopes, IOptions<SmsOptions> options, ILogger<SmsStatusPoller> logger)
        {
            _scopes = scopes;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var real = string.Equals(_options.Provider, SmsProviders.Netgsm, StringComparison.OrdinalIgnoreCase)
                       || string.Equals(_options.Provider, SmsProviders.IletiMerkezi, StringComparison.OrdinalIgnoreCase);
            if (!real || _options.StatusPollMinutes <= 0)
                return;
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_options.StatusPollMinutes));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopes.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<TOP4HoneyChainsContext>();
                    var gateway = scope.ServiceProvider.GetRequiredService<SmsGatewayFactory>().Current();
                    var since = DateTime.UtcNow.AddDays(-2);
                    var open = await db.SmsMessages.Where(m => m.Status == SmsStatus.Sent && m.ProviderMessageId != null && m.CreatedAt > since)
                        .OrderBy(m => m.CreatedAt).Take(100).ToListAsync(stoppingToken);
                    foreach (var m in open)
                    {
                        var status = await gateway.GetStatusAsync(m.ProviderMessageId!, m.CreatedAt, stoppingToken);
                        if (status == null || status.Status == m.Status)
                            continue;
                        m.Status = status.Status;
                        m.StatusAt = DateTime.UtcNow;
                        m.Error = status.Error;
                    }
                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogWarning(ex, "SMS delivery reports could not be updated");
                }
            }
        }
    }
}
