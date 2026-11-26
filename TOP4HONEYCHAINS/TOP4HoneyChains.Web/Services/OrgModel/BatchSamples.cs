using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-98: the final honey result. On the company path a batch with honey from more than one apiary can get a laboratory
    // analysis of its final blend: the same sample and report flow as a lot (sealed jar with a seal label from the pool, a
    // counter-sample, a verified laboratory, receipt, results, four-eyes approval), the sample taken from the batch. The
    // approved report is shown as "final honey result" on the batch page and in the Honey Story, apart from the lot results,
    // and never becomes a lot's CCP 1 values. Finalizing without it shows a warning, not a block.
    public class BatchSampleService
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;
        private readonly CcpResponsibilityService _resp;
        private readonly Permissions _perm;
        private readonly SealPoolService _seals;

        public BatchSampleService(TOP4HoneyChainsContext context, ICurrentOrg current, CcpResponsibilityService resp, Permissions perm, SealPoolService seals)
        {
            _context = context;
            _current = current;
            _resp = resp;
            _perm = perm;
            _seals = seals;
        }

        public record State(bool Applies, Sample? Sample, LabReport? Approved);

        // the batch's apiaries (through its lots)
        public Task<int> ApiaryCountAsync(int batchId) =>
            _context.FinalHoneyProductionDetails.Where(d => d.FinalHoneyProductionId == batchId)
                .Select(d => d.RawHoneyProduction.RawHoneyProductionApiaryId).Distinct().CountAsync();

        // company path and more than one apiary
        public async Task<bool> AppliesAsync(int batchId) =>
            await _resp.BatchOwnerAsync(batchId) is int org && await _resp.PathOfAsync(org) == CcpPaths.Company
            && await ApiaryCountAsync(batchId) > 1;

        public async Task<State> StateAsync(int batchId)
        {
            var applies = await AppliesAsync(batchId);
            var samples = await _context.Samples.AsNoTracking().Include(s => s.LabOrg)
                .Include(s => s.Report!).ThenInclude(r => r.Results).ThenInclude(r => r.Parameter)
                .Where(s => s.FinalHoneyProductionId == batchId).OrderByDescending(s => s.SampleId).ToListAsync();
            var approved = samples.Select(s => s.Report).Where(r => r?.Status == LabReportStatus.Approved).OrderByDescending(r => r!.ApprovedAt).FirstOrDefault();
            return new State(applies, samples.FirstOrDefault(), approved);
        }

        // why the current user may not take the final blend's sample (null = allowed)
        public async Task<string?> DeniedAsync(int batchId)
        {
            var batch = await _context.FinalHoneyProductions.AsNoTracking().FirstOrDefaultAsync(f => f.FinalHoneyProductionId == batchId);
            if (batch == null)
                return "The batch does not exist.";
            if (_current.IsPlatformAdmin)
                return "Platform administrators are read-only on business data; request a correction instead.";
            var active = await _current.GetAsync();
            if (active == null || await _resp.BatchOwnerAsync(batchId) != active.OrgId)
                return "Only the organization that holds the batch takes its samples.";
            if (!active.IsVerified)
                return $"Your organization is {active.OrgStatus}; only verified organizations can enter data.";
            if (!_perm.Allows(PermissionAreas.Samples, PermissionAreas.Enter, active.OrgRole))
                return $"Your role ({active.OrgRole}) cannot take samples; roles: {string.Join(", ", _perm.Roles(PermissionAreas.Samples, PermissionAreas.Enter))}.";
            if (batch.FinalHoneyProductionIsFinalized)
                return "The batch is finalized.";
            if (!await AppliesAsync(batchId))
                return "A final honey result is taken on the company path for a batch with honey from more than one apiary.";
            if (await _context.Samples.AnyAsync(s => s.FinalHoneyProductionId == batchId && s.Status != SampleStatus.Rejected
                                                     && (s.Report == null || s.Report.Status != LabReportStatus.Approved)))
                return "A sample of this batch is already at the laboratory.";
            return null;
        }

        public async Task<(Sample? Sample, string? Error)> CreateAsync(int batchId, int labOrgId, string? seal, string? counterSeal,
            string? counterLocation, DateOnly? retainUntil, DateOnly today)
        {
            if (await DeniedAsync(batchId) is string denied)
                return (null, denied);
            var active = (await _current.GetAsync())!;
            seal = SealPoolService.Normalize(seal);
            counterSeal = SealPoolService.Normalize(counterSeal);
            counterLocation = counterLocation?.Trim();
            var lab = await _context.Organizations.AsNoTracking()
                .AnyAsync(o => o.OrgId == labOrgId && !o.IsPlatform && o.Status == OrgStatus.Verified && o.Capabilities.Any(c => c.Capability == Capabilities.Laboratory));
            var pools = new[] { active.OrgId };
            var labels = new[] { SealKind.Label };
            string? error = null;
            if (!lab)
                error = "Choose a verified laboratory.";
            else if (seal == null || counterSeal == null)
                error = "Enter the seal numbers of the sample and of the counter-sample (at most 50 characters).";
            else if (seal == counterSeal)
                error = "The sample and the counter-sample need different seals.";
            else if (await _seals.RefusalAsync(seal, pools, labels) is string r1)
                error = r1;
            else if (await _seals.RefusalAsync(counterSeal, pools, labels) is string r2)
                error = r2;
            else if (string.IsNullOrEmpty(counterLocation) || counterLocation.Length > 200)
                error = "Enter where the counter-sample is kept (at most 200 characters).";
            else if (retainUntil == null || retainUntil <= today)
                error = "Enter until when the counter-sample is kept (a future date).";
            if (error != null)
                return (null, error);

            var lotId = await _context.FinalHoneyProductionDetails.Where(d => d.FinalHoneyProductionId == batchId)
                .OrderBy(d => d.RawHoneyProductionId).Select(d => d.RawHoneyProductionId).FirstAsync();
            var count = await _context.Samples.CountAsync(s => s.FinalHoneyProductionId == batchId);
            var now = DateTime.UtcNow;
            await using var transaction = await _context.Database.BeginTransactionAsync();
            var sample = new Sample
            {
                SampleCode = $"S-B{batchId}-{count + 1}",
                RawHoneyProductionId = lotId,
                FinalHoneyProductionId = batchId,
                Kind = SampleKind.Batch,
                TakingOrgId = active.OrgId,
                LabOrgId = labOrgId,
                SealNumber = seal!,
                CounterSealNumber = counterSeal!,
                CounterLocation = counterLocation!,
                RetainUntil = retainUntil!.Value,
                TakenByUserId = _current.UserId!,
                TakenAt = now
            };
            sample.Events.Add(new SampleCustodyEvent { Event = SampleStatus.Sent, FromOrgId = active.OrgId, ToOrgId = labOrgId, UserId = _current.UserId!, At = now });
            _context.Samples.Add(sample);
            var codes = await _seals.TrackAsync(new[] { seal!, counterSeal! });
            SealPoolService.Use(codes[seal!], SealUse.Jar, _current.UserId!, DateTime.Now, sample: sample);
            SealPoolService.Use(codes[counterSeal!], SealUse.CounterJar, _current.UserId!, DateTime.Now, sample: sample);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return (sample, null);
        }
    }
}
