using System.Net;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-96: interface texts corrected for the user manuals (lv, tr, and the beekeeper portal in es, pl, lv)
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class D96TextsTests
    {
        private readonly E2EApp _app;

        public D96TextsTests(E2EApp app) => _app = app;

        private async Task<string> AdminPageAsync(string user, string url)
        {
            await _app.CreateUserAsync(user, "Administrator");
            var client = await UserClient.LoginAsync(_app, user);
            var response = await client.GetAsync(url);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Turkish_oversight_menu_is_gozetim_and_no_emanet()
        {
            var html = await AdminPageAsync("e2e_d96_tr", "/Dashboard/Index?culture=tr");
            Assert.Contains("Gözetim", html);
            Assert.DoesNotContain("manetçi", html);
        }

        [Fact]
        public async Task Latvian_open_button_is_atvert()
        {
            var html = await AdminPageAsync("e2e_d96_lv", "/Organizations/Index?culture=lv");
            Assert.Contains(">Atvērt<", html.Replace(" ", "").Replace("\r", "").Replace("\n", ""));
            Assert.DoesNotContain("Atvērts", html);
        }
    }
}
