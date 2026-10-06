using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // "İşlerim": the start page of organization users with the organization model (D-44)
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.PanelUser)]
    public class MyWorkController : Controller
    {
        public static readonly TimeSpan CountCacheFor = TimeSpan.FromSeconds(30);

        private readonly MyWorkService _work;
        private readonly ICurrentOrg _current;
        private readonly IMemoryCache _cache;

        public MyWorkController(MyWorkService work, ICurrentOrg current, IMemoryCache cache)
        {
            _work = work;
            _current = current;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Items = await _work.ItemsAsync();
            ViewBag.Active = await _current.GetAsync();
            ViewBag.Memberships = await _current.GetMembershipsAsync();
            ViewBag.Controller = "MyWork";
            ViewBag.Action = "Index";
            return View();
        }

        public record WorkCount(int Total, Dictionary<string, int> ByKind);

        // D-66: only the numbers of the page above, for the badge on the menu link. Computed by the same service, so
        // they match the page; cached per user and active membership for 30 seconds; never stored by the browser.
        [HttpGet]
        public async Task<IActionResult> Count()
        {
            Response.Headers.CacheControl = "no-store";
            var scope = _current.IsPlatformAdmin ? "admin" : (await _current.GetAsync())?.MembershipId.ToString() ?? "none";
            var count = await _cache.GetOrCreateAsync($"work-count:{_current.UserId}:{scope}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CountCacheFor;
                var items = await _work.ItemsAsync();
                return new WorkCount(items.Count, items.GroupBy(i => i.Kind).ToDictionary(g => g.Key, g => g.Count()));
            });
            return Json(count);
        }
    }
}
