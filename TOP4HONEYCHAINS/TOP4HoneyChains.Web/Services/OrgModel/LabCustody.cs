using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-78 P2 (D78-SPEC 4.6, D-81, O-4 option c): how the laboratory analyses the container jars of a lot.
    // Composite (default): the lab mixes the received jars of the lot into one composite sample and reports on it; a container
    // jar is analysed on its own only after the composite exists (when it fails or on request) and does not replace the lot's
    // CCP 1 values. PerContainer: every jar is reported on its own and each approved report sets the lot's CCP 1 values.
    public class LabOptions
    {
        public string AnalysisMode { get; set; } = LabAnalysisMode.Composite;
    }

    public static class LabAnalysisMode
    {
        public const string Composite = "Composite";
        public const string PerContainer = "PerContainer";
    }

    public class LabCustodyService
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly LabOptions _options;

        private readonly SealPoolService? _pool;

        public LabCustodyService(TOP4HoneyChainsContext context, IOptionsSnapshot<LabOptions> options, SealPoolService? pool = null)
        {
            _context = context;
            _options = options.Value;
            _pool = pool;
        }

        public string Mode => string.Equals(_options.AnalysisMode, LabAnalysisMode.PerContainer, StringComparison.OrdinalIgnoreCase)
            ? LabAnalysisMode.PerContainer : LabAnalysisMode.Composite;

        // D-78 P3 (D-82): the approved results of every container of a lot (the latest approved jar report of each container)
        public record ContainerResult(int ContainerId, string ContainerCode, int ReportId, string ReportCode, IReadOnlyList<LabResult> Results)
        {
            public IReadOnlyDictionary<int, decimal> Numbers => Results.Where(r => r.MeasuredNumericValue != null)
                .GroupBy(r => r.ParameterId).ToDictionary(g => g.Key, g => g.First().MeasuredNumericValue!.Value);
        }

        public async Task<List<ContainerResult>> ContainerResultsAsync(int lotId)
        {
            var reports = await _context.LabReports.AsNoTracking().Include(r => r.Results).Include(r => r.Sample!).ThenInclude(s => s.Container)
                .Where(r => r.Status == LabReportStatus.Approved && r.Sample!.RawHoneyProductionId == lotId && r.Sample.Kind == SampleKind.Container)
                .ToListAsync();
            return reports.GroupBy(r => r.Sample!.ContainerId)
                .Select(g => g.OrderByDescending(r => r.ApprovedAt).ThenByDescending(r => r.ReportId).First())
                .OrderBy(r => r.Sample!.Container!.Seq)
                .Select(r => new ContainerResult(r.Sample!.ContainerId!.Value, r.Sample.Container!.ContainerCode, r.ReportId, r.ReportCode, r.Results.ToList()))
                .ToList();
        }

        // The lot's CCP 1 values in per-container mode: per parameter the worst value of the containers (WorstValue)
        public async Task<Dictionary<int, LabService.LabValue>> WorstValuesAsync(int lotId, IEnumerable<CriticalControlPointParameter> parameters, HoneyProfile honey)
        {
            var results = await ContainerResultsAsync(lotId);
            var values = new Dictionary<int, LabService.LabValue>();
            foreach (var p in parameters)
            {
                var candidates = results.Select(r => (r, value: r.Results.FirstOrDefault(x => x.ParameterId == p.ParameterId)))
                    .Where(x => x.value != null).ToList();
                if (candidates.Count == 0)
                    continue;
                var list = candidates.Select(x => new WorstValue.Candidate(x.r.ContainerCode, x.value!.MeasuredNumericValue, x.value.MeasuredTextValue, x.r.Numbers)).ToList();
                var note = $"Worst of {candidates.Count} container(s)";
                if (WorstValue.Pick(p, list, honey) is int i)
                    values[p.ParameterId] = new LabService.LabValue(list[i].Number, null, candidates[i].r.ReportId, $"{note}: {list[i].Tag}, lab report {candidates[i].r.ReportCode}");
                else if (WorstValue.CombineTexts(list) is string text)
                    values[p.ParameterId] = new LabService.LabValue(null, text, candidates[^1].r.ReportId, note);
            }
            return values;
        }

        // Reception of a container jar: seal number matches, label intact, cap intact (button still down) are all required answers;
        // any "no" rejects the jar and needs a reason.
        public static string? CheckReception(bool? sealMatches, bool? labelIntact, bool? capIntact, string? note)
        {
            if (sealMatches == null || labelIntact == null || capIntact == null)
                return "Answer all three reception checks: seal number, label and cap.";
            if ((sealMatches == false || labelIntact == false || capIntact == false) && string.IsNullOrWhiteSpace(note))
                return "A failed reception check rejects the jar: write the reason.";
            if (note?.Trim().Length > 500)
                return "The reason can be at most 500 characters.";
            return null;
        }

        // Whether an approved report on this sample becomes the lot's CCP 1 values
        public bool SetsLotValues(Sample sample) => sample.Kind != SampleKind.Batch && (sample.Kind != SampleKind.Container || Mode == LabAnalysisMode.PerContainer);

        // Extra rule for entering results on a container jar in composite mode
        public async Task<string?> ReportDeniedAsync(Sample sample)
        {
            if (sample.Kind != SampleKind.Container || Mode == LabAnalysisMode.PerContainer)
                return null;
            return await _context.Samples.AnyAsync(s => s.RawHoneyProductionId == sample.RawHoneyProductionId && s.Kind == SampleKind.Composite)
                ? null
                : "The laboratory analyses the composite of the lot's jars first; analyse a single jar after the composite was made.";
        }

        // The jars a composite of the lot would be made from: the received jar of every container (the latest one when a container was re-sampled)
        public async Task<(List<Sample> Jars, string? Error)> CompositeSourcesAsync(int lotId, int labOrgId)
        {
            if (Mode != LabAnalysisMode.Composite)
                return (new(), "The laboratory analyses every container's jar on its own (Lab:AnalysisMode = PerContainer).");
            if (await _context.Samples.AnyAsync(s => s.RawHoneyProductionId == lotId && s.Kind == SampleKind.Composite))
                return (new(), "A composite of this lot was already made.");
            var containers = await _context.Containers.AsNoTracking().Where(c => c.RawHoneyProductionId == lotId).Select(c => c.ContainerId).ToListAsync();
            if (containers.Count == 0)
                return (new(), "This lot has no containers.");
            var jars = await _context.Samples.Where(s => s.RawHoneyProductionId == lotId && s.Kind == SampleKind.Container).ToListAsync();
            var chosen = new List<Sample>();
            foreach (var c in containers)
            {
                var jar = jars.Where(s => s.ContainerId == c && s.Status == SampleStatus.Received && s.LabOrgId == labOrgId).OrderByDescending(s => s.SampleId).FirstOrDefault();
                if (jar == null)
                    return (new(), "Every container's jar must be received in good order before the composite is made.");
                chosen.Add(jar);
            }
            return (chosen, null);
        }

        public async Task<Sample> CreateCompositeAsync(int lotId, List<Sample> jars, int labOrgId, string userId)
        {
            var lab = await _context.Organizations.AsNoTracking().Where(o => o.OrgId == labOrgId).Select(o => o.DisplayName).FirstAsync();
            var now = DateTime.UtcNow;
            var composite = new Sample
            {
                SampleCode = $"S-{lotId}-COMP",
                RawHoneyProductionId = lotId,
                Kind = SampleKind.Composite,
                TakingOrgId = jars[0].TakingOrgId,
                LabOrgId = labOrgId,
                SealNumber = $"S-{lotId}-COMP",
                CounterSealNumber = "-",
                CounterLocation = $"Laboratory: {lab}",
                RetainUntil = jars.Max(j => j.RetainUntil),
                TakenByUserId = userId,
                TakenAt = now,
                Status = SampleStatus.Received,
                ReceivedByUserId = userId,
                ReceivedAt = now,
                SealIntact = true
            };
            composite.Events.Add(new SampleCustodyEvent
            {
                Event = SampleStatus.Received, FromOrgId = composite.TakingOrgId, ToOrgId = labOrgId, UserId = userId, At = now, SealIntact = true,
                Note = "Composite of " + string.Join(", ", jars.Select(j => j.SampleCode))
            });
            await using var transaction = await _context.Database.BeginTransactionAsync();
            _context.Samples.Add(composite);
            await _context.SaveChangesAsync();
            _context.SampleSources.AddRange(jars.Select(j => new SampleSource { CompositeSampleId = composite.SampleId, SourceSampleId = j.SampleId, CreatedAt = now }));
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return composite;
        }

        // A rejected jar ends its chain: the plant takes a new jar from the same container, which breaks the container's seal again
        // (a visible seal event) and seals it with a new plant seal afterwards.
        public async Task<(Sample? Jar, string? Error)> ResampleAsync(int containerId, string? jarSeal, string? counterSeal, bool capIntact, string? reseal, string userId)
        {
            var container = await _context.Containers.Include(c => c.SealEvents).Include(c => c.Samples).FirstOrDefaultAsync(c => c.ContainerId == containerId);
            if (container == null)
                return (null, "The container was not found.");
            var jars = container.Samples.Where(s => s.Kind == SampleKind.Container).OrderBy(s => s.SampleId).ToList();
            if (jars.Count == 0 || jars.Any(s => s.Status != SampleStatus.Rejected))
                return (null, "A new jar is taken only when the container's jar was rejected by the laboratory.");
            jarSeal = jarSeal?.Trim();
            counterSeal = counterSeal?.Trim();
            reseal = reseal?.Trim();
            if (string.IsNullOrEmpty(jarSeal) || string.IsNullOrEmpty(counterSeal) || string.IsNullOrEmpty(reseal))
                return (null, "Enter the seal numbers of the new jar, of its counter-sample and the new seal of the container.");
            if (jarSeal.Length > 50 || counterSeal.Length > 50 || reseal.Length > 50)
                return (null, "Seal numbers have at most 50 characters.");
            var lotId = container.RawHoneyProductionId;
            var last = jars[^1];
            // D-97: a container closed since D-97 takes the new jar labels and the new closing seal from the seal pool
            var capType = container.Closure != null
                ? await _context.LotIntakes.Where(i => i.IntakeId == container.IntakeId).Select(i => i.JarCapType).FirstOrDefaultAsync() : null;
            if (!capIntact)
                return (null, capType == JarCap.TamperRing
                    ? "The tamper ring of the jar's cap must be unbroken. A broken ring: discard the jar and fill a new one."
                    : capType == JarCap.SafetyButton
                        ? "The button of the jar's cap must be down. A raised button: discard the jar and fill a new one."
                        : "The jar's cap must be intact (button down) when sealed. A popped cap: discard the jar and fill a new one.");
            Dictionary<string, SealCode>? pool = null;
            if (container.Closure != null && _pool != null)
            {
                jarSeal = SealPoolService.Normalize(jarSeal)!;
                counterSeal = SealPoolService.Normalize(counterSeal)!;
                reseal = SealPoolService.Normalize(reseal)!;
                var pools = new[] { last.TakingOrgId };
                foreach (var (code, kinds) in new (string, string[]?)[] { (jarSeal, new[] { SealKind.Label }), (counterSeal, new[] { SealKind.Label }), (reseal, SealPoolService.ClosingKinds(container.Closure)) })
                    if (await _pool.RefusalAsync(code, pools, kinds) is string refusal)
                        return (null, refusal);
                if (new[] { jarSeal, counterSeal, reseal }.Distinct().Count() < 3)
                    return (null, "Every seal code of the delivery must be different.");
                pool = await _pool.TrackAsync(new[] { jarSeal, counterSeal, reseal });
            }
            var used = await _context.ContainerSealEvents.Where(e => e.Container!.RawHoneyProductionId == lotId).Select(e => e.SealNumber)
                .Concat(_context.Samples.Where(s => s.RawHoneyProductionId == lotId).Select(s => s.SealNumber))
                .Concat(_context.Samples.Where(s => s.RawHoneyProductionId == lotId).Select(s => s.CounterSealNumber)).ToListAsync();
            var fresh = new[] { jarSeal, counterSeal, reseal };
            if (fresh.Distinct(StringComparer.OrdinalIgnoreCase).Count() < 3 || fresh.Any(n => used.Contains(n, StringComparer.OrdinalIgnoreCase)))
                return (null, "Every seal number of the delivery must be different.");

            var now = DateTime.UtcNow;
            var current = CustodyIntakeService.CurrentSeal(container.SealEvents);
            _context.ContainerSealEvents.Add(new ContainerSealEvent { ContainerId = containerId, Event = SealEvent.Broken, SealNumber = current ?? "-", UserId = userId, At = now, Note = "Opened for a new sample" });
            _context.ContainerSealEvents.Add(new ContainerSealEvent { ContainerId = containerId, Event = SealEvent.Applied, SealNumber = reseal, Intact = true, UserId = userId, At = now, Note = "Sealed again after sampling" });
            var jar = new Sample
            {
                SampleCode = $"S-{lotId}-C{container.Seq}-{jars.Count + 1}",
                RawHoneyProductionId = lotId,
                ContainerId = containerId,
                Kind = SampleKind.Container,
                TakingOrgId = last.TakingOrgId,
                LabOrgId = last.LabOrgId,
                SealNumber = jarSeal,
                CounterSealNumber = counterSeal,
                CounterLocation = last.CounterLocation,
                RetainUntil = last.RetainUntil,
                JarCapIntactAtSealing = true,
                TakenByUserId = userId,
                TakenAt = now,
                Status = SampleStatus.Sent,
                CapType = capType
            };
            jar.Events.Add(new SampleCustodyEvent { Event = SampleStatus.Sent, FromOrgId = jar.TakingOrgId, ToOrgId = jar.LabOrgId, UserId = userId, At = now });
            _context.Samples.Add(jar);
            if (pool != null)
            {
                if (current != null && await _context.SealCodes.FirstOrDefaultAsync(s => s.Code == current) is { } opened)
                    SealPoolService.Break(opened, now);
                SealPoolService.Use(pool[jarSeal], SealUse.Jar, userId, now, sample: jar);
                SealPoolService.Use(pool[counterSeal], SealUse.CounterJar, userId, now, sample: jar);
                SealPoolService.Use(pool[reseal], SealUse.Closing, userId, now, container);
            }
            await _context.SaveChangesAsync();
            return (jar, null);
        }
    }
}
