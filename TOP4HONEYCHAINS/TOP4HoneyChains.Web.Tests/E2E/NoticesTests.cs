using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.Identity;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-78 P1 (D78-SPEC 4.9, 6.1-6.2): every user meets the information notice; signing roles confirm by SMS code (O-12);
    // the gate blocks only with Notices:Enforce (O-14); each organization names its data controller
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class NoticesTests
    {
        private readonly E2EApp _app;
        public NoticesTests(E2EApp app) => _app = app;

        private async Task<(string UserId, int OrgId)> MemberAsync(string user, string role, string orgName)
        {
            var id = await _app.CreateUserAsync(user);
            var org = await _app.CreateOrgAsync(orgName, null, (Capabilities.Processing, null));
            await _app.AddMemberAsync(org, id, role);
            return (id, org);
        }

        private Task<List<NoticeAcceptance>> AcceptancesAsync(string userId) =>
            _app.WithContextAsync(db => db.NoticeAcceptances.Include(a => a.Notice).Where(a => a.UserId == userId).ToListAsync());

        private string LastCodeTo(string maskedEnd) =>
            _app.WithContextAsync(db => db.SmsMessages.Where(m => m.ToPhoneMasked.EndsWith(maskedEnd) && m.Purpose == SmsPurpose.NoticeConfirm)
                .OrderByDescending(m => m.SmsId).Select(m => m.BodyStored).FirstAsync()).Result is string body
                ? Regex.Match(body, @"\b\d{6}\b").Value : "";

        [Fact]
        public async Task A_member_sees_the_banner_and_accepts_by_click()
        {
            var (userId, _) = await MemberAsync("zz_test_notice_staff", OrgRoles.Staff, "zz_test notice org 1");
            var user = await UserClient.LoginAsync(_app, "zz_test_notice_staff");

            var work = await user.HtmlAsync("/MyWork/Index");
            Assert.Contains("id=\"notice-banner\"", work);

            var page = await user.HtmlAsync("/Notices/Accept");
            Assert.Contains("data-draft", page);                    // the seeded texts are drafts (O-14)
            Assert.DoesNotContain("id=\"notice-send-code\"", page);  // Staff is not a signing role (O-12)

            Assert.Equal("Tick the box to confirm that you have read the notice.",
                await user.PostFlashAsync("/Notices/Accept", new() { ["returnUrl"] = "/MyWork/Index" }, "/Notices/Accept"));
            var ok = await user.PostAsync("/Notices/Accept", new() { ["read"] = "true", ["returnUrl"] = "/MyWork/Index" }, "/Notices/Accept");
            Assert.Equal(HttpStatusCode.Redirect, ok.StatusCode);
            Assert.Equal("/MyWork/Index", ok.Headers.Location!.ToString());

            var acc = Assert.Single(await AcceptancesAsync(userId));
            Assert.Equal(AcceptanceMethod.Click, acc.Method);
            Assert.Null(acc.Notice!.OrgId);
            Assert.DoesNotContain("id=\"notice-banner\"", await user.HtmlAsync("/MyWork/Index"));
        }

        [Fact]
        public async Task A_signing_role_confirms_with_an_sms_code_which_also_verifies_the_phone()
        {
            var (userId, _) = await MemberAsync("zz_test_notice_oa", OrgRoles.OrgAdmin, "zz_test notice org 2");
            var user = await UserClient.LoginAsync(_app, "zz_test_notice_oa");
            Assert.Contains("id=\"notice-send-code\"", await user.HtmlAsync("/Notices/Accept"));

            // without a code nothing is accepted
            Assert.Equal("No valid code. Send a new code.",
                await user.PostFlashAsync("/Notices/Accept", new() { ["read"] = "true", ["code"] = "123456" }, "/Notices/Accept"));

            Assert.Equal("A code was sent by SMS. Enter it below.",
                await user.PostFlashAsync("/Notices/SendCode", new() { ["phone"] = "0532 000 02 01" }, "/Notices/Accept"));
            var code = LastCodeTo("0201");
            Assert.Matches(@"^\d{6}$", code);
            var ok = await user.PostAsync("/Notices/Accept", new() { ["read"] = "true", ["code"] = code }, "/Notices/Accept");
            Assert.Equal(HttpStatusCode.Redirect, ok.StatusCode);

            var acc = Assert.Single(await AcceptancesAsync(userId));
            Assert.Equal(AcceptanceMethod.SmsCode, acc.Method);
            Assert.NotNull(acc.CodeId);
            using var scope = _app.Services.CreateScope();
            var u = await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByIdAsync(userId);
            Assert.Equal("+905320000201", u!.PhoneNumber);
            Assert.True(u.PhoneNumberConfirmed);
        }

        [Fact]
        public async Task With_enforce_the_gate_blocks_until_the_notice_is_accepted()
        {
            var config = _app.Services.GetRequiredService<IConfiguration>();
            await MemberAsync("zz_test_notice_gate", OrgRoles.Staff, "zz_test notice org 3");
            var user = await UserClient.LoginAsync(_app, "zz_test_notice_gate");
            Assert.Equal(200, await user.StatusAsync("/MyWork/Index"));   // Notices:Enforce is off by default
            try
            {
                config["Notices:Enforce"] = "true";
                var blocked = await user.GetAsync("/MyWork/Index");
                Assert.Equal(HttpStatusCode.Redirect, blocked.StatusCode);
                Assert.StartsWith("/Notices/Accept?returnUrl=", blocked.Headers.Location!.ToString());
                Assert.Equal(200, await user.StatusAsync("/privacy"));        // public pages stay open
                Assert.Equal(200, await user.StatusAsync("/Notices/Accept"));
                await user.PostAsync("/Notices/Accept", new() { ["read"] = "true" }, "/Notices/Accept");
                Assert.Equal(200, await user.StatusAsync("/MyWork/Index"));
            }
            finally
            {
                config["Notices:Enforce"] = "false";
            }
        }

        [Fact]
        public async Task A_new_version_and_an_organization_notice_are_asked_for_again()
        {
            await _app.CreateUserAsync("zz_test_notice_admin", "Administrator");
            var (userId, orgId) = await MemberAsync("zz_test_notice_v2", OrgRoles.Staff, "zz_test notice org 4");
            var user = await UserClient.LoginAsync(_app, "zz_test_notice_v2");
            await user.PostAsync("/Notices/Accept", new() { ["read"] = "true" }, "/Notices/Accept");
            Assert.Contains("id=\"notice-none\"", await user.HtmlAsync("/Notices/Accept"));

            // the organization publishes its own notice: the member is asked for it, the platform notice stays accepted
            var admin = await UserClient.LoginAsync(_app, "zz_test_notice_admin");
            var published = await admin.PostAsync("/Notices/Publish", new()
            {
                ["kind"] = NoticeKind.PrivacyNotice, ["audience"] = NoticeAudience.Member, ["orgId"] = orgId.ToString(), ["purpose"] = "",
                ["version"] = "zz_test-org-v1", ["language"] = "en", ["body"] = "zz_test organization notice", ["isDraft"] = "true"
            }, "/Notices/Publish");
            Assert.Equal(HttpStatusCode.Redirect, published.StatusCode);
            var page = await user.HtmlAsync("/Notices/Accept");
            Assert.Contains("zz_test organization notice", page);
            Assert.Contains("zz_test notice org 4", page);
            var duplicate = await admin.PostAsync("/Notices/Publish", new()
            {
                ["kind"] = NoticeKind.PrivacyNotice, ["audience"] = NoticeAudience.Member, ["orgId"] = orgId.ToString(),
                ["version"] = "zz_test-org-v1", ["language"] = "en", ["body"] = "again"
            }, "/Notices/Publish");
            Assert.Equal(HttpStatusCode.OK, duplicate.StatusCode);   // the form is shown again with the message
            Assert.Contains("This version already exists in this language. Publish a new version.", await duplicate.Content.ReadAsStringAsync());

            await user.PostAsync("/Notices/Accept", new() { ["read"] = "true" }, "/Notices/Accept");
            var acc = await AcceptancesAsync(userId);
            Assert.Equal(2, acc.Count);
            Assert.Contains(acc, a => a.OrgId == orgId);

            // a second version retires the first; the member is asked again
            await admin.PostAsync("/Notices/Publish", new()
            {
                ["kind"] = NoticeKind.PrivacyNotice, ["audience"] = NoticeAudience.Member, ["orgId"] = orgId.ToString(),
                ["version"] = "zz_test-org-v2", ["language"] = "en", ["body"] = "zz_test organization notice v2"
            }, "/Notices/Publish");
            Assert.Contains("zz_test organization notice v2", await user.HtmlAsync("/Notices/Accept"));
            Assert.NotNull(await _app.WithContextAsync(db => db.Notices.Where(n => n.Version == "zz_test-org-v1").Select(n => n.RetiredAt).FirstAsync()));

            // members cannot publish
            Assert.NotEqual(HttpStatusCode.OK, (await user.GetAsync("/Notices/Publish")).StatusCode);
        }

        [Fact]
        public async Task The_public_notice_page_is_open_and_shows_the_consent_texts()
        {
            var anonymous = UserClient.Anonymous(_app);
            var beekeeper = await anonymous.HtmlAsync("/privacy");
            Assert.Contains("id=\"public-notice\"", beekeeper);
            Assert.Contains("data-draft", beekeeper);
            Assert.Contains("DRAFT", beekeeper);
            Assert.Contains("Name on public pages", beekeeper);
            var member = await anonymous.HtmlAsync("/privacy?for=member");
            Assert.Contains("Privacy notice for the users of TOP4HoneyChains", member);
        }

        [Fact]
        public async Task The_data_controller_is_kept_by_the_platform_administrator_or_the_compliance_officer()
        {
            await _app.CreateUserAsync("zz_test_dc_admin", "Administrator");
            var (_, orgId) = await MemberAsync("zz_test_dc_co", OrgRoles.ComplianceOfficer, "zz_test dc org");
            await _app.AddMemberAsync(orgId, await _app.CreateUserAsync("zz_test_dc_staff"), OrgRoles.Staff);

            var co = await UserClient.LoginAsync(_app, "zz_test_dc_co");
            Assert.Contains("id=\"data-controller-missing\"", await co.HtmlAsync($"/Organizations/Details/{orgId}"));
            Assert.Equal("Data controller details saved.", await co.PostFlashAsync($"/Organizations/DataController/{orgId}",
                new() { ["name"] = "zz_test Kooperatif A.Ş.", ["contact"] = "kvkk@example.invalid", ["address"] = "zz_test address", ["verbis"] = "zz-1" },
                $"/Organizations/Details/{orgId}"));
            var html = await co.HtmlAsync($"/Organizations/Details/{orgId}");
            Assert.DoesNotContain("id=\"data-controller-missing\"", html);

            var staff = await UserClient.LoginAsync(_app, "zz_test_dc_staff");
            Assert.DoesNotContain("id=\"data-controller-form\"", await staff.HtmlAsync($"/Organizations/Details/{orgId}"));
            var refused = await staff.PostAsync($"/Organizations/DataController/{orgId}", new() { ["name"] = "zz_test changed" }, $"/Organizations/Details/{orgId}");
            Assert.True(refused.StatusCode != HttpStatusCode.Redirect || refused.Headers.Location!.ToString().Contains("AccessDenied"), $"{refused.StatusCode} {refused.Headers.Location}");
            Assert.Equal("zz_test Kooperatif A.Ş.", await _app.WithContextAsync(db => db.Organizations.Where(o => o.OrgId == orgId).Select(o => o.DataControllerName).FirstAsync()));

            // the organization's member sees its data controller with the notice
            Assert.Contains("kvkk@example.invalid", await staff.HtmlAsync("/Notices/Accept"));
        }
    }
}
