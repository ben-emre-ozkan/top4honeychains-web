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
        // D-56: rights of organization roles (editable matrix)
        private TOP4HoneyChains.Web.Services.OrgModel.Permissions Perm => HttpContext.RequestServices.GetRequiredService<TOP4HoneyChains.Web.Services.OrgModel.Permissions>();
        private string[] FinalizeRequestRoles => Perm.Roles(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Finalize, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Enter);
        private string[] FinalizeApproveRoles => Perm.Roles(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Finalize, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Approve);

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
            // O-9: a verified organization without a Compliance Officer sees a warning (never blocked)
            if (ViewBag.Active is ActiveMembership a && a.IsVerified)
            {
                ViewBag.MissingComplianceOfficer = !ComplianceService.HasComplianceOfficer(await _context.Memberships.AsNoTracking().Where(m => m.OrgId == a.OrgId).ToListAsync());
                ViewBag.ComplianceDue = HttpContext.RequestServices.GetRequiredService<ComplianceService>().GraceUntil;
            }
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
            ViewBag.CanCreateBatch = PlantBatchService.Denied(ViewBag.Active as ActiveMembership, Perm) == null;
            ViewBag.Controller = "My organization";
            ViewBag.Action = "Index";
            return View();
        }

        // ---------------------------------------------------------------- new batch (D-88)

        public class NewBatchModel
        {
            public string? Title { get; set; }
            public DateOnly? Date { get; set; }
            public List<PlantBatchService.Lot> Lots { get; set; } = new();
            // the kilograms entered per lot (only for ticked lots on a post)
            public Dictionary<int, decimal> Amounts { get; set; } = new();
            public HashSet<int> Chosen { get; set; } = new();
            public List<string> Problems { get; set; } = new();
        }

        private PlantBatchService Batches => HttpContext.RequestServices.GetRequiredService<PlantBatchService>();

        public async Task<IActionResult> NewBatch()
        {
            var active = await _current.GetAsync();
            if (PlantBatchService.Denied(active, Perm) is string denied)
            {
                TempData["Error"] = denied;
                return RedirectToAction(nameof(Index));
            }
            var lots = await Batches.EligibleLotsAsync(active!.OrgId);
            ViewBag.Controller = "My organization";
            ViewBag.Action = "New batch";
            return View(new NewBatchModel
            {
                Date = DateOnly.FromDateTime(DateTime.Today),
                Lots = lots,
                Amounts = lots.ToDictionary(l => l.LotId, l => l.Left)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewBatch(string? title, DateOnly? date, int[]? lots, [FromForm(Name = "kg")] Dictionary<int, decimal>? kg)
        {
            var active = await _current.GetAsync();
            if (PlantBatchService.Denied(active, Perm) is string denied)
            {
                TempData["Error"] = denied;
                return RedirectToAction(nameof(Index));
            }
            kg ??= new();
            var chosen = (lots ?? Array.Empty<int>()).Distinct().ToList();
            var amounts = chosen.ToDictionary(id => id, id => kg.TryGetValue(id, out var v) ? v : 0m);
            var eligible = await Batches.EligibleLotsAsync(active!.OrgId);
            var problems = PlantBatchService.Check(title, date, amounts, eligible, DateOnly.FromDateTime(DateTime.Today));
            if (problems.Count > 0)
            {
                ViewBag.Controller = "My organization";
                ViewBag.Action = "New batch";
                var shown = eligible.ToDictionary(l => l.LotId, l => kg.TryGetValue(l.LotId, out var v) ? v : l.Left);
                return View(new NewBatchModel { Title = title, Date = date, Lots = eligible, Amounts = shown, Chosen = chosen.ToHashSet(), Problems = problems });
            }
            var created = await Batches.CreateAsync(active.OrgId, title!, date!.Value, amounts);
            TempData["Success"] = $"Batch created with {created.CcpCount} control points; they are in My work.";
            return RedirectToAction(nameof(Batch), new { id = created.BatchId });
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
            // D-98: the final honey result (company path, honey from more than one apiary)
            var finals = HttpContext.RequestServices.GetRequiredService<BatchSampleService>();
            var final = await finals.StateAsync(id);
            ViewBag.Final = final;
            if (final.Applies && !batch.FinalHoneyProductionIsFinalized)
            {
                ViewBag.FinalDenied = await finals.DeniedAsync(id);
                if (ViewBag.FinalDenied == null)
                    ViewBag.Labs = await _context.Organizations.AsNoTracking()
                        .Where(o => !o.IsPlatform && o.Status == OrgStatus.Verified && o.Capabilities.Any(c => c.Capability == Capabilities.Laboratory))
                        .OrderBy(o => o.DisplayName).Select(o => new { o.OrgId, o.DisplayName }).ToDictionaryAsync(o => o.OrgId, o => o.DisplayName);
            }
            ViewBag.Controller = "My organization";
            ViewBag.Action = "Batch";
            return View(batch);
        }

        // D-98: the sample of the batch's final blend, sent to a laboratory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalSample(int id, int labOrgId, string? sealNumber, string? counterSealNumber, string? counterLocation, DateOnly? retainUntil)
        {
            if (await OwnBatchAsync(id) == null)
                return NotFound();
            var finals = HttpContext.RequestServices.GetRequiredService<BatchSampleService>();
            var (sample, error) = await finals.CreateAsync(id, labOrgId, sealNumber, counterSealNumber, counterLocation, retainUntil, DateOnly.FromDateTime(DateTime.Today));
            if (error != null)
                TempData["Error"] = error;
            else
            {
                TempData["Success"] = $"Sample {sample!.SampleCode} sent to the laboratory.";
                TempData["Scene"] = "sample";
            }
            return RedirectToAction(nameof(Batch), new { id });
        }

        // C3 (D-84): finalizing is for an organization that processes or packs honey
        private static bool MayFinalizeBatches(ActiveMembership active) => active.Has(Capabilities.Processing) || active.Has(Capabilities.Packing);

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
            else if (!MayFinalizeBatches(active))
                TempData["Error"] = "Only a processing or packing organization finalizes batches.";
            else if (blockers.Count > 0)
                TempData["Error"] = "The batch cannot be finalized yet: " + string.Join(" ", blockers);
            else
            {
                batch.FinalizeRequestedByUserId = _current.UserId;
                batch.FinalizeRequestedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Finalizing requested; a {OrgRoles.QualityApprover} (another person) must confirm it.";
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
            else if (!MayFinalizeBatches(active))
                error = "Only a processing or packing organization finalizes batches.";
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
            TempData["Sealed"] = true;   // D-67: the batch page shows the seal moment once
            return RedirectToAction(nameof(Batch), new { id });
        }
    }
}
