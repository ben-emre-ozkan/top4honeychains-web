using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Data;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-78 P5 (D78-SPEC 6.3, D-84): the retention job. It is OFF by default: ApplyEnabled lets a platform administrator run it
    // from the Retention page (the first run is the owner's decision, it changes data); Schedule adds a monthly run, and only
    // after a first run by hand. The dry run changes nothing and is always available.
    public class RetentionOptions
    {
        public bool ApplyEnabled { get; set; }
        public bool Schedule { get; set; }
        public int OneTimeCodeDays { get; set; } = 30;
        public int SmsYears { get; set; } = 1;
        public int PriceYears { get; set; } = 5;
        public int ProductYears { get; set; } = 5;
        // batches carry no best-before date: the batch date plus this many years stands in for it (D-84)
        public int BestBeforeYears { get; set; } = 2;
        public int ProofYears { get; set; } = 3;
        public int FormerMemberYears { get; set; } = 2;
        public int AuditYears { get; set; } = 10;
    }

    public class RetentionService
    {
        public const string SessionKey = "top4.retention";
        public const string AuditAction = "Retention";

        // Applies: the job changes these rows when it runs; the other rules are counted only, their change waits for decision O-14
        public record Rule(string Key, bool Applies, DateTime Cutoff, int Count);

        private readonly TOP4HoneyChainsContext _context;
        private readonly RetentionOptions _options;
        private readonly TimeProvider _clock;
        private readonly AuditWriter _audit;

        public RetentionService(TOP4HoneyChainsContext context, IOptionsSnapshot<RetentionOptions> options, TimeProvider clock, AuditWriter audit)
        {
            _context = context;
            _options = options.Value;
            _clock = clock;
            _audit = audit;
        }

        public RetentionOptions Options => _options;
        private DateTime Now => _clock.GetUtcNow().UtcDateTime;

        // one day stricter than the triggers of 028, so a time-zone edge never makes a run fail
        private DateTime CodeCutoff => Now.AddDays(-_options.OneTimeCodeDays - 1);
        private DateTime SmsCutoff => Now.AddYears(-_options.SmsYears).AddDays(-1);
        private DateTime PriceCutoff => Now.AddYears(-_options.PriceYears).AddDays(-1);

        // ---------------------------------------------------------------- the rows each rule touches

        // codes that prove a delivery, a consent, a notice acceptance, a registration or a phone check stay with that record
        private const string CodesWhere = @"FROM dbo.OneTimeCodes c WHERE c.CreatedAt < @cutoff
              AND NOT EXISTS (SELECT 1 FROM dbo.LotIntakes x WHERE x.CodeId = c.CodeId)
              AND NOT EXISTS (SELECT 1 FROM dbo.Consents x WHERE x.CodeId = c.CodeId)
              AND NOT EXISTS (SELECT 1 FROM dbo.NoticeAcceptances x WHERE x.CodeId = c.CodeId)
              AND NOT EXISTS (SELECT 1 FROM dbo.BeekeeperRegistrations x WHERE x.CodeId = c.CodeId)
              AND NOT EXISTS (SELECT 1 FROM dbo.UserSettings x WHERE x.PhoneCodeId = c.CodeId)";
        private const string SmsWhere = @"FROM dbo.SmsMessages s WHERE s.CreatedAt < @cutoff
              AND NOT (s.BodyStored = N'' AND s.ToPhoneMasked = '' AND s.ToPhoneHash = REPLICATE('0', 64))";
        private const string PricesWhere = "FROM dbo.LotPrices p WHERE p.EnteredAt < @cutoff";

        // the end of a lot's retention: 5 years after the latest best-before of a batch that contains it, or 5 years after the delivery
        private const string LotEnds = @"WITH LotEnds AS (
                SELECT r.RawHoneyProductionId, a.ApiaryBeekeeperId AS BeekeeperId,
                       COALESCE(DATEADD(year, @bestBefore + @years, (SELECT MAX(f.FinalHoneyProductionDate) FROM dbo.FinalHoneyProductionDetails d
                                    JOIN dbo.FinalHoneyProductions f ON f.FinalHoneyProductionId = d.FinalHoneyProductionId
                                    WHERE d.RawHoneyProductionId = r.RawHoneyProductionId)),
                                DATEADD(year, @years, CAST(r.RawHoneyProductionDate AS datetime2))) AS EndsAt
                FROM dbo.RawHoneyProductions r LEFT JOIN dbo.Apiaries a ON a.ApiaryId = r.RawHoneyProductionApiaryId),
            Ended AS (SELECT b.BeekeeperId, (SELECT MAX(e.EndsAt) FROM LotEnds e WHERE e.BeekeeperId = b.BeekeeperId) AS LastEnd
                FROM dbo.Beekeepers b
                WHERE b.IsActive = 0 AND NOT EXISTS (SELECT 1 FROM LotEnds e WHERE e.BeekeeperId = b.BeekeeperId AND (e.EndsAt IS NULL OR e.EndsAt >= @now))) ";

        // the queries start with WITH, which EF cannot compose over: read the single value as a list
        private async Task<int> CountAsync(string sql, params SqlParameter[] parameters) =>
            (await _context.Database.SqlQueryRaw<int>(sql, parameters.Cast<object>().ToArray()).ToListAsync()).Single();

        private SqlParameter[] LotParameters() => new[]
        {
            new SqlParameter("@now", Now), new SqlParameter("@years", _options.ProductYears), new SqlParameter("@bestBefore", _options.BestBeforeYears)
        };

        // ---------------------------------------------------------------- dry run: counts only, nothing is changed

        public async Task<List<Rule>> DryRunAsync()
        {
            var rules = new List<Rule>
            {
                new("codes", true, CodeCutoff, await CountAsync("SELECT COUNT(*) AS Value " + CodesWhere, new SqlParameter("@cutoff", CodeCutoff))),
                new("sms", true, SmsCutoff, await CountAsync("SELECT COUNT(*) AS Value " + SmsWhere, new SqlParameter("@cutoff", SmsCutoff))),
                new("prices", true, PriceCutoff, await CountAsync("SELECT COUNT(*) AS Value " + PricesWhere, new SqlParameter("@cutoff", PriceCutoff))),
            };
            var productCutoff = Now.AddYears(-_options.ProductYears);
            rules.Add(new("lots", false, productCutoff,
                await CountAsync(LotEnds + "SELECT COUNT(*) AS Value FROM LotEnds WHERE EndsAt < @now", LotParameters())));
            rules.Add(new("beekeepers", false, productCutoff,
                await CountAsync(LotEnds + "SELECT COUNT(*) AS Value FROM Ended", LotParameters())));
            rules.Add(new("evidence", false, productCutoff, await CountAsync(LotEnds +
                "SELECT COUNT(*) AS Value FROM dbo.LotIntakes i JOIN LotEnds e ON e.RawHoneyProductionId = i.RawHoneyProductionId WHERE i.FormStoredFileName IS NOT NULL AND e.EndsAt < @now",
                LotParameters())));
            var proof = LotParameters().Append(new SqlParameter("@proofYears", _options.ProofYears)).ToArray();
            rules.Add(new("consents", false, productCutoff.AddYears(-_options.ProofYears), await CountAsync(LotEnds +
                @"SELECT (SELECT COUNT(*) FROM dbo.Consents c JOIN Ended x ON x.BeekeeperId = c.BeekeeperId WHERE x.LastEnd IS NULL OR x.LastEnd < DATEADD(year, -@proofYears, @now))
                       + (SELECT COUNT(*) FROM dbo.NoticeAcceptances n JOIN Ended x ON x.BeekeeperId = n.BeekeeperId WHERE x.LastEnd IS NULL OR x.LastEnd < DATEADD(year, -@proofYears, @now)) AS Value",
                proof)));
            var memberCutoff = Now.AddYears(-_options.FormerMemberYears);
            rules.Add(new("accounts", false, memberCutoff, await CountAsync(@"SELECT COUNT(*) AS Value FROM dbo.AspNetUsers u
                WHERE EXISTS (SELECT 1 FROM dbo.Memberships m WHERE m.UserId = u.Id)
                  AND NOT EXISTS (SELECT 1 FROM dbo.Memberships m WHERE m.UserId = u.Id AND (m.RevokedAt IS NULL OR m.RevokedAt >= @cutoff))
                  AND NOT EXISTS (SELECT 1 FROM dbo.AspNetUserRoles ur JOIN dbo.AspNetRoles r ON r.Id = ur.RoleId WHERE ur.UserId = u.Id AND r.Name = N'Administrator')
                  AND NOT EXISTS (SELECT 1 FROM dbo.BeekeeperAccounts b WHERE b.UserId = u.Id)", new SqlParameter("@cutoff", memberCutoff))));
            var auditCutoff = Now.AddYears(-_options.AuditYears);
            rules.Add(new("audit", false, auditCutoff, await CountAsync("SELECT COUNT(*) AS Value FROM dbo.AuditEntries WHERE At < @cutoff", new SqlParameter("@cutoff", auditCutoff))));
            return rules;
        }

        // ---------------------------------------------------------------- the run: only the rules that apply, in one transaction

        public record RunResult(int Codes, int Sms, int Prices);

        public async Task<RunResult> ApplyAsync(string trigger)
        {
            if (!_options.ApplyEnabled)
                throw new InvalidOperationException("Retention is not enabled (Retention:ApplyEnabled).");
            var strategy = _context.Database.CreateExecutionStrategy();
            var result = await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _context.Database.BeginTransactionAsync();
                await _context.Database.ExecuteSqlRawAsync("EXEC sp_set_session_context @key = N'" + SessionKey + "', @value = 1");
                var codes = await _context.Database.ExecuteSqlRawAsync("DELETE c " + CodesWhere, new SqlParameter("@cutoff", CodeCutoff));
                var sms = await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE s SET s.BodyStored = N'', s.ToPhoneMasked = '', s.ToPhoneHash = REPLICATE('0', 64) " + SmsWhere, new SqlParameter("@cutoff", SmsCutoff));
                var prices = await _context.Database.ExecuteSqlRawAsync("DELETE p " + PricesWhere, new SqlParameter("@cutoff", PriceCutoff));
                await _context.Database.ExecuteSqlRawAsync("EXEC sp_set_session_context @key = N'" + SessionKey + "', @value = NULL");
                await tx.CommitAsync();
                return new RunResult(codes, sms, prices);
            });
            await _audit.WriteAsync(AuditAction, "Retention", trigger, null,
                new { result.Codes, result.Sms, result.Prices, CodeCutoff, SmsCutoff, PriceCutoff }, trigger == "schedule" ? "Scheduled" : null,
                userId: trigger == "schedule" ? "system" : null, userName: trigger == "schedule" ? "retention job" : null);
            return result;
        }

        public Task<DateTime?> LastRunAsync() =>
            _context.AuditEntries.AsNoTracking().Where(a => a.Action == AuditAction).OrderByDescending(a => a.AuditId).Select(a => (DateTime?)a.At).FirstOrDefaultAsync();

        // the scheduled run needs a first run by hand (the owner's approval) and is due a month after the last run
        public async Task<bool> ScheduledRunDueAsync()
        {
            if (!_options.ApplyEnabled || !_options.Schedule)
                return false;
            var manual = await _context.AuditEntries.AsNoTracking().AnyAsync(a => a.Action == AuditAction && a.Flags == null);
            var last = await LastRunAsync();
            return manual && last != null && last.Value.AddMonths(1) <= Now;
        }
    }

    // the monthly run (Retention:Schedule); checks once a day whether a run is due
    public class RetentionScheduler : BackgroundService
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly ILogger<RetentionScheduler> _log;

        public RetentionScheduler(IServiceScopeFactory scopes, ILogger<RetentionScheduler> log)
        {
            _scopes = scopes;
            _log = log;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopes.CreateScope();
                    var retention = scope.ServiceProvider.GetRequiredService<RetentionService>();
                    if (await retention.ScheduledRunDueAsync())
                    {
                        var r = await retention.ApplyAsync("schedule");
                        _log.LogInformation("Retention run: {Codes} codes deleted, {Sms} SMS cleared, {Prices} prices deleted", r.Codes, r.Sms, r.Prices);
                    }
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    _log.LogError(e, "Retention run failed");
                }
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}
