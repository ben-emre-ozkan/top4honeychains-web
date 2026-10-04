using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // The CCP responsibility matrix (D-25): editable configuration, effective at once
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.PlatformAdmin)]
    public class CcpResponsibilitiesController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;

        public CcpResponsibilitiesController(TOP4HoneyChainsContext context, ICurrentOrg current)
        {
            _context = context;
            _current = current;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Ccps = await _context.CriticalControlPoints.AsNoTracking().OrderBy(c => c.Id).ToListAsync();
            ViewBag.Configs = await _context.CcpResponsibilities.AsNoTracking()
                .Include(r => r.Capabilities).Include(r => r.Roles).ToDictionaryAsync(r => r.CcpId);
            ViewBag.Controller = "CCP responsibilities";
            ViewBag.Action = "Index";
            return View();
        }

        public async Task<IActionResult> Edit(int id)
        {
            var ccp = await _context.CriticalControlPoints.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (ccp == null)
                return NotFound();
            ViewBag.Ccp = ccp;
            ViewBag.Config = await _context.CcpResponsibilities.AsNoTracking()
                .Include(r => r.Capabilities).Include(r => r.Roles).FirstOrDefaultAsync(r => r.CcpId == id);
            ViewBag.AllCapabilities = await _context.Capabilities.Where(c => c.Code != Capabilities.PlatformAdmin).OrderBy(c => c.SortOrder).Select(c => c.Code).ToListAsync();
            ViewBag.AllRoles = OrgRoles.All;
            ViewBag.Controller = "CCP responsibilities";
            ViewBag.Action = "Edit";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string ownerRule, bool requiresApproval, string[]? capabilities, string[]? enterRoles, string[]? approveRoles)
        {
            if (!await _context.CriticalControlPoints.AnyAsync(c => c.Id == id))
                return NotFound();
            capabilities ??= Array.Empty<string>();
            enterRoles ??= Array.Empty<string>();
            approveRoles ??= Array.Empty<string>();
            var knownCaps = await _context.Capabilities.Where(c => c.Code != Capabilities.PlatformAdmin).Select(c => c.Code).ToListAsync();

            string? error = null;
            if (!CcpOwnerRules.All.Contains(ownerRule))
                error = "Choose who is responsible.";
            else if (capabilities.Length == 0 || capabilities.Any(c => !knownCaps.Contains(c)))
                error = "Choose at least one capability.";
            else if (enterRoles.Length == 0 || enterRoles.Concat(approveRoles).Any(r => !OrgRoles.All.Contains(r)))
                error = "Choose at least one role that enters values.";
            else if (requiresApproval && approveRoles.Length == 0)
                error = "Choose at least one approving role, or turn approval off.";
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Edit), new { id });
            }

            var cfg = await _context.CcpResponsibilities.Include(r => r.Capabilities).Include(r => r.Roles).FirstOrDefaultAsync(r => r.CcpId == id);
            if (cfg == null)
            {
                cfg = new CcpResponsibility { CcpId = id };
                _context.CcpResponsibilities.Add(cfg);
            }
            cfg.OwnerRule = ownerRule;
            cfg.RequiresApproval = requiresApproval;
            cfg.UpdatedAt = DateTime.UtcNow;
            cfg.UpdatedByUserId = _current.UserId;
            _context.CcpResponsibilityCapabilities.RemoveRange(cfg.Capabilities.Where(c => !capabilities.Contains(c.Capability)));
            foreach (var c in capabilities.Distinct().Where(c => cfg.Capabilities.All(x => x.Capability != c)))
                cfg.Capabilities.Add(new CcpResponsibilityCapability { CcpId = id, Capability = c });
            var wanted = enterRoles.Distinct().Select(r => (CcpActions.Enter, r))
                .Concat((requiresApproval ? approveRoles : Array.Empty<string>()).Distinct().Select(r => (CcpActions.Approve, r))).ToList();
            _context.CcpResponsibilityRoles.RemoveRange(cfg.Roles.Where(x => !wanted.Contains((x.Action, x.OrgRole))));
            foreach (var (action, role) in wanted.Where(w => cfg.Roles.All(x => x.Action != w.Item1 || x.OrgRole != w.Item2)))
                cfg.Roles.Add(new CcpResponsibilityRole { CcpId = id, Action = action, OrgRole = role });
            await _context.SaveChangesAsync();
            TempData["Success"] = "Saved; the change applies at once.";
            return RedirectToAction(nameof(Index));
        }
    }
}
