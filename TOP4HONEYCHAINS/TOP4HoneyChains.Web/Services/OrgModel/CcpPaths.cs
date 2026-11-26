using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-98: two paths after the laboratory. On the association path the Quality Assurance Officer marks the lot "ready for
    // packing" once its lab report is approved, and the honey goes straight to packing: CCP 2 to 6 do not apply to the lot or
    // its batch (recorded as "Not applicable" with the reason, audited, not shown to members). On the company path (processing
    // companies) CCP 2 and the later steps apply as before. The path is a setting of the organization; null = Association when
    // it has the Association capability, Company otherwise.
    public static class CcpPaths
    {
        public const string Association = "Association";
        public const string Company = "Company";
        public static readonly string[] All = { Association, Company };

        public static readonly string[] SkippedLotCcps = { "CCP 2", "CCP 3" };
        public static readonly string[] SkippedBatchCcps = { "CCP 4", "CCP 5", "CCP 6" };

        // the reason stored on the records (English; translated when shown)
        public const string Reason = "Association path: the honey goes to packing after the approved laboratory report.";

        public static string Effective(string? setting, bool hasAssociation) =>
            setting is Association or Company ? setting : hasAssociation ? Association : Company;
    }

    public class CcpPathService
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;
        private readonly AuditWriter _audit;
        private readonly CcpResponsibilityService _resp;

        public CcpPathService(TOP4HoneyChainsContext context, ICurrentOrg current, AuditWriter audit, CcpResponsibilityService resp)
        {
            _context = context;
            _current = current;
            _audit = audit;
            _resp = resp;
        }

        public Task<string> OfOrgAsync(int orgId) => _resp.PathOfAsync(orgId);

        public async Task<string?> OfLotAsync(int lotId) =>
            await _resp.LotOwnerAsync(lotId) is int org ? await OfOrgAsync(org) : null;

        public async Task<string?> OfBatchAsync(int batchId) =>
            await _resp.BatchOwnerAsync(batchId) is int org ? await OfOrgAsync(org) : null;

        // an approved lab report of the lot itself (not of a batch's final blend)
        public Task<bool> LabApprovedAsync(int lotId) =>
            _context.LabReports.AnyAsync(r => r.Status == LabReportStatus.Approved && r.Sample!.RawHoneyProductionId == lotId
                                             && r.Sample.FinalHoneyProductionId == null);

        // why the current user may not mark the lot "ready for packing" (null = allowed)
        public async Task<string?> PackingReadyDeniedAsync(int lotId)
        {
            var lot = await _context.RawHoneyProductions.AsNoTracking().FirstOrDefaultAsync(r => r.RawHoneyProductionId == lotId);
            if (lot == null)
                return "The lot does not exist.";
            if (lot.PackingReadyAt != null)
                return "The honey of this lot is already marked ready for packing.";
            var owner = await _resp.LotOwnerAsync(lotId);
            if (owner == null || await OfOrgAsync(owner.Value) != CcpPaths.Association)
                return "Only organizations on the association path mark honey ready for packing.";
            if (_current.IsPlatformAdmin)
                return "Platform administrators are read-only on business data; request a correction instead.";
            var active = await _current.GetAsync();
            if (active == null || active.OrgId != owner)
                return "Only the organization that holds the lot marks it ready for packing.";
            if (!active.IsVerified)
                return $"Your organization is {active.OrgStatus}; only verified organizations can enter data.";
            if (active.OrgRole != OrgRoles.QualityApprover)
                return "Only the Quality Assurance Officer marks the honey ready for packing.";
            if (await _resp.LotLockedAsync(lotId) is string locked)
                return locked;
            if (!await LabApprovedAsync(lotId))
                return "The lot's laboratory report is not approved yet.";
            var ids = await SkippedIdsAsync(CcpPaths.SkippedLotCcps);
            if (await _context.CriticalControlPointMonitoringLogs.AnyAsync(l => l.RawHoneyProductionId == lotId && l.CCPId != null && ids.Contains(l.CCPId.Value)))
                return "CCP 2 or CCP 3 of this lot already has values, so the lot follows the company path.";
            return null;
        }

        private Task<List<int>> SkippedIdsAsync(string[] codes) =>
            _context.CriticalControlPoints.Where(c => codes.Contains(c.Code)).Select(c => c.Id).ToListAsync();

        private Task<string> UserNameAsync(string? userId) => Task.FromResult(_current.UserName ?? userId ?? "system");

        // "Bal paketlemeye hazır": CCP 2 and 3 of the lot become "Not applicable" with the reason, approved by the officer who
        // decided it; one audit entry per record and one for the decision. The caller checked PackingReadyDeniedAsync.
        public async Task MarkPackingReadyAsync(int lotId)
        {
            var active = (await _current.GetAsync())!;
            var user = _current.UserId!;
            var userName = await UserNameAsync(user);
            var now = DateTime.Now;
            await using var transaction = await _context.Database.BeginTransactionAsync();
            var lot = await _context.RawHoneyProductions.FirstAsync(r => r.RawHoneyProductionId == lotId);
            lot.PackingReadyAt = now;
            lot.PackingReadyByUserId = user;
            var ids = await SkippedIdsAsync(CcpPaths.SkippedLotCcps);
            foreach (var a in await _context.RawHoneyCCPAssignments.Where(a => a.RawHoneyProductionId == lotId && ids.Contains(a.RawHoneyCCPAssignmentCCP)).ToListAsync())
            {
                a.RawHoneyCCPAssignmentStatus = AssignmentStatusIds.NotApplicable;
                a.NotApplicableReason = CcpPaths.Reason;
                a.NotApplicableBy = userName;
                a.NotApplicableAt = now;
                a.RawHoneyCCPAssignmentEndDate = now;
                a.ApprovedByUserId = user;
                a.ApprovedByOrgId = active.OrgId;
                a.ApprovedAt = now;
                a.SelfApproved = false;
                await _audit.WriteAsync("NotApplicable", "RawHoneyCCPAssignments", $"{lotId}/{a.RawHoneyCCPAssignmentCCP}", null,
                    new { Status = a.RawHoneyCCPAssignmentStatus, Reason = CcpPaths.Reason }, "AssociationPath");
            }
            await _context.SaveChangesAsync();
            await _audit.WriteAsync("PackingReady", "RawHoneyProductions", lotId.ToString(), null, new { PackingReadyAt = now }, "AssociationPath");
            await transaction.CommitAsync();
        }

        // A new batch of an organization on the association path: CCP 4 to 6 do not apply (inside the caller's transaction)
        public async Task ApplyToNewBatchAsync(int batchId, int orgId)
        {
            if (await OfOrgAsync(orgId) != CcpPaths.Association)
                return;
            var ids = await SkippedIdsAsync(CcpPaths.SkippedBatchCcps);
            var user = _current.UserId;
            var userName = await UserNameAsync(user);
            var now = DateTime.Now;
            foreach (var a in await _context.FinalHoneyCCPAssignments.Where(a => a.FinalHoneyProductionId == batchId && ids.Contains(a.FinalHoneyCCPAssignmentCCP)).ToListAsync())
            {
                a.FinalHoneyCCPAssignmentStatus = AssignmentStatusIds.NotApplicable;
                a.NotApplicableReason = CcpPaths.Reason;
                a.NotApplicableBy = userName;
                a.NotApplicableAt = now;
                a.FinalHoneyCCPAssignmentEndDate = now;
                a.ApprovedByUserId = user;
                a.ApprovedByOrgId = orgId;
                a.ApprovedAt = now;
                await _audit.WriteAsync("NotApplicable", "FinalHoneyCCPAssignments", $"{batchId}/{a.FinalHoneyCCPAssignmentCCP}", null,
                    new { Status = a.FinalHoneyCCPAssignmentStatus, Reason = CcpPaths.Reason }, "AssociationPath");
            }
            await _context.SaveChangesAsync();
        }
    }
}
