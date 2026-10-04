using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // Process flow of a lot or batch (WP4 "List of Samples -> Process Flow -> CCP", D-44): the CCP stages as a
    // timeline with status, responsible organization and approval state. No values are shown; a stage links to
    // its CCP page only when the user may read it.
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.PanelUser)]
    public class FlowController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;
        private readonly CcpResponsibilityService _resp;
        private readonly CustodyService _custody;

        public FlowController(TOP4HoneyChainsContext context, ICurrentOrg current, CcpResponsibilityService resp, CustodyService custody)
        {
            _context = context;
            _current = current;
            _resp = resp;
            _custody = custody;
        }

        public record Stage(int CcpId, string? Code, string? Title, string? Step, bool Assigned, int Status, string? StatusName,
            bool Approved, DateTime? ApprovedAt, bool SelfApproved, List<string> Responsible, bool Readable, string? Url);

        public record SampleRow(int SampleId, string Code, string Lab, string Status, bool? SealIntact, string? ReportStatus, string? ReportCode);

        public class Model
        {
            public string Kind { get; set; } = "";
            public int Id { get; set; }
            public string Title { get; set; } = "";
            public List<Stage> Stages { get; } = new();
            public List<SampleRow> Samples { get; } = new();
            public List<(int LotId, string? HoneyType, decimal Used)> Lots { get; } = new();
            public string? Intake { get; set; }
            public bool Finalized { get; set; }
        }

        private async Task<Dictionary<int, string>> OrgNamesAsync(IEnumerable<int> ids)
        {
            var list = ids.Distinct().ToList();
            return await _context.Organizations.AsNoTracking().Where(o => list.Contains(o.OrgId)).ToDictionaryAsync(o => o.OrgId, o => o.DisplayName);
        }

        private async Task<List<Stage>> StagesAsync(string kind, int id, string appliesTo,
            List<(int Ccp, int Status, DateTime? ApprovedAt, bool Self)> assigned)
        {
            var ccps = await _context.CriticalControlPoints.AsNoTracking().OrderBy(c => c.Id).ToListAsync();
            var statuses = await _context.AssignmentStatuses.AsNoTracking().ToDictionaryAsync(s => s.AssignmentStatusId, s => s.AssignmentStatusName);
            var stages = new List<Stage>();
            foreach (var c in ccps.Where(c => c.AppliesTo == appliesTo || assigned.Any(a => a.Ccp == c.Id)))
            {
                var a = assigned.FirstOrDefault(x => x.Ccp == c.Id);
                var isAssigned = assigned.Any(x => x.Ccp == c.Id);
                var responsible = await OrgNamesAsync(await _resp.ResponsibleOrgsAsync(kind, id, c.Id));
                var readable = isAssigned && (await _resp.AccessAsync(kind, id, c.Id)).Read;
                var url = !readable ? null : kind == "raw"
                    ? $"/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId={id}&ccpId={c.Id}"
                    : $"/CCPMonitoring/FinalHoneyProductionManageCCP?finalHoneyProductionId={id}&ccpId={c.Id}";
                stages.Add(new Stage(c.Id, c.Code, c.Title, c.ProcessingStep, isAssigned, isAssigned ? a.Status : 0,
                    isAssigned ? statuses.GetValueOrDefault(a.Status) : null, a.ApprovedAt != null, a.ApprovedAt, a.Self,
                    responsible.Values.ToList(), readable, url));
            }
            return stages;
        }

        public async Task<IActionResult> Lot(int id)
        {
            var lot = await _context.RawHoneyProductions.AsNoTracking()
                .Include(r => r.RawHoneyProductionHoneyTypeNavigation)
                .Include(r => r.RawHoneyCCPAssignments)
                .FirstOrDefaultAsync(r => r.RawHoneyProductionId == id);
            if (lot == null || !await MayReadLotAsync(lot))
                return NotFound();
            var model = new Model { Kind = "lot", Id = id, Title = $"#{id} · {lot.RawHoneyProductionHoneyTypeNavigation?.HoneyTypeTitle}" };
            model.Stages.AddRange(await StagesAsync("raw", id, CcpAppliesTo.RawLot,
                lot.RawHoneyCCPAssignments.Select(a => (a.RawHoneyCCPAssignmentCCP, a.RawHoneyCCPAssignmentStatus, a.ApprovedAt, a.SelfApproved)).ToList()));
            var intake = await _context.LotIntakes.AsNoTracking().Include(i => i.ReceivingOrg).FirstOrDefaultAsync(i => i.RawHoneyProductionId == id);
            if (intake != null)
                model.Intake = $"{intake.DeliveryDate:dd.MM.yyyy} · {intake.ReceivingOrg?.DisplayName} · {intake.Quantity:0.##} kg";
            foreach (var s in await _context.Samples.AsNoTracking().Include(s => s.LabOrg).Include(s => s.Report)
                         .Where(s => s.RawHoneyProductionId == id).OrderBy(s => s.SampleId).ToListAsync())
                model.Samples.Add(new SampleRow(s.SampleId, s.SampleCode, s.LabOrg!.DisplayName, s.Status, s.SealIntact, s.Report?.Status,
                    s.Report?.Status == LabReportStatus.Approved ? s.Report.ReportCode : null));
            ViewBag.Controller = "Flow";
            ViewBag.Action = "Lot";
            return View("Flow", model);
        }

        public async Task<IActionResult> Batch(int id)
        {
            var batch = await _context.FinalHoneyProductions.AsNoTracking()
                .Include(f => f.FinalHoneyCCPAssignments)
                .Include(f => f.FinalHoneyProductionDetails).ThenInclude(d => d.RawHoneyProduction).ThenInclude(r => r.RawHoneyProductionHoneyTypeNavigation)
                .FirstOrDefaultAsync(f => f.FinalHoneyProductionId == id);
            if (batch == null || !await MayReadBatchAsync(batch))
                return NotFound();
            var model = new Model { Kind = "batch", Id = id, Title = batch.FinalHoneyProductionTitle ?? $"#{id}", Finalized = batch.FinalHoneyProductionIsFinalized };
            model.Stages.AddRange(await StagesAsync("batch", id, CcpAppliesTo.Batch,
                batch.FinalHoneyCCPAssignments.Select(a => (a.FinalHoneyCCPAssignmentCCP, a.FinalHoneyCCPAssignmentStatus, a.ApprovedAt, a.SelfApproved)).ToList()));
            foreach (var d in batch.FinalHoneyProductionDetails)
                model.Lots.Add((d.RawHoneyProductionId, d.RawHoneyProduction.RawHoneyProductionHoneyTypeNavigation?.HoneyTypeTitle, d.UsedAmount));
            ViewBag.Controller = "Flow";
            ViewBag.Action = "Batch";
            return View("Flow", model);
        }

        // Administrators; organizations that may read a CCP of the lot; the organizations of its samples
        private async Task<bool> MayReadLotAsync(RawHoneyProduction lot)
        {
            if (_current.IsPlatformAdmin)
                return true;
            var active = await _current.GetAsync();
            if (active == null)
                return false;
            var ccps = lot.RawHoneyCCPAssignments.Select(a => a.RawHoneyCCPAssignmentCCP).ToList();
            if ((await _resp.ReadableCcpIdsAsync("raw", lot.RawHoneyProductionId, ccps)).Count > 0 || await _resp.LotOwnerAsync(lot.RawHoneyProductionId) == active.OrgId)
                return true;
            return await _context.Samples.AnyAsync(s => s.RawHoneyProductionId == lot.RawHoneyProductionId && (s.LabOrgId == active.OrgId || s.TakingOrgId == active.OrgId));
        }

        // Administrators; the owner; holders; organizations that may read a CCP of the batch
        private async Task<bool> MayReadBatchAsync(FinalHoneyProduction batch)
        {
            if (_current.IsPlatformAdmin)
                return true;
            var active = await _current.GetAsync();
            if (active == null)
                return false;
            if (await _resp.BatchOwnerAsync(batch.FinalHoneyProductionId) == active.OrgId || await _custody.HasReceivedAsync(batch.FinalHoneyProductionId, active.OrgId))
                return true;
            var ccps = batch.FinalHoneyCCPAssignments.Select(a => a.FinalHoneyCCPAssignmentCCP).ToList();
            return (await _resp.ReadableCcpIdsAsync("batch", batch.FinalHoneyProductionId, ccps)).Count > 0;
        }
    }
}
