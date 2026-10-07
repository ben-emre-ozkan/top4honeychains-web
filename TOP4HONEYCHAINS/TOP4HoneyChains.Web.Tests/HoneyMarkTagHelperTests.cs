using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TOP4HoneyChains.Web.TagHelpers;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-70: the animated mark is inlined once per copy with its own id prefix; its styles are not repeated
    public class HoneyMarkTagHelperTests
    {
        private static string Render(IHttpContextAccessor http, string mode, string? label = null)
        {
            var helper = new HoneyMarkTagHelper(http) { Mode = mode, Size = 40, Label = label };
            var context = new TagHelperContext(new TagHelperAttributeList(), new Dictionary<object, object>(), "id");
            var output = new TagHelperOutput("honey-mark", new TagHelperAttributeList(),
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
            helper.Process(context, output);
            return $"<{output.TagName} class=\"{output.Attributes["class"].Value}\"{(output.Attributes.ContainsName("data-hm-once") ? " data-hm-once" : "")}>{output.Content.GetContent()}</{output.TagName}>";
        }

        [Fact]
        public void Two_copies_on_one_page_have_unique_ids_and_references()
        {
            var http = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
            var first = Render(http, "hover");
            var second = Render(http, "loop");
            var ids = Regex.Matches(first + second, "id=\"([^\"]+)\"").Select(m => m.Groups[1].Value).ToList();
            Assert.NotEmpty(ids);
            Assert.Equal(ids.Count, ids.Distinct().Count());
            // every url(#…) points at an id of the same copy
            foreach (var html in new[] { first, second })
            {
                var own = Regex.Matches(html, "id=\"([^\"]+)\"").Select(m => m.Groups[1].Value).ToHashSet();
                foreach (Match m in Regex.Matches(html, @"url\(#([^)]+)\)"))
                    Assert.Contains(m.Groups[1].Value, own);
            }
            Assert.DoesNotContain("id=\"th-", first + second);
        }

        [Fact]
        public void The_master_styles_are_not_inlined_and_the_modes_set_their_classes()
        {
            var http = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
            var hover = Render(http, "hover");
            Assert.DoesNotContain("<style", hover);
            Assert.Contains("class=\"honey-mark\"", hover);
            Assert.Contains("aria-hidden=\"true\"", hover);       // decorative without a label
            Assert.Contains("class=\"honey-mark is-looping\"", Render(http, "loop"));
            Assert.Contains("class=\"honey-mark is-looping is-slow\"", Render(http, "slow"));
            Assert.Contains("data-hm-once", Render(http, "once"));
            var named = Render(http, "slow", "Nothing to do");
            Assert.Matches("role=\"img\" aria-labelledby=\"hm\\d+-title\"", named);
            Assert.Contains(">Nothing to do</title>", named);
        }
    }
}
