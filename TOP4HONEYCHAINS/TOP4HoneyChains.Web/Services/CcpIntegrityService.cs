using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services
{
    // Lock and finalize rules for batches, raw lots and their CCPs (D-14, D-16)
    public class CcpIntegrityService
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly OrgModel.FeatureFlags _flags;
        private readonly OrgModel.CcpResponsibilityService _resp;

        public CcpIntegrityService(TOP4HoneyChainsContext context, Microsoft.Extensions.Options.IOptions<OrgModel.FeatureFlags> flags,
            OrgModel.CcpResponsibilityService resp)
        {
            _context = context;
            _flags = flags.Value;
            _resp = resp;
        }

        // With the organization model a CCP counts as done only when it is also approved (D-27), unless the
        // responsibility matrix says the CCP needs no approval. v1.0: Completed or Not applicable.
        private async Task<List<int>> NoApprovalCcpIdsAsync() =>
            (await _resp.ConfigsAsync()).Values.Where(c => !c.RequiresApproval).Select(c => c.CcpId).ToList();

        public async Task<Func<int, DateTime?, int, bool>> DonePredicateAsync()
        {
            if (!_flags.OrgRoleModel)
                return (status, _, _) => AssignmentStatusIds.IsDone(status);
            var exempt = await NoApprovalCcpIdsAsync();
            return (status, approvedAt, ccpId) => AssignmentStatusIds.IsDone(status) && (approvedAt != null || exempt.Contains(ccpId));
        }

        private static string NotDoneText(string? statusName, int status) =>
            AssignmentStatusIds.IsDone(status) ? $"{statusName}, not approved yet" : statusName ?? "not completed";

        // A finalized batch is locked for good: batch data, composition, CCP assignments and logs (D-14)
        public async Task<string?> GetBatchLockReasonAsync(int batchId)
        {
            var finalized = await _context.FinalHoneyProductions
                .Where(f => f.FinalHoneyProductionId == batchId)
                .Select(f => (bool?)f.FinalHoneyProductionIsFinalized)
                .FirstOrDefaultAsync();
            return finalized == true
                ? "This batch is finalized and locked. Its data, composition, CCPs and CCP values can no longer be changed."
                : null;
        }

        // A raw lot used in a finalized batch is locked as well (D-14)
        public async Task<string?> GetRawLotLockReasonAsync(int rawHoneyProductionId)
        {
            var titles = await LockingBatchTitlesQuery(rawHoneyProductionId).ToListAsync();
            return titles.Count == 0
                ? null
                : $"This raw honey lot is used in the finalized batch {string.Join(", ", titles.Select(t => $"\"{t}\""))} and is locked. Its data, CCPs and CCP values can no longer be changed.";
        }

        public async Task<HashSet<int>> GetLockedRawLotIdsAsync(IEnumerable<int> rawHoneyProductionIds)
        {
            var ids = rawHoneyProductionIds.Distinct().ToList();
            var locked = await _context.FinalHoneyProductionDetails
                .Where(d => ids.Contains(d.RawHoneyProductionId) && d.FinalHoneyProduction.FinalHoneyProductionIsFinalized)
                .Select(d => d.RawHoneyProductionId)
                .Distinct()
                .ToListAsync();
            return locked.ToHashSet();
        }

        private IQueryable<string?> LockingBatchTitlesQuery(int rawHoneyProductionId) =>
            _context.FinalHoneyProductionDetails
                .Where(d => d.RawHoneyProductionId == rawHoneyProductionId && d.FinalHoneyProduction.FinalHoneyProductionIsFinalized)
                .Select(d => d.FinalHoneyProduction.FinalHoneyProductionTitle)
                .Distinct();

        // A raw lot can go into a batch when it has at least one CCP and all of them are Completed or
        // Not applicable (D-05, D-15)
        public async Task<IQueryable<RawHoneyProduction>> EligibleRawLotsAsync(IQueryable<RawHoneyProduction> lots)
        {
            var on = _flags.OrgRoleModel;
            var exempt = on ? await NoApprovalCcpIdsAsync() : new List<int>();
            return lots.Where(r => _context.RawHoneyCCPAssignments.Any(a => a.RawHoneyProductionId == r.RawHoneyProductionId))
                .Where(r => _context.RawHoneyCCPAssignments
                    .Where(a => a.RawHoneyProductionId == r.RawHoneyProductionId)
                    .All(a => (a.RawHoneyCCPAssignmentStatus == AssignmentStatusIds.Completed
                               || a.RawHoneyCCPAssignmentStatus == AssignmentStatusIds.NotApplicable)
                              && (!on || a.ApprovedAt != null || exempt.Contains(a.RawHoneyCCPAssignmentCCP))));
        }

        // Everything that prevents finalizing a batch; empty = it can be finalized (D-16)
        public async Task<List<string>> GetFinalizeBlockersAsync(int batchId)
        {
            var blockers = new List<string>();
            var done = await DonePredicateAsync();

            var details = await _context.FinalHoneyProductionDetails
                .Where(d => d.FinalHoneyProductionId == batchId)
                .Include(d => d.RawHoneyProduction).ThenInclude(r => r.RawHoneyProductionApiary)
                .Include(d => d.RawHoneyProduction).ThenInclude(r => r.RawHoneyProductionHoneyTypeNavigation)
                .ToListAsync();

            if (details.Sum(d => d.UsedAmount) <= 0)
                blockers.Add("The batch has no raw honey yet (total must be greater than zero).");

            // Every CCP that applies to batches must be assigned and done
            var batchCcps = await _context.CriticalControlPoints
                .Where(c => c.AppliesTo == CcpAppliesTo.Batch)
                .OrderBy(c => c.Id)
                .ToListAsync();
            var assignments = await _context.FinalHoneyCCPAssignments
                .Where(a => a.FinalHoneyProductionId == batchId)
                .Include(a => a.AssignmentStatus)
                .ToListAsync();

            foreach (var ccp in batchCcps)
            {
                var a = assignments.FirstOrDefault(x => x.FinalHoneyCCPAssignmentCCP == ccp.Id);
                if (a == null)
                    blockers.Add($"Batch: {ccp.Code} ({ccp.Title}) is not assigned.");
                else if (!done(a.FinalHoneyCCPAssignmentStatus, a.ApprovedAt, a.FinalHoneyCCPAssignmentCCP))
                    blockers.Add($"Batch: {ccp.Code} ({ccp.Title}) is {NotDoneText(a.AssignmentStatus?.AssignmentStatusName, a.FinalHoneyCCPAssignmentStatus)}.");
            }
            foreach (var a in assignments.Where(a => batchCcps.All(c => c.Id != a.FinalHoneyCCPAssignmentCCP)
                                                     && !done(a.FinalHoneyCCPAssignmentStatus, a.ApprovedAt, a.FinalHoneyCCPAssignmentCCP)))
            {
                var ccp = await _context.CriticalControlPoints.FindAsync(a.FinalHoneyCCPAssignmentCCP);
                blockers.Add($"Batch: {ccp?.Code} ({ccp?.Title}) is {NotDoneText(a.AssignmentStatus?.AssignmentStatusName, a.FinalHoneyCCPAssignmentStatus)}.");
            }

            // Lots: as for eligibility, checked again because a lot's CCPs can change after it was added
            var rawIds = details.Select(d => d.RawHoneyProductionId).ToList();
            var rawAssignments = await _context.RawHoneyCCPAssignments
                .Where(a => rawIds.Contains(a.RawHoneyProductionId))
                .Include(a => a.CCP)
                .Include(a => a.AssignmentStatus)
                .ToListAsync();
            foreach (var d in details)
            {
                var lot = $"Lot {d.RawHoneyProduction?.RawHoneyProductionApiary?.ApiaryTitle} / {d.RawHoneyProduction?.RawHoneyProductionHoneyTypeNavigation?.HoneyTypeTitle} (#{d.RawHoneyProductionId})";
                var own = rawAssignments.Where(a => a.RawHoneyProductionId == d.RawHoneyProductionId).ToList();
                if (own.Count == 0)
                    blockers.Add($"{lot}: no CCP assigned.");
                foreach (var a in own.Where(a => !done(a.RawHoneyCCPAssignmentStatus, a.ApprovedAt, a.RawHoneyCCPAssignmentCCP)).OrderBy(a => a.CCP?.Id))
                    blockers.Add($"{lot}: {a.CCP?.Code} ({a.CCP?.Title}) is {NotDoneText(a.AssignmentStatus?.AssignmentStatusName, a.RawHoneyCCPAssignmentStatus)}.");
            }

            return blockers;
        }

        public async Task<HoneyProfile> GetHoneyProfileForRawLotAsync(int rawHoneyProductionId) =>
            await _context.RawHoneyProductions
                .Where(r => r.RawHoneyProductionId == rawHoneyProductionId)
                .Select(r => new HoneyProfile(r.RawHoneyProductionHoneyType, r.RawHoneyProductionHoneyTypeNavigation!.HoneyClass))
                .FirstOrDefaultAsync() ?? HoneyProfile.Unknown;

        // A batch's honey type and class are those shared by all its lots; mixed or unknown -> null
        public async Task<HoneyProfile> GetHoneyProfileForBatchAsync(int batchId)
        {
            var lots = await _context.FinalHoneyProductionDetails
                .Where(d => d.FinalHoneyProductionId == batchId)
                .Select(d => new { d.RawHoneyProduction.RawHoneyProductionHoneyType, d.RawHoneyProduction.RawHoneyProductionHoneyTypeNavigation!.HoneyClass })
                .Distinct()
                .ToListAsync();
            var types = lots.Select(l => l.RawHoneyProductionHoneyType).Distinct().ToList();
            var classes = lots.Select(l => l.HoneyClass).Distinct().ToList();
            return new HoneyProfile(types.Count == 1 ? types[0] : null, classes.Count == 1 ? classes[0] : null);
        }
    }
}
