using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Filters;
using TOP4HoneyChains.Web.Services;

namespace TOP4HoneyChains.Web.TagHelpers
{
    // D-86: on a demo site every page gets a fixed strip at the bottom: "Demo environment – no real SMS", with the link to the
    // demo SMS inbox for signed-in panel users. Once per request (views and layouts can both carry a <body>); hidden in print.
    [HtmlTargetElement("body")]
    public class DemoBannerTagHelper : TagHelper
    {
        private const string Done = "D86.DemoBanner";

        private readonly DemoMode _demo;
        private readonly IHttpContextAccessor _http;
        private readonly IStringLocalizer<SharedResource> _l;
        private readonly TOP4HoneyChainsContext _db;
        private readonly IMemoryCache _cache;

        public DemoBannerTagHelper(DemoMode demo, IHttpContextAccessor http, IStringLocalizer<SharedResource> l, TOP4HoneyChainsContext db, IMemoryCache cache)
        {
            _demo = demo;
            _http = http;
            _l = l;
            _db = db;
            _cache = cache;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var http = _http.HttpContext;
            if (!_demo.Enabled || http == null || http.Items.ContainsKey(Done))
                return;
            http.Items[Done] = true;

            var enc = HtmlEncoder.Default;
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var link = userId != null && !await BeekeeperBoundaryMiddleware.IsBeekeeperAsync(userId, _db, _cache)
                ? $" <a href=\"/Demo/Inbox\" id=\"demo-inbox-link\" style=\"color:#FFD27A;font-weight:700;text-decoration:underline;white-space:nowrap\">{enc.Encode(_l["Demo.InboxLink"])}</a>"
                : "";
            output.PreContent.AppendHtml(
                "<style>@media print{#demo-banner,#demo-banner-space{display:none!important}}</style>"
                + "<div id=\"demo-banner\" role=\"note\" style=\"position:fixed;left:0;right:0;bottom:0;z-index:2000;background:#3B2A06;color:#FFF7E6;"
                + "font:600 13px/1.4 system-ui,'Segoe UI',sans-serif;padding:7px 16px;text-align:center;box-shadow:0 -2px 8px rgba(0,0,0,.25)\">"
                + "<span style=\"background:#F0B44A;color:#3B2A06;border-radius:4px;padding:1px 6px;margin-right:8px;font-weight:800\">DEMO</span>"
                + enc.Encode(_l["Demo.Banner"]) + link + "</div>");
            output.PostContent.AppendHtml("<div id=\"demo-banner-space\" style=\"height:44px\" aria-hidden=\"true\"></div>");
        }
    }
}
