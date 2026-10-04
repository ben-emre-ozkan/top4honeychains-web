using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // Start page of organization members: the active membership, the switcher, the lots and batches the
    // active organization works on (org-scoped, ROLE-MODEL section 14) and, for own batches, the
    // finalize request and its approval by a second person (D-27)
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.PanelUser)]
    public class OrgWorkspaceController : Controller
    {
        // Who may request finalizing an own batch, and who finalizes it (four-eyes)
        private static readonly string[] FinalizeRequestRoles = { OrgRoles.Staff, OrgRoles.OrgAdmin };
        private static readonly string[] FinalizeApproveRoles = { OrgRoles.QualityApprover };

        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;
        private readonly OrgScope _scope;
        private readonly CcpResponsibilityService _resp;
        private readonly CcpIntegrityService _ccp;
        private readonly AuditWriter _audit;

        public OrgWorkspaceController(TOP4HoneyChainsContext context, ICurrentOrg current, OrgScope scope,
            CcpResponsibilityService resp, CcpIntegrityService ccp, AuditWriter audit)
        {
            _context = context;
            _current = current;
            _scope = scope;
            _resp = resp;
            _ccp = ccp;
            _audit = audit;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Active = await _current.GetAsync();
            ViewBag.Memberships = await _current.GetMembershipsAsync();
            var lots = await (await _scope.VisibleLotsAsync())
                .Include(r => r.RawHoneyProductionApiary)
                .Include(r => r.RawHoneyProductionHoneyTypeNavigation)
                .Include(r => r.RawHoneyCCPAssignments).ThenInclude(a => a.CCP)
                .OrderByDescending(r => r.RawHoneyProductionId)
                .Take(200)
                .ToListAsync();
            var batches = await (await _scope.VisibleBatchesAsync())
                .Include(f => f.FinalHoneyCCPAssignments).ThenInclude(a => a.CCP)
                .OrderByDescending(f => f.FinalHoneyProductionId)
                .Take(200)
                .ToListAsync();

            // Only the control points the active organization may open are shown
            var readable = new Dictionary<string, HashSet<int>>();
            foreach (var l in lots)
                readable["raw" + l.RawHoneyProductionId] = await _resp.ReadableCcpIdsAsync("raw", l.RawHoneyProductionId, l.RawHoneyCCPAssignments.Select(a => a.RawHoneyCCPAssignmentCCP));
            foreach (var b in batches)
                readable["batch" + b.FinalHoneyProductionId] = await _resp.ReadableCcpIdsAsync("batch", b.FinalHoneyProductionId, b.FinalHoneyCCPAssignments.Select(a => a.FinalHoneyCCPAssignmentCCP));

            ViewBag.Lots = lots;
            ViewBag.Batches = batches;
            ViewBag.Readable = readable;
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

        // ---------------------------------------------------------------- own batch and finalize

        private async Task<FinalHoneyProduction?> OwnBatchAsync(int id)
        {
            var active = await _current.GetAsync();
            if (active == null || await _resp.BatchOwnerAsync(id) != active.OrgId)
                return null;
            return await _context.FinalHoneyProductions
                .Include(f => f.FinalHoneyProductionDetails).ThenInclude(d => d.RawHoneyProduction).ThenInclude(r => r.RawHoneyProductionApiary)
                .Include(f => f.FinalHoneyProductionDetails).ThenInclude(d => d.RawHoneyProduction).ThenInclude(r => r.RawHoneyCCPAssignments).ThenInclude(a => a.CCP)
                .Include(f => f.FinalHoneyCCPAssignments).ThenInclude(a => a.CCP)
                .FirstOrDefaultAsync(f => f.FinalHoneyProductionId == id);
        }

        public async Task<IActionResult> Batch(int id)
        {
            var batch = await OwnBatchAsync(id);
            if (batch == null)
                return NotFound();
            var active = (await _current.GetAsync())!;
            var readable = new Dictionary<string, HashSet<int>>
            {
                ["batch" + id] = await _resp.ReadableCcpIdsAsync("batch", id, batch.FinalHoneyCCPAssignments.Select(a => a.FinalHoneyCCPAssignmentCCP))
            };
            foreach (var d in batch.FinalHoneyProductionDetails)
                readable["raw" + d.RawHoneyProductionId] = await _resp.ReadableCcpIdsAsync("raw", d.RawHoneyProductionId,
                    d.RawHoneyProduction.RawHoneyCCPAssignments.Select(a => a.RawHoneyCCPAssignmentCCP));

            ViewBag.Readable = readable;
            ViewBag.Blockers = batch.FinalHoneyProductionIsFinalized ? new List<string>() : await _ccp.GetFinalizeBlockersAsync(id);
            ViewBag.CanRequest = active.IsVerified && FinalizeRequestRoles.Contains(active.OrgRole);
            ViewBag.CanFinalize = active.IsVerified && FinalizeApproveRoles.Contains(active.OrgRole);
            ViewBag.Done = await _ccp.DonePredicateAsync();
            ViewBag.Controller = "My organization";
            ViewBag.Action = "Batch";
            return View(batch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestFinalize(int id)
        {
            var batch = await OwnBatchAsync(id);
            if (batch == null)
                return NotFound();
            var active = (await _current.GetAsync())!;
            var blockers = await _ccp.GetFinalizeBlockersAsync(id);
            if (batch.FinalHoneyProductionIsFinalized)
                TempData["Error"] = "This batch is already finalized.";
            else if (!active.IsVerified || !FinalizeRequestRoles.Contains(active.OrgRole))
                TempData["Error"] = $"Finalizing is requested by {string.Join(" or ", FinalizeRequestRoles)} of a verified organization.";
            else if (blockers.Count > 0)
                TempData["Error"] = "The batch cannot be finalized yet: " + string.Join(" ", blockers);
            else
            {
                batch.FinalizeRequestedByUserId = _current.UserId;
                batch.FinalizeRequestedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Finalizing requested; a QualityApprover (another person) must confirm it.";
            }
            return RedirectToAction(nameof(Batch), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalize(int id)
        {
            var batch = await OwnBatchAsync(id);
            if (batch == null)
                return NotFound();
            var active = (await _current.GetAsync())!;
            var blockers = await _ccp.GetFinalizeBlockersAsync(id);
            var self = batch.FinalizeRequestedByUserId == _current.UserId;
            string? error = null;
            if (batch.FinalHoneyProductionIsFinalized)
                error = "This batch is already finalized.";
            else if (!active.IsVerified || !FinalizeApproveRoles.Contains(active.OrgRole))
                error = $"Only a {string.Join(" or ", FinalizeApproveRoles)} of a verified organization can finalize.";
            else if (batch.FinalizeRequestedByUserId == null)
                error = "Finalizing must be requested first.";
            else if (self && !active.FourEyesExempt)
                error = "Four-eyes: the person who requested finalizing cannot confirm it.";
            else if (blockers.Count > 0)
                error = "The batch cannot be finalized yet: " + string.Join(" ", blockers);

            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Batch), new { id });
            }

            batch.FinalHoneyProductionIsFinalized = true;
            batch.FinalHoneyProductionFinalizedDate = DateTime.UtcNow;
            batch.FinalizedByUserId = _current.UserId;
            batch.SelfFinalized = self;
            await Origin.StoreAsync(_context, id);   // D-31: country shares by weight, a snapshot
            await _context.SaveChangesAsync();
            await _audit.WriteAsync("Finalize", "FinalHoneyProductions", id.ToString(), null, new { batch.FinalizeRequestedByUserId }, self ? "SelfApproved" : null);
            TempData["Success"] = self ? "Batch finalized under the four-eyes exemption (flagged in the audit log)." : "Batch finalized.";
            return RedirectToAction(nameof(Batch), new { id });
        }
    }
}
