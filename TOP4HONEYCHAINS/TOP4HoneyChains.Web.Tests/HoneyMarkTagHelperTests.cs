using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TOP4HoneyChains.Web.TagHelpers;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // 2026-11-26: <honey-mark> renders the project's emblem (docs/brand/logo-2026-11), never a redrawn mark
    public class HoneyMarkTagHelperTests
    {
        private static (string Html, TagHelperOutput Output) Render(string mode, int size, string? label = null)
        {
            var helper = new HoneyMarkTagHelper(new HttpContextAccessor { HttpContext = new DefaultHttpContext() }) { Mode = mode, Size = size, Label = label };
            var context = new TagHelperContext(new TagHelperAttributeList(), new Dictionary<object, object>(), "id");
            var output = new TagHelperOutput("honey-mark", new TagHelperAttributeList(),
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
            helper.Process(context, output);
            return (output.Content.GetContent(), output);
        }

        [Fact]
        public void The_mark_is_the_emblem_in_webp_and_png_with_its_size()
        {
            var (html, output) = Render("slow", 120);
            Assert.Equal("span", output.TagName);
            Assert.Contains("srcset=\"/img/brand/logo-emblem.webp\"", html);
            Assert.Contains("src=\"/img/brand/logo-emblem.png\"", html);
            Assert.Contains("height=\"120\"", html);
            Assert.Contains("aria-hidden", output.Attributes.Select(a => a.Name));
            Assert.DoesNotContain("<svg", html);
            Assert.Contains("logo-emblem-128.png", Render("hover", 40).Html);   // small copies for small sizes
        }

        [Fact]
        public void A_label_becomes_the_alt_text()
        {
            var (html, output) = Render("once", 54, "Nothing to do");
            Assert.Contains("alt=\"Nothing to do\"", html);
            Assert.DoesNotContain("aria-hidden", output.Attributes.Select(a => a.Name));
        }
    }
}
