using System.Net;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-95: interface fixes from the user manual's report (items 4, 5, 7, 9, 13, 14, 15, 16, 17, 18, 19)
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class D95InterfaceTests
    {
        private readonly E2EApp _app;

        public D95InterfaceTests(E2EApp app) => _app = app;

        private async Task<UserClient> AdminAsync([System.Runtime.CompilerServices.CallerMemberName] string test = "", string suffix = "")
        {
            var name = $"e2e_d95_{test[..Math.Min(12, test.Length)]}{suffix}".ToLowerInvariant();
            await _app.CreateUserAsync(name, "Administrator");
            return await UserClient.LoginAsync(_app, name);
        }

        private static async Task<string> HtmlAsync(UserClient client, string url)
        {
            var response = await client.GetAsync(url);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("en", "Consumers")]
        [InlineData("tr", "Tüketiciler")]
        [InlineData("lv", "Patērētāji")]
        public async Task The_consumer_audience_of_a_notice_has_a_label(string culture, string label)
        {
            var html = await HtmlAsync(await AdminAsync(suffix: culture), $"/Notices/Publish?culture={culture}");
            Assert.Contains(label, html);
            Assert.DoesNotContain("Notice.Audience.Consumer", html);
        }

        [Fact]
        public async Task Retention_says_1_year_not_1_years()
        {
            var html = await HtmlAsync(await AdminAsync(), "/Retention/Index?culture=en");
            Assert.Contains(">1 year<", html.Replace("\r", "").Replace("\n", "").Replace("  ", ""));
            Assert.DoesNotContain("1 years", html);
        }

        [Fact]
        public async Task The_organisations_list_has_no_wholesaler_tag_and_the_registry_text_is_current()
        {
            var admin = await AdminAsync();
            var orgs = await HtmlAsync(admin, "/Organizations/Index?culture=en");
            Assert.DoesNotContain(">Wholesaler</span>", orgs);
            var registry = await HtmlAsync(admin, "/Registry/Index?culture=en");
            Assert.DoesNotContain("they never log in", registry);
            Assert.Contains("can also sign in to the beekeeper area", registry);
        }
    }
}
