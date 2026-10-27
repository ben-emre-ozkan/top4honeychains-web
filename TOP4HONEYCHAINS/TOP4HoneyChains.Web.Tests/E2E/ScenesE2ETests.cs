using System.Text.RegularExpressions;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-78 P4 (D78-SPEC 7, D-83): the switch persists per user, scenes reach the pages as static final frames with the switch on
    // the page, the gallery answers in Development, the tour carries the role's steps.
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class ScenesE2ETests
    {
        private readonly E2EApp _app;
        public ScenesE2ETests(E2EApp app) => _app = app;

        private static string Switch(string html) => Regex.Match(html, "<html[^>]*data-scenes=\"(on|off)\"").Groups[1].Value;

        [Fact]
        public async Task The_show_animations_switch_persists_per_user()
        {
            await _app.CreateUserAsync("zz_test_scene_admin", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "zz_test_scene_admin");
            Assert.Equal("on", Switch(await admin.HtmlAsync("/Dashboard/Index")));   // default on
            var off = await admin.PostAsync("/Account/Animations", new() { ["show"] = "false", ["returnUrl"] = "/Dashboard/Index" }, "/Dashboard/Index");
            Assert.Equal("/Dashboard/Index", off.Headers.Location?.ToString());
            Assert.Equal("off", Switch(await admin.HtmlAsync("/Dashboard/Index")));
            // a new sign-in keeps it (stored per user, not per browser)
            var again = await UserClient.LoginAsync(_app, "zz_test_scene_admin");
            Assert.Equal("off", Switch(await again.HtmlAsync("/Dashboard/Index")));
            await again.PostAsync("/Account/Animations", new() { ["show"] = "true", ["returnUrl"] = "https://example.invalid/" }, "/Dashboard/Index");
            Assert.Equal("on", Switch(await again.HtmlAsync("/Dashboard/Index")));
        }

        [Fact]
        public async Task The_gallery_answers_in_development_with_every_scene()
        {
            var html = await UserClient.Anonymous(_app).HtmlAsync("/Dev/Scenes");
            foreach (var name in TOP4HoneyChains.Web.TagHelpers.HoneySceneTagHelper.Names)
                Assert.Contains($"id=\"g-{name}\"", html);
            Assert.Contains("noindex", html);
        }

        [Fact]
        public async Task Pages_carry_their_scene_as_a_static_frame_and_the_tour_of_the_role()
        {
            await _app.CreateUserAsync("zz_test_scene_admin2", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "zz_test_scene_admin2");
            var page = await admin.HtmlAsync("/Dashboard/Index");
            Assert.Contains("id=\"hs-tour\"", page);
            Assert.Contains("\"target\":\"dashboard\"", page);
            Assert.Contains("data-tour=\"dashboard\"", page);
            Assert.Contains("data-tour-open", page);
            Assert.Contains("/js/honey-scene.js", page);
            Assert.Contains("/css/honey-scene.css", page);
        }
    }
}
