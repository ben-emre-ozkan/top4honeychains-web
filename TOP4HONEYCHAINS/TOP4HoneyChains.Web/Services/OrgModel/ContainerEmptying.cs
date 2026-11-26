using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-98 (migration 036): a container of a delivery stays sealed until it is emptied into a batch (D78-SPEC 3.2). Before emptying,
    // the closing seal is scanned or typed and confirmed intact:
    // - the container's current seal and intact: the container is emptied into the batch, its seal is opened (seal event Broken,
    //   pool code Broken) and an association container is free again;
    // - another code, or the seal reported broken: the step stops, the container stays sealed, the check is recorded (visible on the
    //   batch, the delivery and the association container) and written to the audit log.
    // Only containers closed since D-97 (Closure set) are listed; older deliveries have no closing seal from the pool.
    // Whoever may enter batch data (Batch/Enter) in the organization that owns the batch empties containers into it.
    public class ContainerEmptyingService
    {
        public record Row(Container Container, string? CurrentSeal, ContainerEmptying? Emptied, IReadOnlyList<ContainerEmptying> Checks)
        {
            public bool Sealed => Emptied == null && CurrentSeal != null;
            // the last check failed and the container was not emptied since
            public ContainerEmptying? Failed => Emptied == null ? Checks.LastOrDefault(c => c.Outcome != EmptyingOutcome.Emptied) : null;
        }

        public record Result(bool Emptied, bool Recorded, string Message);

        private readonly TOP4HoneyChainsContext _context;
        private readonly TimeProvider _clock;
        private readonly AuditWriter _audit;

        public ContainerEmptyingService(TOP4HoneyChainsContext context, TimeProvider clock, AuditWriter audit)
        {
            _context = context;
            _clock = clock;
            _audit = audit;
        }

        private DateTime Now => _clock.GetUtcNow().UtcDateTime;

        public static string BatchCode(int batchId) => "#" + batchId.ToString(CultureInfo.InvariantCulture);

        // why the active membership may not empty containers into a batch of ownerOrgId; null when it may
        public static string? Denied(ActiveMembership? active, int? ownerOrgId, Permissions perm)
        {
            if (active == null || ownerOrgId == null || active.OrgId != ownerOrgId || !active.IsVerified
                || !perm.Allows(PermissionAreas.Batch, PermissionAreas.Enter, active.OrgRole))
                return "Containers are emptied into a batch by those who enter batch data in the organization that owns the batch.";
            return null;
        }

        // the containers (closed since D-97) of the batch's lots, in delivery order, with their seal and every check at emptying
        public async Task<List<Row>> ForBatchAsync(int batchId)
        {
            var lots = await _context.FinalHoneyProductionDetails.AsNoTracking().Where(d => d.FinalHoneyProductionId == batchId)
                .Select(d => d.RawHoneyProductionId).Distinct().ToListAsync();
            var containers = await _context.Containers.AsNoTracking().Include(c => c.SealEvents).Include(c => c.OrgContainer).Include(c => c.Intake)
                .Where(c => lots.Contains(c.RawHoneyProductionId) && c.Closure != null)
                .OrderBy(c => c.RawHoneyProductionId).ThenBy(c => c.Seq).ToListAsync();
            var checks = await ChecksAsync(containers.Select(c => c.ContainerId));
            return containers.Select(c => RowOf(c, checks)).ToList();
        }

        public async Task<Dictionary<int, List<ContainerEmptying>>> ChecksAsync(IEnumerable<int> containerIds)
        {
            var ids = containerIds.Distinct().ToList();
            var rows = await _context.ContainerEmptyings.AsNoTracking().Where(e => ids.Contains(e.ContainerId)).OrderBy(e => e.EmptyingId).ToListAsync();
            return rows.GroupBy(e => e.ContainerId).ToDictionary(g => g.Key, g => g.ToList());
        }

        private static Row RowOf(Container c, IReadOnlyDictionary<int, List<ContainerEmptying>> checks)
        {
            var list = checks.TryGetValue(c.ContainerId, out var l) ? l : new List<ContainerEmptying>();
            return new Row(c, CustodyIntakeService.CurrentSeal(c.SealEvents), list.FirstOrDefault(e => e.Outcome == EmptyingOutcome.Emptied), list);
        }

        // the association containers still holding honey: id -> the code of the delivery container that is not emptied yet
        public async Task<Dictionary<int, string>> BoxesInUseAsync(IEnumerable<int> orgContainerIds)
        {
            var ids = orgContainerIds.Distinct().ToList();
            var open = await _context.Containers.AsNoTracking()
                .Where(c => c.OrgContainerId != null && ids.Contains(c.OrgContainerId.Value)
                            && !_context.ContainerEmptyings.Any(e => e.ContainerId == c.ContainerId && e.Outcome == EmptyingOutcome.Emptied))
                .OrderBy(c => c.ContainerId).Select(c => new { Id = c.OrgContainerId!.Value, c.ContainerCode }).ToListAsync();
            return open.GroupBy(o => o.Id).ToDictionary(g => g.Key, g => g.Last().ContainerCode);
        }

        // One seal check at emptying. broken: the person reports the seal broken. Refusals that record nothing come back with
        // Recorded = false (no seal scanned, intact not confirmed, wrong batch, already emptied).
        public async Task<Result> EmptyAsync(int batchId, int containerId, string? scanned, bool intact, bool broken, string userId)
        {
            var batch = await _context.FinalHoneyProductions.AsNoTracking().FirstOrDefaultAsync(f => f.FinalHoneyProductionId == batchId);
            if (batch == null)
                return new Result(false, false, "The batch was not found.");
            if (batch.FinalHoneyProductionIsFinalized)
                return new Result(false, false, "The batch is finalized; containers can no longer be emptied into it.");
            var inBatch = _context.FinalHoneyProductionDetails.Where(d => d.FinalHoneyProductionId == batchId).Select(d => d.RawHoneyProductionId);
            var container = await _context.Containers.Include(c => c.SealEvents)
                .FirstOrDefaultAsync(c => c.ContainerId == containerId && c.Closure != null && inBatch.Contains(c.RawHoneyProductionId));
            if (container == null)
                return new Result(false, false, "This container does not belong to a lot of this batch.");
            var code = container.ContainerCode;
            var done = await _context.ContainerEmptyings.AsNoTracking()
                .FirstOrDefaultAsync(e => e.ContainerId == containerId && e.Outcome == EmptyingOutcome.Emptied);
            if (done != null)
                return new Result(false, false, string.Format(CultureInfo.InvariantCulture, "Container {0} was already emptied into batch {1}.", code, BatchCode(done.FinalHoneyProductionId)));
            var current = CustodyIntakeService.CurrentSeal(container.SealEvents);
            if (current == null)
                return new Result(false, false, string.Format(CultureInfo.InvariantCulture, "Container {0} has no closing seal on record.", code));
            var seal = SealPoolService.Normalize(scanned);
            if (seal?.Length > 50)
                seal = seal[..50];
            if (seal == null && !broken)
                return new Result(false, false, string.Format(CultureInfo.InvariantCulture, "Scan or type the closing seal of container {0}.", code));

            var now = Now;
            var record = new ContainerEmptying
            {
                ContainerId = containerId, FinalHoneyProductionId = batchId, ExpectedSeal = current, ScannedSeal = seal, UserId = userId, At = now
            };
            if (broken || seal != current)
            {
                record.Outcome = broken ? EmptyingOutcome.FoundBroken : EmptyingOutcome.Mismatch;
                record.Note = broken ? "Seal found broken; the container was not emptied" : "Another seal code was scanned; the container was not emptied";
                _context.ContainerEmptyings.Add(record);
                await _context.SaveChangesAsync();
                await _audit.WriteAsync("SealCheckFailed", "Containers", containerId.ToString(CultureInfo.InvariantCulture), null,
                    new { record.Outcome, Batch = batchId, Expected = current, Scanned = seal }, record.Outcome);
                return new Result(false, true, broken
                    ? string.Format(CultureInfo.InvariantCulture, "The seal of container {0} was reported broken. The container was not emptied; the check was recorded.", code)
                    : string.Format(CultureInfo.InvariantCulture, "Seal {0} is not the closing seal of container {1}. The container was not emptied; the check was recorded.", seal, code));
            }
            if (!intact)
                return new Result(false, false, string.Format(CultureInfo.InvariantCulture, "Confirm that the seal of container {0} is intact, or report it broken.", code));

            await using var transaction = await _context.Database.BeginTransactionAsync();
            record.Outcome = EmptyingOutcome.Emptied;
            record.Note = "Emptied into batch " + BatchCode(batchId);
            _context.ContainerEmptyings.Add(record);
            _context.ContainerSealEvents.Add(new ContainerSealEvent
            {
                ContainerId = containerId, Event = SealEvent.Broken, SealNumber = current, Intact = true, UserId = userId, At = now, Note = record.Note
            });
            if (await _context.SealCodes.FirstOrDefaultAsync(s => s.Code == current) is { Status: SealStatus.Used } pooled)
                SealPoolService.Break(pooled, now);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return new Result(true, true, string.Format(CultureInfo.InvariantCulture, "Container {0} emptied into batch {1}; seal {2} is now opened.", code, BatchCode(batchId), current));
        }
    }
}
