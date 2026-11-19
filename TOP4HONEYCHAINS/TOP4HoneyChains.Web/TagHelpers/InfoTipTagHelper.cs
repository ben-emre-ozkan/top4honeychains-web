using System.Net;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;

namespace TOP4HoneyChains.Web.TagHelpers
{
    // D-91: an info button next to a field label, the same round "i" button as the scene replay button (hs-replay).
    //   <info-tip key="Reg78.Info.Registry" about="@L["Reg78.RegistryNumber"].Value" />
    // A disclosure: a real <button> (click, tap, Enter, Space) toggles the note below it; Escape closes it (js/honey-scene.js).
    // Nothing is shown only on hover. The note is in the page flow, so a scrolling table cannot clip it.
    [HtmlTargetElement("info-tip", TagStructure = TagStructure.WithoutEndTag)]
    public class InfoTipTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _http;
        private readonly IStringLocalizer<SharedResource> _l;

        public InfoTipTagHelper(IHttpContextAccessor http, IStringLocalizer<SharedResource> l)
        {
            _http = http;
            _l = l;
        }

        // resx key of the note
        public string Key { get; set; } = "";
        // what the note is about (the field label), for the button's accessible name
        public string? About { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var id = NextId();
            var more = _l["Info.More"].Value;
            var label = string.IsNullOrWhiteSpace(About) ? more : $"{more}: {About}";
            output.TagName = null;
            output.Content.SetHtmlContent(
                $"<button type=\"button\" class=\"hs-replay info-tip-btn\" data-info-tip aria-expanded=\"false\" aria-controls=\"{id}\" aria-label=\"{WebUtility.HtmlEncode(label)}\" title=\"{WebUtility.HtmlEncode(more)}\">" +
                "<svg viewBox=\"0 0 20 20\" width=\"14\" height=\"14\" aria-hidden=\"true\" focusable=\"false\"><circle cx=\"10\" cy=\"10\" r=\"8\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\"/><path d=\"M10 9v5M10 6.2v.1\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\"/></svg></button>" +
                $"<span class=\"info-tip-note\" id=\"{id}\" role=\"note\" hidden>{WebUtility.HtmlEncode(_l[Key].Value)}</span>");
        }

        // it1, it2, … per request
        private string NextId()
        {
            var items = _http.HttpContext?.Items;
            if (items == null)
                return "it" + Guid.NewGuid().ToString("N")[..8];
            var n = items.TryGetValue(typeof(InfoTipTagHelper), out var v) && v is int i ? i + 1 : 1;
            items[typeof(InfoTipTagHelper)] = n;
            return "it" + n;
        }
    }
}
