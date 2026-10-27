using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-78 P5 (D78-SPEC 6.3, 028, D-84): the retention job. The dry run counts and changes nothing; the run is refused while it is
    // switched off and, once switched on and approved, removes exactly the rows past their period (through the triggers of 028).
    public partial class CustodyIntakeTests
    {
        private record RetentionSeed(long OldCode, long ProofCode, long RecentCode, long OldSms, long RecentSms, int OldPrice, int RecentPrice);

        private async Task<RetentionSeed> RetentionSeedAsync()
        {
            var p = await PlantAsync("ret");
            var proofUser = await _app.CreateUserAsync("zz_test_ret_phone");
            var now = DateTime.UtcNow;
            return await _app.WithContextAsync(async db =>
            {
                SmsMessage Sms(DateTime at) => new()
                {
                    Purpose = SmsPurpose.Info, ToPhoneMasked = "+90 5** *** 0000", ToPhoneHash = new string('a', 64), Language = "tr",
                    BodyStored = "zz_test retention", Segments = 1, Provider = "Fake", Status = SmsStatus.Queued, CreatedAt = at
                };
                var oldSms = Sms(now.AddYears(-2));
                var recentSms = Sms(now.AddDays(-10));
                db.SmsMessages.AddRange(oldSms, recentSms);
                await db.SaveChangesAsync();
                OneTimeCode Code(DateTime at) => new()
                {
                    SmsId = recentSms.SmsId, Purpose = SmsPurpose.PhoneVerify, SubjectType = "User", SubjectKey = "zz_test_ret", ContentSha256 = new string('b', 64),
                    CodeHash = new string('c', 64), Salt = new byte[16], ExpiresAt = at.AddMinutes(10), CreatedAt = at
                };
                var oldCode = Code(now.AddDays(-40));
                var proofCode = Code(now.AddDays(-40));
                var recentCode = Code(now.AddDays(-5));
                db.OneTimeCodes.AddRange(oldCode, proofCode, recentCode);
                await db.SaveChangesAsync();
                // the old proof code is the phone check of a user: it stays with that record
                db.UserSettings.Add(new UserSetting { UserId = proofUser, PhoneCodeId = proofCode.CodeId, PhoneVerifiedAt = now.AddDays(-40) });
                var lot = new RawHoneyProduction
                {
                    RawHoneyProductionApiaryId = p.ApiaryId, RawHoneyProductionAmount = 30, RawHoneyProductionDate = new DateOnly(2026, 6, 1),
                    RawHoneyProductionHoneyType = p.HoneyTypeId, RawHoneyProductionPeriodId = p.PeriodId, OwnerOrgId = p.OrgId, OriginCountry = "TR"
                };
                db.RawHoneyProductions.Add(lot);
                await db.SaveChangesAsync();
                LotPrice Price(DateTime at) => new() { RawHoneyProductionId = lot.RawHoneyProductionId, PricePerKg = 250, Currency = "TRY", EnteredByUserId = "seed", EnteredAt = at };
                var oldPrice = Price(now.AddYears(-6));
                var recentPrice = Price(now.AddDays(-1));
                db.LotPrices.AddRange(oldPrice, recentPrice);
                await db.SaveChangesAsync();
                return new RetentionSeed(oldCode.CodeId, proofCode.CodeId, recentCode.CodeId, oldSms.SmsId, recentSms.SmsId, oldPrice.PriceId, recentPrice.PriceId);
            });
        }

        private async Task<Dictionary<string, int>> DryRunAsync()
        {
            using var scope = _app.Services.CreateScope();
            return (await scope.ServiceProvider.GetRequiredService<RetentionService>().DryRunAsync()).ToDictionary(r => r.Key, r => r.Count);
        }

        // the triggers of 023-025 and the retention passage of 028, on the test database
        private async Task InstallRetentionTriggersAsync()
        {
            await using var conn = new SqlConnection(_app.ConnectionString);
            await conn.OpenAsync();
            await InstallTriggersAsync(conn);
            var script = await File.ReadAllTextAsync(Path.Combine(RepoRoot(), "db", "migrations", "028_d78_retention.sql"));
            foreach (var batch in Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline).Where(b => b.Contains("CREATE OR ALTER TRIGGER")))
                await new SqlCommand(batch.Trim(), conn).ExecuteNonQueryAsync();
        }

        [Fact]
        public async Task The_retention_dry_run_counts_and_changes_nothing_and_the_run_waits_for_the_switch_and_the_approval()
        {
            await InstallRetentionTriggersAsync();
            await _app.CreateUserAsync("zz_test_ret_admin", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "zz_test_ret_admin");
            var before = await DryRunAsync();
            var s = await RetentionSeedAsync();

            // dry run: the old unreferenced code, the old SMS and the old price are counted; nothing changes
            var after = await DryRunAsync();
            Assert.Equal(before["codes"] + 1, after["codes"]);
            Assert.Equal(before["sms"] + 1, after["sms"]);
            Assert.Equal(before["prices"] + 1, after["prices"]);
            var page = await admin.HtmlAsync("/Retention/Index");
            Assert.Contains("data-rule=\"codes\"", page);
            Assert.Contains("data-rule=\"audit\"", page);
            Assert.DoesNotContain("id=\"retention-run\"", page);   // off by default: no run button
            var csv = await (await admin.GetAsync("/Retention/Report")).Content.ReadAsStringAsync();
            Assert.Contains("codes;", csv);
            Assert.True(await _app.WithContextAsync(db => db.OneTimeCodes.AnyAsync(c => c.CodeId == s.OldCode)));

            // the triggers refuse a deletion outside the job
            await using (var conn = new SqlConnection(_app.ConnectionString))
            {
                await conn.OpenAsync();
                await Assert.ThrowsAsync<SqlException>(() => new SqlCommand($"DELETE FROM dbo.OneTimeCodes WHERE CodeId = {s.OldCode}", conn).ExecuteNonQueryAsync());
                await Assert.ThrowsAsync<SqlException>(() => new SqlCommand($"DELETE FROM dbo.LotPrices WHERE PriceId = {s.OldPrice}", conn).ExecuteNonQueryAsync());
            }

            // switched off: refused, nothing changes
            Assert.Contains("Retention is off", await admin.PostFlashAsync("/Retention/Run", new() { ["approved"] = "true" }, "/Retention/Index"));
            Assert.Equal(after["codes"], (await DryRunAsync())["codes"]);

            var config = _app.Services.GetRequiredService<IConfiguration>();
            config["Retention:ApplyEnabled"] = "true";
            try
            {
                Assert.Contains("id=\"retention-run\"", await admin.HtmlAsync("/Retention/Index"));
                // without the owner's approval: refused
                Assert.Contains("Confirm that the owner approved", await admin.PostFlashAsync("/Retention/Run", new(), "/Retention/Index"));
                // a non-administrator cannot open or run it
                var p = await PlantAsync("ret2");
                Assert.NotEqual(200, await p.Co.StatusAsync("/Retention/Index"));
                await p.Co.PostAsync("/Retention/Run", new() { ["approved"] = "true" }, "/MyWork/Index");
                Assert.True(await _app.WithContextAsync(db => db.OneTimeCodes.AnyAsync(c => c.CodeId == s.OldCode)));

                Assert.Contains("Retention run:", await admin.PostFlashAsync("/Retention/Run", new() { ["approved"] = "true" }, "/Retention/Index"));
            }
            finally
            {
                config["Retention:ApplyEnabled"] = "false";
            }

            await _app.WithContextAsync(async db =>
            {
                Assert.False(await db.OneTimeCodes.AnyAsync(c => c.CodeId == s.OldCode));
                Assert.True(await db.OneTimeCodes.AnyAsync(c => c.CodeId == s.ProofCode));      // proves a phone check
                Assert.True(await db.OneTimeCodes.AnyAsync(c => c.CodeId == s.RecentCode));
                var old = await db.SmsMessages.AsNoTracking().SingleAsync(m => m.SmsId == s.OldSms);
                Assert.Equal("", old.BodyStored);
                Assert.Equal("", old.ToPhoneMasked);
                Assert.Equal(SmsPurpose.Info, old.Purpose);                                      // the count stays
                Assert.Equal("zz_test retention", (await db.SmsMessages.AsNoTracking().SingleAsync(m => m.SmsId == s.RecentSms)).BodyStored);
                Assert.False(await db.LotPrices.AnyAsync(x => x.PriceId == s.OldPrice));
                Assert.True(await db.LotPrices.AnyAsync(x => x.PriceId == s.RecentPrice));
                Assert.True(await db.AuditEntries.AnyAsync(a => a.Action == RetentionService.AuditAction && a.Flags == null));
                return 0;
            });
            var final = await DryRunAsync();
            Assert.Equal(0, final["codes"]);
            Assert.Equal(0, final["sms"]);
            Assert.Equal(0, final["prices"]);
            // the monthly run stays off unless switched on; even then it waits a month after the last run
            using var scope = _app.Services.CreateScope();
            Assert.False(await scope.ServiceProvider.GetRequiredService<RetentionService>().ScheduledRunDueAsync());
        }
    }
}
