using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TOP4HoneyChains.Web.TagHelpers
{
    // D-70: the animated brand mark (docs/brand/top4-mark-animated.svg) inlined as SVG.
    //   <honey-mark mode="hover|once|loop|slow" size="40" label="TOP4HoneyChains" class="…" />
    // - every id and url(#…) reference gets a prefix unique within the request, so several copies can share a page;
    // - the master's <style> is not repeated: the rules live once in wwwroot/css/honey-mark.css;
    // - without a label the mark is decorative (aria-hidden), e.g. inside a link that already has a name;
    // - mode: hover (default), once (plays on load, then on hover; honey-mark.js), loop (is-looping), slow (slow loop).
    [HtmlTargetElement("honey-mark", TagStructure = TagStructure.WithoutEndTag)]
    public class HoneyMarkTagHelper : TagHelper
    {
        private const string Resource = "TOP4HoneyChains.Web.top4-mark-animated.svg";   // wwwroot/img/brand/top4-mark-animated.svg, embedded
        private static readonly object Gate = new();
        private static string? _template;

        private readonly IHttpContextAccessor _http;

        public HoneyMarkTagHelper(IHttpContextAccessor http)
        {
            _http = http;
        }

        public string Mode { get; set; } = "hover";
        public int Size { get; set; } = 40;           // height in px; the width follows the 512:445 view box
        public string? Label { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var prefix = NextPrefix();
            var svg = Template().Replace("id=\"th-", $"id=\"{prefix}-").Replace("url(#th-", $"url(#{prefix}-");
            var width = (int)Math.Round(Size * 512.0 / 445.0);
            var classes = Mode switch
            {
                "loop" => "honey-mark is-looping",
                "slow" => "honey-mark is-looping is-slow",
                _ => "honey-mark",
            };
            var a11y = string.IsNullOrWhiteSpace(Label)
                ? "aria-hidden=\"true\" focusable=\"false\""
                : $"role=\"img\" aria-labelledby=\"{prefix}-title\"";
            var title = string.IsNullOrWhiteSpace(Label) ? "" : $"<title id=\"{prefix}-title\">{System.Net.WebUtility.HtmlEncode(Label)}</title>";
            svg = svg.Replace("<svg ", $"<svg class=\"{classes}\" width=\"{width}\" height=\"{Size}\" {a11y} ", StringComparison.Ordinal)
                     .Replace("<defs>", title + "<defs>", StringComparison.Ordinal);

            output.TagName = "span";
            output.TagMode = TagMode.StartTagAndEndTag;
            var extra = output.Attributes.TryGetAttribute("class", out var c) ? " " + c.Value : "";
            output.Attributes.SetAttribute("class", "hm" + extra);
            if (Mode == "once")
                output.Attributes.SetAttribute("data-hm-once", "");
            output.Content.SetHtmlContent(svg);
        }

        // hm1, hm2, … per request
        private string NextPrefix()
        {
            var items = _http.HttpContext?.Items;
            if (items == null)
                return "hm" + Guid.NewGuid().ToString("N")[..8];
            var n = items.TryGetValue(typeof(HoneyMarkTagHelper), out var v) && v is int i ? i + 1 : 1;
            items[typeof(HoneyMarkTagHelper)] = n;
            return "hm" + n;
        }

        // the master without its <style>, title, desc and fixed size; read once
        // D-78 P4: the decorative mark inside another drawing (the bee that lands in the empty My work scene)
        public static string Decorative(string prefix, int size, string mode)
        {
            var width = (int)Math.Round(size * 512.0 / 445.0);
            var classes = mode switch { "loop" => "honey-mark is-looping", "slow" => "honey-mark is-looping is-slow", _ => "honey-mark" };
            return Template().Replace("id=\"th-", $"id=\"{prefix}-").Replace("url(#th-", $"url(#{prefix}-")
                .Replace("<svg ", $"<svg class=\"{classes}\" width=\"{width}\" height=\"{size}\" aria-hidden=\"true\" focusable=\"false\" ", StringComparison.Ordinal);
        }

        private static string Template()
        {
            if (_template != null)
                return _template;
            lock (Gate)
            {
                if (_template != null)
                    return _template;
                using var stream = typeof(HoneyMarkTagHelper).Assembly.GetManifestResourceStream(Resource)
                    ?? throw new InvalidOperationException("Embedded resource missing: " + Resource);
                using var reader = new StreamReader(stream);
                var raw = reader.ReadToEnd();
                raw = Regex.Replace(raw, @"<\?xml[^>]*\?>", "");
                raw = Regex.Replace(raw, @"<style>.*?</style>", "", RegexOptions.Singleline);
                raw = Regex.Replace(raw, @"<title[^>]*>.*?</title>|<desc[^>]*>.*?</desc>", "", RegexOptions.Singleline);
                // the root: keep the namespace and the view box only
                raw = Regex.Replace(raw, @"<svg[^>]*>", "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 512 445\">", RegexOptions.None, TimeSpan.FromSeconds(1));
                raw = Regex.Replace(raw, @">\s+<", "><");
                _template = raw.Trim();
                return _template;
            }
        }
    }
}
