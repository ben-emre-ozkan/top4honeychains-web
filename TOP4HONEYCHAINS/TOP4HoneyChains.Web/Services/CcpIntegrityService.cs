using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services
{
    // Lock and finalize rules for batches, raw lots and their CCPs (D-14, D-16)
    public class CcpIntegrityService
    {
        private readonly TOP4HoneyChainsContext _context;

        public CcpIntegrityService(TOP4HoneyChainsContext context)
        {
            _context = context;
        }

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
        public IQueryable<RawHoneyProduction> EligibleRawLots(IQueryable<RawHoneyProduction> lots) =>
            lots.Where(r => _context.RawHoneyCCPAssignments.Any(a => a.RawHoneyProductionId == r.RawHoneyProductionId))
                .Where(r => _context.RawHoneyCCPAssignments
                    .Where(a => a.RawHoneyProductionId == r.RawHoneyProductionId)
                    .All(a => a.RawHoneyCCPAssignmentStatus == AssignmentStatusIds.Completed
                              || a.RawHoneyCCPAssignmentStatus == AssignmentStatusIds.NotApplicable));

        // Everything that prevents finalizing a batch; empty = it can be finalized (D-16)
        public async Task<List<string>> GetFinalizeBlockersAsync(int batchId)
        {
            var blockers = new List<string>();

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
                else if (!AssignmentStatusIds.IsDone(a.FinalHoneyCCPAssignmentStatus))
                    blockers.Add($"Batch: {ccp.Code} ({ccp.Title}) is {a.AssignmentStatus?.AssignmentStatusName ?? "not completed"}.");
            }
            foreach (var a in assignments.Where(a => batchCcps.All(c => c.Id != a.FinalHoneyCCPAssignmentCCP)
                                                     && !AssignmentStatusIds.IsDone(a.FinalHoneyCCPAssignmentStatus)))
            {
                var ccp = await _context.CriticalControlPoints.FindAsync(a.FinalHoneyCCPAssignmentCCP);
                blockers.Add($"Batch: {ccp?.Code} ({ccp?.Title}) is {a.AssignmentStatus?.AssignmentStatusName ?? "not completed"}.");
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
                foreach (var a in own.Where(a => !AssignmentStatusIds.IsDone(a.RawHoneyCCPAssignmentStatus)).OrderBy(a => a.CCP?.Id))
                    blockers.Add($"{lot}: {a.CCP?.Code} ({a.CCP?.Title}) is {a.AssignmentStatus?.AssignmentStatusName ?? "not completed"}.");
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
