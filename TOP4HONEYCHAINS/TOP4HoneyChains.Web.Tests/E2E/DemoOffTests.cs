using System.Net;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-86: without Demo:Enabled (the live site) nothing of the demo shows: no banner, no inbox, no demo registry numbers
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class DemoOffTests
    {
        private readonly E2EApp _app;
        public DemoOffTests(E2EApp app) => _app = app;

        [Fact]
        public async Task The_live_configuration_has_no_demo_features()
        {
            var visitor = UserClient.Anonymous(_app);
            var landing = await visitor.GetAsync("/?culture=en");
            Assert.False(landing.Headers.Contains("X-Robots-Tag"));
            Assert.DoesNotContain("demo-banner", await landing.Content.ReadAsStringAsync());
            Assert.DoesNotContain("DEMO TEXT", WebUtility.HtmlDecode(await visitor.HtmlAsync("/privacy?culture=en")));

            await _app.CreateUserAsync("zz_test_demooff_admin", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "zz_test_demooff_admin");
            Assert.Equal(404, await admin.StatusAsync("/Demo/Inbox"));

            var org = await _app.CreateOrgAsync("zz_test demooff registro", null, (Capabilities.BeekeeperRegistration, null));
            await _app.AddMemberAsync(org, await _app.CreateUserAsync("zz_test_demooff_ro"), OrgRoles.RegistrationOfficer);
            var ro = await UserClient.LoginAsync(_app, "zz_test_demooff_ro");
            var flash = await ro.PostFlashAsync("/Registration/Find", new() { ["number"] = "AR-DEMO-000123" }, "/Registration/Index");
            Assert.Equal("The number must be TR, a province code 01-81 and 10 digits (e.g. TR061234567890).", flash);
        }
    }
}
