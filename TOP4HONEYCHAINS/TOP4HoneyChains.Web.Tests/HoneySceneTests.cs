using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using TOP4HoneyChains.Web.Controllers;
using TOP4HoneyChains.Web.TagHelpers;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-78 P4 (D78-SPEC 7, D-83): the explanatory scenes and the gallery
    public class HoneySceneTests
    {
        private sealed class Words : IStringLocalizer<SharedResource>
        {
            public LocalizedString this[string name] => new(name, name);
            public LocalizedString this[string name, params object[] arguments] => new(name, name);
            public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Array.Empty<LocalizedString>();
        }

        private static string Render(HoneySceneTagHelper helper)
        {
            var context = new TagHelperContext(new TagHelperAttributeList(), new Dictionary<object, object>(), "t");
            var output = new TagHelperOutput("honey-scene", new TagHelperAttributeList(), (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
            helper.Process(context, output);
            using var writer = new StringWriter();
            output.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
            return writer.ToString();
        }

        [Fact]
        public void Ids_are_unique_when_scenes_share_a_page()
        {
            var http = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
            var html = string.Concat(HoneySceneTagHelper.Names.Concat(HoneySceneTagHelper.Names)
                .Select(n => Render(new HoneySceneTagHelper(http, new Words()) { Name = n })));
            var ids = Regex.Matches(html, "\\sid=\"([^\"]+)\"").Select(m => m.Groups[1].Value).ToList();
            Assert.NotEmpty(ids);
            Assert.Equal(ids.Count, ids.Distinct().Count());
            // every reference points at an id of the same page
            foreach (Match m in Regex.Matches(html, "url\\(#([^)]+)\\)"))
                Assert.Contains(m.Groups[1].Value, ids);
        }

        [Theory]
        [MemberData(nameof(Scenes))]
        public void A_scene_is_its_final_frame_hidden_from_screen_readers_small_and_short(string name)
        {
            var http = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
            var html = Render(new HoneySceneTagHelper(http, new Words()) { Name = name, Trigger = "first" });
            Assert.DoesNotContain("is-playing", html);                         // rendered as the static final frame
            Assert.Contains("class=\"hs-art\" aria-hidden=\"true\" focusable=\"false\"", html);
            Assert.Contains("<figcaption class=\"hs-caption\">Scene." + name, html);   // the caption is HTML, translated
            Assert.DoesNotMatch("<(a|input|select|textarea)\\b|tabindex=", Regex.Replace(html, "<figcaption.*</figcaption>", "", RegexOptions.Singleline));
            var svg = HoneySceneTagHelper.Template(name);
            Assert.True(svg.Length <= 8 * 1024, $"{name} is {svg.Length} bytes");
            XDocument.Parse(svg);                                                 // well-formed
            Assert.DoesNotMatch("<text\\b", svg);                                // no text inside the drawing (7.2)
            Assert.InRange(HoneySceneTagHelper.Durations[name], 600, 1500);       // at most 1.5 s
        }

        public static IEnumerable<object[]> Scenes() => HoneySceneTagHelper.Names.Select(n => new object[] { n });

        [Fact]
        public void Every_scene_has_its_caption_in_five_languages()
        {
            var dir = Path.Combine(Root(), "TOP4HoneyChains.Web", "Resources");
            foreach (var lang in new[] { "en", "tr", "es", "pl", "lv" })
            {
                var resx = File.ReadAllText(Path.Combine(dir, $"SharedResource.{lang}.resx"));
                foreach (var name in HoneySceneTagHelper.Names)
                    Assert.Contains($"<data name=\"Scene.{name}\"", resx);
            }
        }

        [Fact]
        public void Reduced_motion_and_the_switch_stop_every_scene_and_only_transform_and_opacity_move()
        {
            var css = File.ReadAllText(Path.Combine(Root(), "TOP4HoneyChains.Web", "wwwroot", "css", "honey-scene.css"));
            Assert.Contains("@media (prefers-reduced-motion: reduce) { .hs.is-playing .hs-art *, .hs.is-playing .hs-bee { animation: none !important; } }", css);
            Assert.Contains("html[data-scenes=\"off\"] .hs.is-playing .hs-art *", css);
            Assert.DoesNotContain("infinite", css);                               // no loops
            foreach (Match k in Regex.Matches(css, "@keyframes hs-[^{]+\\{(.*?)\\}\\s*\\}", RegexOptions.Singleline))
            {
                var props = Regex.Matches(k.Groups[1].Value, "([a-z-]+)\\s*:").Select(m => m.Groups[1].Value).Distinct();
                Assert.All(props, p => Assert.Contains(p, new[] { "transform", "opacity", "stroke-dashoffset" }));
            }
            foreach (Match d in Regex.Matches(css, "animation: hs-[a-z0-9-]+ ([0-9.]+)s"))
                Assert.True(double.Parse(d.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) <= 1.5, d.Value);
        }

        private sealed class Env : IWebHostEnvironment
        {
            public string EnvironmentName { get; set; } = "Production";
            public string ApplicationName { get; set; } = "TOP4HoneyChains.Web";
            public string WebRootPath { get; set; } = "";
            public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
            public string ContentRootPath { get; set; } = "";
            public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        }

        [Theory]
        [InlineData("Production")]
        [InlineData("Staging")]
        public void The_gallery_is_not_reachable_outside_development(string environment) =>
            Assert.IsType<NotFoundResult>(new DevController(new Env { EnvironmentName = environment }).Scenes());

        [Fact]
        public void Every_role_gets_a_tour_of_three_to_five_steps_naming_no_url()
        {
            var roles = new[] { "OrgAdmin", "Staff", "FieldInspector", "SampleAcceptanceOfficer", "QualityApprover", "Analyst", "LabManager", "RegistrationOfficer", "ComplianceOfficer" };
            foreach (var steps in roles.Select(r => Services.UiPreferences.TourFor(r, false, false))
                         .Append(Services.UiPreferences.TourFor(null, true, false)).Append(Services.UiPreferences.TourFor(null, false, true)))
            {
                Assert.InRange(steps.Count, 3, 5);
                Assert.All(steps, s => Assert.DoesNotContain("/", s.Target));
            }
        }

        private static string Root()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "TOP4HoneyChains.Web")))
                dir = dir.Parent;
            return dir!.FullName;
        }
    }
}
