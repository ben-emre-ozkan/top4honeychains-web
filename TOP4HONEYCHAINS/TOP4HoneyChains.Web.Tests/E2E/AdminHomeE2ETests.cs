using System.Net;
using System.Text.RegularExpressions;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-72: the admin home renders its sections in every language, with no raw resource keys and no coordinates
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class AdminHomeE2ETests
    {
        private readonly E2EApp _app;

        public AdminHomeE2ETests(E2EApp app) => _app = app;

        [Theory]
        [InlineData("tr")]
        [InlineData("en")]
        [InlineData("es")]
        [InlineData("pl")]
        [InlineData("lv")]
        public async Task The_admin_home_shows_figures_chain_map_activity_and_waiting_items(string culture)
        {
            var name = $"e2e_home_admin_{culture}";
            await _app.CreateUserAsync(name, "Administrator");
            var admin = await UserClient.LoginAsync(_app, name);

            var response = await admin.GetAsync($"/Dashboard/Index?culture={culture}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            foreach (var id in new[] { "ah-title", "kpi-lots", "kpi-batches", "kpi-reports", "kpi-waiting", "kpi-orgs", "chain-apiary", "chain-delivery",
                                       "chain-lab", "chain-processing", "chain-shelf", "ah-map", "ah-waiting", "ah-activity", "qa-beekeeper", "qa-batch" })
                Assert.Contains($"id=\"{id}\"", html);
            Assert.Contains("data-hero", html);                 // the chain strip runs the landing's light path
            Assert.Contains(name, html);                        // the greeting names the admin
            Assert.DoesNotMatch(new Regex(@">\s*Home\.[A-Z][A-Za-z.]+\s*<"), html);   // no untranslated resource key
            Assert.DoesNotContain("Latitude", html);
            Assert.DoesNotContain("Longitude", html);
        }
    }
}
