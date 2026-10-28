using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.Messaging;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-86: the application as a demo site, on its own database that carries the demo marker
    public sealed class DemoModeApp : E2EApp
    {
        public const string DemoDatabaseName = "TOP4HoneyChainsDemoModeE2E";

        protected override string DatabaseName => DemoDatabaseName;

        protected override void AddSettings(Dictionary<string, string?> settings)
        {
            settings["Demo:Enabled"] = "true";
            settings["Sms:AllowedCountries"] = "TR,AR,PL,LV";
        }

        protected override Task PrepareDatabaseAsync() => MasterAsync(
            $"CREATE DATABASE [{DemoDatabaseName}]; EXEC('USE [{DemoDatabaseName}]; EXEC sys.sp_addextendedproperty @name = N''{DemoDatabase.MarkerName}'', @value = N''1'';')");
    }

    [CollectionDefinition("DemoMode")]
    public class DemoModeCollection : ICollectionFixture<DemoModeApp> { }

    [Collection("DemoMode")]
    [Trait("Category", "E2E")]
    public class DemoModeTests
    {
        private readonly DemoModeApp _app;
        public DemoModeTests(DemoModeApp app) => _app = app;

        [Fact]
        public async Task Every_page_shows_the_banner_and_asks_not_to_be_indexed()
        {
            var visitor = UserClient.Anonymous(_app);
            foreach (var url in new[] { "/?culture=en", "/Account/Login?culture=en", "/privacy?culture=en" })
            {
                var response = await visitor.GetAsync(url);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("noindex, nofollow", response.Headers.GetValues("X-Robots-Tag").Single());
                var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
                Assert.Single(System.Text.RegularExpressions.Regex.Matches(html, "id=\"demo-banner\""));
                Assert.Contains("Demo environment – no real SMS", html);
                Assert.DoesNotContain("demo-inbox-link", html);   // only for signed-in users
            }
            var robots = await (await visitor.GetAsync("/robots.txt")).Content.ReadAsStringAsync();
            Assert.Contains("Disallow: /", robots);
            // the draft notices carry the demo mark
            Assert.Contains("DEMO TEXT – not approved", WebUtility.HtmlDecode(await visitor.HtmlAsync("/privacy?culture=en")));
        }

        [Fact]
        public async Task The_inbox_shows_the_messages_of_the_active_organization_and_the_own_ones()
        {
            var plant = await _app.CreateOrgAsync("zz_test demo plant", null, (Capabilities.Processing, null));
            var other = await _app.CreateOrgAsync("zz_test demo other", null, (Capabilities.Processing, null));
            var sao = await _app.CreateUserAsync("zz_test_demo_sao");
            var outsider = await _app.CreateUserAsync("zz_test_demo_outsider");
            await _app.AddMemberAsync(plant, sao, OrgRoles.SampleAcceptanceOfficer);
            await _app.AddMemberAsync(other, outsider, OrgRoles.SampleAcceptanceOfficer);
            await _app.CreateUserAsync("zz_test_demo_admin", "Administrator");
            await _app.WithContextAsync(async db =>
            {
                SmsMessage Message(string text, int? orgId, string? sender) => new()
                {
                    Purpose = SmsPurpose.IntakeConfirm, ToPhoneMasked = "+90 555 *** ** 01", ToPhoneHash = "zz", Language = "en", BodyStored = text,
                    Segments = 1, Provider = "Fake", Status = SmsStatus.Fake, OrgId = orgId, SentByUserId = sender, CreatedAt = DateTime.UtcNow
                };
                db.SmsMessages.AddRange(Message("zz plant code 111111", plant, null), Message("zz other code 222222", other, null),
                    Message("zz own notice code 333333", null, sao));
                return await db.SaveChangesAsync();
            });

            var member = await UserClient.LoginAsync(_app, "zz_test_demo_sao");
            var home = await member.HtmlAsync("/MyWork/Index");
            Assert.Contains("id=\"demo-inbox-link\"", home);
            var inbox = await member.HtmlAsync("/Demo/Inbox");
            Assert.Contains("zz plant code <strong class=\"demo-code\">111111</strong>", inbox);
            Assert.Contains("333333", inbox);
            Assert.DoesNotContain("222222", inbox);

            var admin = await UserClient.LoginAsync(_app, "zz_test_demo_admin");
            var all = await admin.HtmlAsync("/Demo/Inbox");
            foreach (var code in new[] { "111111", "222222", "333333" })
                Assert.Contains(code, all);

            var redirect = await UserClient.Anonymous(_app).GetAsync("/Demo/Inbox");
            Assert.Equal(HttpStatusCode.Redirect, redirect.StatusCode);
        }

        [Fact]
        public async Task A_partner_country_demo_number_starts_a_registration()
        {
            var org = await _app.CreateOrgAsync("zz_test demo registro", null, (Capabilities.BeekeeperRegistration, null));
            await _app.AddMemberAsync(org, await _app.CreateUserAsync("zz_test_demo_ro"), OrgRoles.RegistrationOfficer);
            var ro = await UserClient.LoginAsync(_app, "zz_test_demo_ro");
            var found = await ro.PostAsync("/Registration/Find", new() { ["number"] = "ar-demo-000123" }, "/Registration/Index");
            Assert.Equal("/Registration/New?registryNumber=AR-DEMO-000123", found.Headers.Location?.ToString());
            Assert.Contains("in this demo also AR-DEMO-", await ro.PostFlashAsync("/Registration/Find", new() { ["number"] = "XX-DEMO-000123" }, "/Registration/Index"));
        }

        // a demo configuration never starts on a database without the marker (e.g. the live one), nor with a real SMS provider.
        // The guard is called directly: a second host in the test run would replace the application's static helpers.
        [Theory]
        [InlineData("master", "Fake", "has no 'TOP4HoneyChains.Demo' marker")]
        [InlineData(DemoModeApp.DemoDatabaseName, "Logging", "needs Sms:Provider=Fake")]
        public async Task The_guard_refuses_to_start(string database, string provider, string message)
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = E2EApp.ConnectionStringOf(database)
            }).Build();
            var demo = new DemoMode(Options.Create(new DemoOptions { Enabled = true }));
            var guard = new DemoGuard(demo, Options.Create(new SmsOptions { Provider = provider }), config);
            var error = await Assert.ThrowsAsync<InvalidOperationException>(() => guard.StartAsync(CancellationToken.None));
            Assert.Contains(message, error.Message);
            Assert.False(demo.DatabaseOk);

            // the demo database with the fake provider passes
            var ok = new DemoGuard(demo, Options.Create(new SmsOptions { Provider = "Fake" }), new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string?> { ["ConnectionStrings:DefaultConnection"] = E2EApp.ConnectionStringOf(DemoModeApp.DemoDatabaseName) }).Build());
            await ok.StartAsync(CancellationToken.None);
            Assert.True(demo.DatabaseOk);
        }
    }
}
