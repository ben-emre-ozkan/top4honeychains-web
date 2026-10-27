using System.Net;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.Messaging;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-78 P1 (D78-SPEC 5.2): the one-time code rules against the database, with the fake provider and a movable clock.
    // Every test uses its own phone numbers and subjects, because the rate limits count across the shared test database.
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class MessageServiceTests
    {
        private readonly E2EApp _app;
        public MessageServiceTests(E2EApp app) => _app = app;

        private static readonly Dictionary<string, string> Values = new() { ["org"] = "zz_test org", ["link"] = "http://localhost/privacy", ["name"] = "NO", ["version"] = "v1" };

        private async Task<T> WithServiceAsync<T>(Func<IMessageService, TOP4HoneyChainsContext, Task<T>> work)
        {
            using var scope = _app.Services.CreateScope();
            return await work(scope.ServiceProvider.GetRequiredService<IMessageService>(), scope.ServiceProvider.GetRequiredService<TOP4HoneyChainsContext>());
        }

        private Task<SendResult> SendAsync(string subject, string phone, string content = "c1", string? sender = null, string purpose = SmsPurpose.Registration) =>
            WithServiceAsync((m, _) => m.SendCodeAsync(new CodeRequest(purpose, "Test", subject, phone, "en", CodeRules.ContentHash(content), Values, sender)));

        private Task<VerifyResult> VerifyAsync(string subject, string? code, string content = "c1", string? user = null, string purpose = SmsPurpose.Registration) =>
            WithServiceAsync((m, _) => m.VerifyCodeAsync(purpose, "Test", subject, code, CodeRules.ContentHash(content), user));

        // the code as the fake provider delivered it (the store of the fake provider, D78-SPEC 5.3)
        private string CodeOf(long smsId)
        {
            var text = _app.WithContextAsync(db => db.SmsMessages.Where(m => m.SmsId == smsId).Select(m => m.BodyStored).FirstAsync()).Result;
            return Regex.Match(text, @"\b\d{6}\b").Value;
        }

        [Fact]
        public async Task A_code_is_hashed_bound_to_its_subject_and_used_once()
        {
            var sent = await SendAsync("zz_test-otp-1", "+905320000101");
            Assert.True(sent.Ok, sent.Error);
            var code = CodeOf(sent.SmsId!.Value);
            Assert.Equal(6, code.Length);

            var stored = await _app.WithContextAsync(db => db.OneTimeCodes.FirstAsync(c => c.CodeId == sent.CodeId));
            Assert.NotEqual(code, stored.CodeHash);
            Assert.DoesNotContain(code, stored.CodeHash);
            Assert.Equal(16, stored.Salt.Length);
            Assert.True(stored.ExpiresAt - stored.CreatedAt == TimeSpan.FromMinutes(5));
            var sms = await _app.WithContextAsync(db => db.SmsMessages.FirstAsync(m => m.SmsId == sent.SmsId));
            Assert.Equal("+90******0101", sms.ToPhoneMasked);
            Assert.DoesNotContain("5320000101", sms.ToPhoneHash);
            Assert.Equal(SmsStatus.Fake, sms.Status);

            Assert.False((await VerifyAsync("zz_test-otp-other", code)).Ok);        // another subject
            Assert.True((await VerifyAsync("zz_test-otp-1", code)).Ok);
            Assert.False((await VerifyAsync("zz_test-otp-1", code)).Ok);            // used once
        }

        [Fact]
        public async Task Three_wrong_entries_void_the_code()
        {
            var sent = await SendAsync("zz_test-otp-2", "+905320000102");
            var code = CodeOf(sent.SmsId!.Value);
            var wrong = code == "000000" ? "111111" : "000000";
            Assert.Equal("Wrong code. 2 tries left.", (await VerifyAsync("zz_test-otp-2", wrong)).Error);
            Assert.Equal("Wrong code. 1 tries left.", (await VerifyAsync("zz_test-otp-2", "12x")).Error);
            Assert.Equal("Wrong code. The code is no longer valid; send a new code.", (await VerifyAsync("zz_test-otp-2", wrong)).Error);
            Assert.False((await VerifyAsync("zz_test-otp-2", code)).Ok);
            Assert.Equal(CodeVoidReason.TooManyAttempts, await _app.WithContextAsync(db => db.OneTimeCodes.Where(c => c.CodeId == sent.CodeId).Select(c => c.VoidReason).FirstAsync()));
        }

        [Fact]
        public async Task A_code_expires_after_five_minutes()
        {
            var sent = await SendAsync("zz_test-otp-3", "+905320000103");
            var code = CodeOf(sent.SmsId!.Value);
            try
            {
                _app.Clock.Advance(TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(1));
                Assert.Equal("The code has expired. Send a new code.", (await VerifyAsync("zz_test-otp-3", code)).Error);
            }
            finally
            {
                _app.Clock.Reset();
            }
        }

        [Fact]
        public async Task Changed_content_voids_the_code()
        {
            var sent = await SendAsync("zz_test-otp-4", "+905320000104", content: "name=A");
            var code = CodeOf(sent.SmsId!.Value);
            Assert.Equal("The details changed after the code was sent. Send a new code.", (await VerifyAsync("zz_test-otp-4", code, content: "name=B")).Error);
            Assert.False((await VerifyAsync("zz_test-otp-4", code, content: "name=A")).Ok);
        }

        [Fact]
        public async Task A_resend_waits_a_minute_and_voids_the_earlier_code_and_at_most_three_per_hour()
        {
            try
            {
                var first = await SendAsync("zz_test-otp-5", "+905320000105");
                Assert.Equal("Wait a minute before sending a new code.", (await SendAsync("zz_test-otp-5", "+905320000105")).Error);
                _app.Clock.Advance(TimeSpan.FromSeconds(61));
                var second = await SendAsync("zz_test-otp-5", "+905320000105");
                Assert.True(second.Ok, second.Error);
                Assert.Equal(CodeVoidReason.Resent, await _app.WithContextAsync(db => db.OneTimeCodes.Where(c => c.CodeId == first.CodeId).Select(c => c.VoidReason).FirstAsync()));
                _app.Clock.Advance(TimeSpan.FromSeconds(61));
                Assert.True((await SendAsync("zz_test-otp-5", "+905320000105")).Ok);
                _app.Clock.Advance(TimeSpan.FromSeconds(61));
                Assert.Equal("Too many codes for this record in the last hour. Try again later.", (await SendAsync("zz_test-otp-5", "+905320000105")).Error);
                // the newest code still works
                var newest = await _app.WithContextAsync(db => db.OneTimeCodes.Where(c => c.SubjectKey == "zz_test-otp-5").OrderByDescending(c => c.CodeId).Select(c => c.SmsId).FirstAsync());
                Assert.True((await VerifyAsync("zz_test-otp-5", CodeOf(newest))).Ok);
            }
            finally
            {
                _app.Clock.Reset();
            }
        }

        [Fact]
        public async Task At_most_five_codes_per_hour_to_one_phone()
        {
            for (var i = 0; i < 5; i++)
                Assert.True((await SendAsync($"zz_test-otp-6-{i}", "+905320000106")).Ok);
            Assert.Equal("Too many codes to this phone number. Try again later.", (await SendAsync("zz_test-otp-6-x", "+905320000106")).Error);
        }

        [Fact]
        public async Task An_officer_sends_at_most_thirty_codes_per_hour_and_pauses_after_ten_wrong_entries()
        {
            var officer = "zz_test-officer-" + Guid.NewGuid().ToString("N");
            var sent = new List<SendResult>();
            for (var i = 0; i < 30; i++)
            {
                var r = await SendAsync($"zz_test-otp-7-{i}", $"+9053211{i:D5}", sender: officer);
                Assert.True(r.Ok, r.Error);
                sent.Add(r);
            }
            Assert.Equal("You have sent too many codes in the last hour. Try again later.", (await SendAsync("zz_test-otp-7-x", "+905321199999", sender: officer)).Error);

            // ten wrong entries on the officer's codes, then the right code is refused for 15 minutes
            for (var i = 0; i < 5; i++)
            {
                await VerifyAsync($"zz_test-otp-7-{i}", "999999", user: officer);
                await VerifyAsync($"zz_test-otp-7-{i}", "999998", user: officer);
            }
            var good = CodeOf(sent[10].SmsId!.Value);
            Assert.Equal("Too many wrong codes. Wait 15 minutes before trying again.", (await VerifyAsync("zz_test-otp-7-10", good, user: officer)).Error);
            try
            {
                _app.Clock.Advance(TimeSpan.FromMinutes(4));   // still paused
                Assert.False((await VerifyAsync("zz_test-otp-7-10", good, user: officer)).Ok);
            }
            finally
            {
                _app.Clock.Reset();
            }
        }

        [Fact]
        public async Task Numbers_outside_the_allowed_countries_are_refused()
        {
            Assert.Equal("SMS to numbers outside the allowed countries is not enabled.", (await SendAsync("zz_test-otp-8", "+37120345678")).Error);
            Assert.Equal("A mobile phone number is required.", (await SendAsync("zz_test-otp-8", "")).Error);
        }

        [Fact]
        public async Task The_daily_limit_stops_everything_but_password_resets_and_deliveries()
        {
            try
            {
                _app.Clock.Advance(TimeSpan.FromDays(3));   // a day of its own, so other tests are not affected
                // fill the day up to the limit (E2E: Sms:DailyLimit = 2000)
                await _app.WithContextAsync(async db =>
                {
                    var day = _app.Clock.GetUtcNow().UtcDateTime;
                    for (var i = 0; i < 8; i++)
                        db.SmsMessages.Add(new SmsMessage
                        {
                            Purpose = SmsPurpose.Info, ToPhoneMasked = "+90******0000", ToPhoneHash = new string('0', 64), Language = "tr",
                            BodyStored = "zz_test filler", Segments = 250, Provider = "Fake", Status = SmsStatus.Fake, CreatedAt = day
                        });
                    return await db.SaveChangesAsync();
                });
                Assert.True(await WithServiceAsync((m, _) => Task.FromResult(m.DailyLimitReached())));
                Assert.Equal("The daily SMS limit of the platform is reached. Try again tomorrow or use the signed-photo path.",
                    (await SendAsync("zz_test-otp-9", "+905320000109")).Error);
                Assert.True((await SendAsync("zz_test-otp-9r", "+905320000119", purpose: SmsPurpose.PasswordReset)).Ok);
            }
            finally
            {
                _app.Clock.Reset();
            }
        }

        [Fact]
        public async Task Outbox_exists_for_platform_administrators_with_the_fake_provider()
        {
            await _app.CreateUserAsync("zz_test_sms_admin", "Administrator");
            var sent = await SendAsync("zz_test-otp-10", "+905320000110");
            Assert.True(sent.Ok, sent.Error);
            var admin = await UserClient.LoginAsync(_app, "zz_test_sms_admin");
            var html = await admin.HtmlAsync("/Sms/Outbox");
            Assert.Contains("id=\"sms-outbox\"", html);
            Assert.Contains("905320000110", html);   // full number from the fake store (Razor encodes the +)
            Assert.Contains("id=\"menu-sms-outbox\"", html);

            var memberId = await _app.CreateUserAsync("zz_test_sms_member");
            var org = await _app.CreateOrgAsync("zz_test sms org", null, ("Processing", null));
            await _app.AddMemberAsync(org, memberId, OrgRoles.OrgAdmin);
            var member = await UserClient.LoginAsync(_app, "zz_test_sms_member");
            Assert.NotEqual(HttpStatusCode.OK, (await member.GetAsync("/Sms/Outbox")).StatusCode);
        }
    }
}
