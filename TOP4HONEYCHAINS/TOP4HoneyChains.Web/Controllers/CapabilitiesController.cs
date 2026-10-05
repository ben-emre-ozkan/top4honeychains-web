using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // Read-only overview of the organization capabilities (D-22): what each one enables, the CCPs that need it
    // (CCP responsibility matrix), the roles that enter and approve those CCPs, and how many organizations have it.
    // Capabilities are assigned on the organization page and in Roles and permissions; the list itself is fixed (004).
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.PlatformAdmin)]
    public class CapabilitiesController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;

        public CapabilitiesController(TOP4HoneyChainsContext context) => _context = context;

        public record CcpLink(int Id, string Code, string? Title);

        public class Row
        {
            public string Code { get; set; } = "";
            public string Description { get; set; } = "";
            public List<CcpLink> Ccps { get; set; } = new();
            public List<string> EnterRoles { get; set; } = new();
            public List<string> ApproveRoles { get; set; } = new();
            public int Organizations { get; set; }
            public int VerifiedOrganizations { get; set; }
        }

        public async Task<IActionResult> Index()
        {
            var capabilities = await _context.Capabilities.AsNoTracking().OrderBy(c => c.SortOrder).ToListAsync();
            var ccps = await _context.CriticalControlPoints.AsNoTracking().ToDictionaryAsync(c => c.Id);
            var links = await _context.CcpResponsibilityCapabilities.AsNoTracking().ToListAsync();
            var roles = await _context.CcpResponsibilityRoles.AsNoTracking().ToListAsync();
            var orgs = await _context.OrgCapabilities.AsNoTracking()
                .Where(o => !o.Org!.IsPlatform || o.Capability == Capabilities.PlatformAdmin)
                .Select(o => new { o.Capability, o.Org!.Status }).ToListAsync();

            var rows = capabilities.Select(c =>
            {
                var ccpIds = links.Where(l => l.Capability == c.Code).Select(l => l.CcpId).Distinct().OrderBy(id => id).ToList();
                return new Row
                {
                    Code = c.Code,
                    Description = c.Description,
                    Ccps = ccpIds.Where(ccps.ContainsKey).Select(id => new CcpLink(id, ccps[id].Code, ccps[id].Title)).ToList(),
                    EnterRoles = roles.Where(r => ccpIds.Contains(r.CcpId) && r.Action == CcpActions.Enter).Select(r => r.OrgRole).Distinct().ToList(),
                    ApproveRoles = roles.Where(r => ccpIds.Contains(r.CcpId) && r.Action == CcpActions.Approve).Select(r => r.OrgRole).Distinct().ToList(),
                    Organizations = orgs.Count(o => o.Capability == c.Code),
                    VerifiedOrganizations = orgs.Count(o => o.Capability == c.Code && o.Status == OrgStatus.Verified)
                };
            }).ToList();

            ViewBag.Controller = "Org.Capabilities";
            ViewBag.Action = "Index";
            return View(rows);
        }
    }
}
