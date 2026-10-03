using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // Start page of organization members: the active membership, the switcher and the lots and batches
    // the active organization may see (org-scoped queries, ROLE-MODEL section 14)
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.PanelUser)]
    public class OrgWorkspaceController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;
        private readonly OrgScope _scope;

        public OrgWorkspaceController(TOP4HoneyChainsContext context, ICurrentOrg current, OrgScope scope)
        {
            _context = context;
            _current = current;
            _scope = scope;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Active = await _current.GetAsync();
            ViewBag.Memberships = await _current.GetMembershipsAsync();
            ViewBag.Lots = await (await _scope.VisibleLotsAsync())
                .Include(r => r.RawHoneyProductionApiary)
                .Include(r => r.RawHoneyProductionHoneyTypeNavigation)
                .Include(r => r.RawHoneyCCPAssignments).ThenInclude(a => a.CCP)
                .OrderByDescending(r => r.RawHoneyProductionId)
                .Take(200)
                .ToListAsync();
            ViewBag.Batches = await (await _scope.VisibleBatchesAsync())
                .Include(f => f.FinalHoneyCCPAssignments).ThenInclude(a => a.CCP)
                .OrderByDescending(f => f.FinalHoneyProductionId)
                .Take(200)
                .ToListAsync();
            ViewBag.Controller = "My organization";
            ViewBag.Action = "Index";
            return View();
        }

        // Switches the active membership; the cookie only holds a choice, every request re-checks it
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Switch(int membershipId)
        {
            var options = await _current.GetMembershipsAsync();
            if (options.All(o => o.MembershipId != membershipId))
                return NotFound();
            Response.Cookies.Append(CurrentOrg.CookieName, membershipId.ToString(), new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                IsEssential = true
            });
            return RedirectToAction(nameof(Index));
        }
    }
}
