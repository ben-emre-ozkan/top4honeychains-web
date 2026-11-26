using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.TagHelpers
{
    // D-112: a TEST strip at the top of every page that shows records of the live site's test set (the Honey Story, its details and
    // parameter pages, a test beekeeper's storefront, the verification of a test laboratory's report) and of every page a test-set
    // account sees, with the link to the test SMS inbox. Once per request (views and layouts can both carry a <body>); also printed,
    // so a printed test label or page cannot pass for a real one.
    [HtmlTargetElement("body")]
    public class TestSetBannerTagHelper : TagHelper
    {
        private const string Done = "D112.TestBanner";

        private readonly IHttpContextAccessor _http;
        private readonly IStringLocalizer<SharedResource> _l;
        private readonly TOP4HoneyChainsContext _db;
        private readonly IMemoryCache _cache;

        public TestSetBannerTagHelper(IHttpContextAccessor http, IStringLocalizer<SharedResource> l, TOP4HoneyChainsContext db, IMemoryCache cache)
        {
            _http = http;
            _l = l;
            _db = db;
            _cache = cache;
        }

        // after the demo strip (order 0)
        public override int Order => 10;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var http = _http.HttpContext;
            if (http == null || http.Items.ContainsKey(Done))
                return;
            var account = await TestData.IsTestAccountAsync(_db, _cache, http.User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (!account && !TestData.PageMarked(http))
                return;
            http.Items[Done] = true;

            var enc = HtmlEncoder.Default;
            var text = TestData.PageMarked(http) ? _l["TestSet.PageBanner"] : _l["TestSet.AccountBanner"];
            var link = account
                ? $" <a href=\"/TestInbox\" id=\"test-inbox-link\" style=\"color:#fff;font-weight:700;text-decoration:underline;white-space:nowrap\">{enc.Encode(_l["TestSet.InboxLink"])}</a>"
                : "";
            output.PreContent.AppendHtml(
                "<div id=\"test-set-banner\" role=\"note\" style=\"position:relative;z-index:2001;background:#B42318;color:#fff;"
                + "font:600 14px/1.4 system-ui,'Segoe UI',sans-serif;padding:8px 16px;text-align:center\">"
                + "<span style=\"background:#fff;color:#B42318;border-radius:4px;padding:1px 8px;margin-right:8px;font-weight:800;letter-spacing:.08em\">TEST</span>"
                + enc.Encode(text) + link + "</div>");
        }
    }
}
