using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // "İşlerim": the start page of organization users with the organization model (D-44)
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.PanelUser)]
    public class MyWorkController : Controller
    {
        private readonly MyWorkService _work;
        private readonly ICurrentOrg _current;

        public MyWorkController(MyWorkService work, ICurrentOrg current)
        {
            _work = work;
            _current = current;
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
    }
}
