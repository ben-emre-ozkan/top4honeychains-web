using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;

namespace TOP4HoneyChains.Web.TagHelpers
{
    // D-78 P4 (D78-SPEC 7, D-83): an explanatory scene, the "Explanation" level of docs/brand/MOTION.md.
    //   <honey-scene name="sample" trigger="now|first|session|click|manual" for="#add-container" replay="true" />
    // - the drawing (Scenes/{name}.svg, embedded) is the scene's FINAL frame: shown as it is, it is the static illustration for
    //   reduced motion and for users who turned "Show animations" off; the class is-playing (js/honey-scene.js) plays the CSS
    //   timeline (css/honey-scene.css) that leads to it;
    // - every id and url(#…) gets a prefix unique within the request, so scenes can share a page;
    // - the drawing is aria-hidden; the caption is HTML (translated, read by screen readers); nothing is told only by motion;
    // - trigger: now (plays once on load: the action just happened), first (first visit per user and scene), session (once per
    //   browser session), click (when the element "for" is clicked the first time), manual (only by the replay button).
    [HtmlTargetElement("honey-scene", TagStructure = TagStructure.WithoutEndTag)]
    public class HoneySceneTagHelper : TagHelper
    {
        public static readonly string[] Names = { "sample", "delivery", "seal", "send", "reception", "ship", "empty", "journey" };

        // timeline length per scene (ms); 7.2: at most 1.5 s
        public static readonly IReadOnlyDictionary<string, int> Durations = new Dictionary<string, int>
        {
            ["sample"] = 1200, ["delivery"] = 1300, ["seal"] = 1000, ["send"] = 1200, ["reception"] = 1200, ["ship"] = 1200, ["empty"] = 1000, ["journey"] = 1400
        };

        private static readonly ConcurrentDictionary<string, string> Templates = new();

        private readonly IHttpContextAccessor _http;
        private readonly IStringLocalizer<SharedResource> _l;

        public HoneySceneTagHelper(IHttpContextAccessor http, IStringLocalizer<SharedResource> l)
        {
            _http = http;
            _l = l;
        }

        public string Name { get; set; } = "";
        public string Trigger { get; set; } = "first";
        public string? For { get; set; }
        public bool Replay { get; set; } = true;
        public bool Caption { get; set; } = true;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!Names.Contains(Name))
                throw new ArgumentException("Unknown scene: " + Name);
            var prefix = NextPrefix();
            var svg = Template(Name)
                .Replace("id=\"s-", $"id=\"{prefix}-", StringComparison.Ordinal)
                .Replace("url(#s-", $"url(#{prefix}-", StringComparison.Ordinal)
                .Replace("<svg ", "<svg class=\"hs-art\" aria-hidden=\"true\" focusable=\"false\" ", StringComparison.Ordinal);

            output.TagName = "figure";
            output.TagMode = TagMode.StartTagAndEndTag;
            var extra = output.Attributes.TryGetAttribute("class", out var c) ? " " + c.Value : "";
            output.Attributes.SetAttribute("class", $"hs hs-{Name}{extra}");
            output.Attributes.SetAttribute("id", prefix);
            output.Attributes.SetAttribute("data-scene", Name);
            output.Attributes.SetAttribute("data-trigger", Trigger);
            output.Attributes.SetAttribute("data-duration", Durations[Name]);
            if (For != null)
                output.Attributes.SetAttribute("data-for", For);

            var caption = _l["Scene." + Name].Value;
            var replay = Replay
                ? $"<button type=\"button\" class=\"hs-replay\" data-scene-replay=\"{prefix}\" aria-label=\"{WebUtility.HtmlEncode(_l["Scene.Replay"].Value)}: {WebUtility.HtmlEncode(caption)}\" title=\"{WebUtility.HtmlEncode(_l["Scene.Replay"].Value)}\">" +
                  "<svg viewBox=\"0 0 20 20\" width=\"16\" height=\"16\" aria-hidden=\"true\" focusable=\"false\"><circle cx=\"10\" cy=\"10\" r=\"8\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\"/><path d=\"M10 9v5M10 6.2v.1\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\"/></svg></button>"
                : "";
            var figcaption = Caption ? $"<figcaption class=\"hs-caption\">{WebUtility.HtmlEncode(caption)}{replay}</figcaption>" : replay;
            // S4: the logo bee (the mark in slow mode) lands on the middle cell of the comb
            if (Name == "empty")
                svg = $"<span class=\"hs-stage\">{svg}<span class=\"hs-bee\">{HoneyMarkTagHelper.Decorative(prefix + "b", 30, "slow")}</span></span>";
            output.Content.SetHtmlContent(svg + figcaption);
        }

        // hs1, hs2, … per request
        private string NextPrefix()
        {
            var items = _http.HttpContext?.Items;
            if (items == null)
                return "hs" + Guid.NewGuid().ToString("N")[..8];
            var n = items.TryGetValue(typeof(HoneySceneTagHelper), out var v) && v is int i ? i + 1 : 1;
            items[typeof(HoneySceneTagHelper)] = n;
            return "hs" + n;
        }

        public static string Template(string name) => Templates.GetOrAdd(name, n =>
        {
            using var stream = typeof(HoneySceneTagHelper).Assembly.GetManifestResourceStream($"TOP4HoneyChains.Web.Scenes.{n}.svg")
                ?? throw new InvalidOperationException("Embedded scene missing: " + n);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd().Trim();
        });
    }
}
