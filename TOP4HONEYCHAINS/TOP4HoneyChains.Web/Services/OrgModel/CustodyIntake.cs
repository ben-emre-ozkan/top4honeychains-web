using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.Messaging;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-97 (D-78 P2): one container of a delivery as the sample acceptance officer records it, in three steps:
    //   1. the container: kind, weights, how it is closed (ClosureKind) and how it arrived (unsealed, or sealed at harvest by the
    //      association's field inspector with a seal from the pool);
    //   2. the samples: sample jar and counter-sample jar, each with a seal label from the pool over cap and jar, and the cap check;
    //   3. the closing: one seal from the pool, applied after sampling in front of the beekeeper.
    public class ContainerRow
    {
        public string? Closure { get; set; } = ClosureKind.SealLabel;
        public string? OrgContainerCode { get; set; }
        public string? Kind { get; set; } = ContainerKind.Tin;
        public decimal? NetKg { get; set; }
        public decimal? GrossKg { get; set; }
        public bool ArrivedSealed { get; set; }
        public string? OriginSeal { get; set; }
        public bool OriginSealIntact { get; set; }
        public string? JarSeal { get; set; }
        public string? CounterSeal { get; set; }
        public bool CapIntact { get; set; }
        public string? ClosingSeal { get; set; }
    }

    public class IntakeForm
    {
        public string? DraftKey { get; set; }
        public string? Number { get; set; }
        public int ApiaryId { get; set; }
        public int HoneyTypeId { get; set; }
        // D-98: HoneyTypeId = HoneyTypeCatalog.Other: a new honey type with its class, created with the delivery
        public string? NewHoneyTypeName { get; set; }
        public string? NewHoneyTypeClass { get; set; }
        public int PeriodId { get; set; }
        public DateOnly? DeliveryDate { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? OriginCountry { get; set; }
        public int LabOrgId { get; set; }
        // D-97: the cap of the sample jars, once per delivery (JarCap)
        public string? CapType { get; set; } = JarCap.SafetyButton;
        public string? CounterLocation { get; set; }
        public DateOnly? RetainUntil { get; set; }
        public List<ContainerRow> Containers { get; set; } = new();
        public string? Code { get; set; }
        public string? FallbackReason { get; set; }
    }

    // What the beekeeper confirms, in a form that can be rebuilt from the stored delivery (recall check, D78-SPEC 3.10).
    // D-97: Closure and OrgContainer (the permanent container's code) are set for deliveries recorded since D-97; Reseal is then the
    // one closing seal and ArrivalPlantSeal stays null.
    public record ContainerContent(string Kind, decimal Net, decimal? Gross, string? OriginSeal, bool? OriginIntact, string? ArrivalPlantSeal,
        string JarSeal, string CounterSeal, bool CapIntact, string Reseal, string? Closure = null, string? OrgContainer = null);

    // D-97: CapType (the jar cap) is set for deliveries recorded since D-97; their canonical text is "intake2|..." and binds every
    // container code, seal code and the cap type. Deliveries recorded before keep their "intake|..." text, so they stay verified.
    public record IntakeContent(string BeekeeperId, int ApiaryId, int HoneyTypeId, int PeriodId, string Origin, DateOnly Date, string Province,
        string District, int LabOrgId, string CounterLocation, DateOnly RetainUntil, IReadOnlyList<ContainerContent> Containers, string? CapType = null)
    {
        public decimal TotalKg => Containers.Sum(c => c.Net);

        public string Canonical()
        {
            static string D(decimal? v) => v?.ToString("0.##", CultureInfo.InvariantCulture) ?? "";
            static string B(bool? v) => v == null ? "" : v.Value ? "1" : "0";
            if (CapType != null)
            {
                var v2 = new StringBuilder("intake2|");
                v2.Append(string.Join("|", BeekeeperId, ApiaryId, HoneyTypeId, PeriodId, Origin, Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Province, District, LabOrgId, CounterLocation, RetainUntil.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), CapType));
                for (var i = 0; i < Containers.Count; i++)
                {
                    var c = Containers[i];
                    v2.Append($"|C{i + 1}:{c.Closure};{c.OrgContainer};{c.Kind};{D(c.Net)};{D(c.Gross)};{c.OriginSeal};{B(c.OriginIntact)};{c.JarSeal};{c.CounterSeal};{B(c.CapIntact)};{c.Reseal}");
                }
                return v2.ToString();
            }
            var sb = new StringBuilder("intake|");
            sb.Append(string.Join("|", BeekeeperId, ApiaryId, HoneyTypeId, PeriodId, Origin, Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Province, District, LabOrgId, CounterLocation, RetainUntil.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
            for (var i = 0; i < Containers.Count; i++)
            {
                var c = Containers[i];
                sb.Append($"|C{i + 1}:{c.Kind};{D(c.Net)};{D(c.Gross)};{c.OriginSeal};{B(c.OriginIntact)};{c.ArrivalPlantSeal};{c.JarSeal};{c.CounterSeal};{B(c.CapIntact)};{c.Reseal}");
            }
            return sb.ToString();
        }

        public string Hash() => CodeRules.ContentHash(Canonical());
    }

    public class CustodyIntakeService
    {
        public const int MaxContainers = 100;
        public record Saved(int LotId, int IntakeId);

        private readonly TOP4HoneyChainsContext _context;
        private readonly TimeProvider _clock;
        private readonly SealPoolService _pool;
        private readonly HoneyTypeCatalog _honeyTypes;

        public CustodyIntakeService(TOP4HoneyChainsContext context, TimeProvider clock, SealPoolService pool, HoneyTypeCatalog honeyTypes)
        {
            _context = context;
            _clock = clock;
            _pool = pool;
            _honeyTypes = honeyTypes;
        }

        // D-98: what the SMS code binds. With a new honey type ("Other") the content carries no id yet, so the typed name and the
        // class are bound too; the stored hash then uses the new id (SaveAsync).
        public static string ConfirmHash(IntakeForm f, IntakeContent content) =>
            f.HoneyTypeId == HoneyTypeCatalog.Other
                ? CodeRules.ContentHash(content.Canonical() + "|newhoney:" + HoneyTypeCatalog.Normalize(f.NewHoneyTypeName) + ";" + (f.NewHoneyTypeName ?? "").Trim() + ";" + f.NewHoneyTypeClass)
                : content.Hash();

        private DateTime Now => _clock.GetUtcNow().UtcDateTime;

        private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        // The container part of the checks (the delivery fields are checked by the controller as for the legacy intake). D-97: every
        // seal comes from the pool and is unused; the permanent container belongs to the organization or the beekeeper's association.
        public async Task<List<string>> CheckContainersAsync(IntakeForm f, int orgId, string? beekeeperId)
        {
            var errors = new List<string>();
            if (!JarCap.All.Contains(f.CapType ?? ""))
                errors.Add("Choose the cap of the sample jars.");
            if (f.Containers.Count == 0)
                errors.Add("Record at least one container.");
            if (f.Containers.Count > MaxContainers)
                errors.Add("At most 100 containers per delivery.");
            var pools = new[] { orgId };
            var originPools = await _pool.OriginPoolsAsync(orgId, beekeeperId);
            var seals = new List<string>();
            var boxes = new List<string>();
            for (var i = 0; i < f.Containers.Count; i++)
            {
                var c = f.Containers[i];
                var n = i + 1;
                c.OriginSeal = SealPoolService.Normalize(c.OriginSeal);
                c.JarSeal = SealPoolService.Normalize(c.JarSeal);
                c.CounterSeal = SealPoolService.Normalize(c.CounterSeal);
                c.ClosingSeal = SealPoolService.Normalize(c.ClosingSeal);
                c.OrgContainerCode = SealPoolService.Normalize(c.OrgContainerCode);
                if (!c.ArrivedSealed)
                {
                    c.OriginSeal = null;
                    c.OriginSealIntact = false;
                }
                if (!ClosureKind.All.Contains(c.Closure ?? ""))
                {
                    errors.Add($"Container {n}: choose how the container is closed.");
                    continue;
                }
                if (c.Closure == ClosureKind.OrgContainer)
                {
                    var (box, boxError) = await _pool.FindContainerAsync(c.OrgContainerCode, originPools);
                    if (boxError != null)
                        errors.Add($"Container {n}: {boxError}");
                    else
                    {
                        c.Kind = box!.Kind;
                        c.OrgContainerCode = box.Code;
                        boxes.Add(box.Code);
                    }
                }
                else
                    c.OrgContainerCode = null;
                if (!ContainerKind.All.Contains(c.Kind ?? ""))
                    errors.Add($"Container {n}: choose the kind of container.");
                if (c.NetKg is not (> 0 and <= 100000))
                    errors.Add($"Container {n}: enter the net weight (more than 0 kg).");
                if (c.GrossKg != null && c.GrossKg < c.NetKg)
                    errors.Add($"Container {n}: the gross weight cannot be lower than the net weight.");
                if (c.ArrivedSealed && c.OriginSeal == null)
                    errors.Add($"Container {n}: enter the code of the seal the field inspector put on at harvest.");
                if (c.JarSeal == null || c.CounterSeal == null)
                    errors.Add($"Container {n}: enter the seal label codes of the sample jar and of the counter-sample jar.");
                if (!c.CapIntact)
                    errors.Add(f.CapType == JarCap.TamperRing
                        ? $"Container {n}: the tamper ring of the jar's cap must be unbroken. A broken ring: discard the jar and fill a new one."
                        : $"Container {n}: the button of the jar's cap must be down. A raised button: discard the jar and fill a new one.");
                if (c.ClosingSeal == null)
                    errors.Add($"Container {n}: enter the closing seal, applied after sampling in front of the beekeeper.");
                foreach (var (code, usePools, kinds) in new (string?, IReadOnlyCollection<int>, string[]?)[]
                         {
                             (c.OriginSeal, originPools, null), (c.JarSeal, pools, new[] { SealKind.Label }), (c.CounterSeal, pools, new[] { SealKind.Label }),
                             (c.ClosingSeal, pools, SealPoolService.ClosingKinds(c.Closure))
                         })
                {
                    if (code == null)
                        continue;
                    if (code.Length > 50)
                        errors.Add($"Container {n}: seal codes have at most 50 characters.");
                    else if (await _pool.RefusalAsync(code, usePools, kinds) is string refusal)
                        errors.Add($"Container {n}: {refusal}");
                    seals.Add(code);
                }
            }
            if (seals.GroupBy(s => s, StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1))
                errors.Add("Every seal code of the delivery must be different.");
            if (boxes.GroupBy(b => b).Any(g => g.Count() > 1))
                errors.Add("Each association container can be recorded only once per delivery.");
            return errors;
        }

        public static IntakeContent ContentOf(IntakeForm f, string beekeeperId) =>
            new(beekeeperId, f.ApiaryId, f.HoneyTypeId, f.PeriodId, f.OriginCountry ?? Origin.Default, f.DeliveryDate!.Value, f.Province!.Trim(), f.District!.Trim(),
                f.LabOrgId, f.CounterLocation!.Trim(), f.RetainUntil!.Value,
                f.Containers.Select(c => new ContainerContent(c.Kind!, c.NetKg!.Value, c.GrossKg,
                    c.ArrivedSealed ? c.OriginSeal : null, c.ArrivedSealed ? c.OriginSealIntact : null, null,
                    c.JarSeal!, c.CounterSeal!, c.CapIntact, c.ClosingSeal!, c.Closure, c.OrgContainerCode)).ToList(), f.CapType);

        // The content of a stored delivery with containers, rebuilt from the lot, the containers, their seal events and jars
        public async Task<IntakeContent?> StoredContentAsync(int intakeId)
        {
            var intake = await _context.LotIntakes.AsNoTracking().FirstOrDefaultAsync(i => i.IntakeId == intakeId);
            if (intake == null || intake.IsLegacy)
                return null;
            var lot = await _context.RawHoneyProductions.AsNoTracking().Include(r => r.RawHoneyProductionApiary)
                .FirstAsync(r => r.RawHoneyProductionId == intake.RawHoneyProductionId);
            var containers = await _context.Containers.AsNoTracking().Include(c => c.SealEvents).Include(c => c.Samples).Include(c => c.OrgContainer)
                .Where(c => c.IntakeId == intakeId).OrderBy(c => c.Seq).ToListAsync();
            var rows = new List<ContainerContent>();
            Sample? anyJar = null;
            foreach (var c in containers)
            {
                var events = c.SealEvents.OrderBy(e => e.EventId).ToList();
                var arrival = events.FirstOrDefault();
                var firstBroken = events.FindIndex(e => e.Event == SealEvent.Broken);
                var reseal = firstBroken < 0 ? null : events.Skip(firstBroken + 1).FirstOrDefault(e => e.Event == SealEvent.Applied);
                var jar = c.Samples.Where(s => s.Kind == SampleKind.Container).OrderBy(s => s.SampleId).FirstOrDefault();
                anyJar ??= jar;
                if (c.Closure != null)
                {
                    // D-97: one closing seal, the first one applied at intake (later re-sampling adds events after it)
                    var closing = events.FirstOrDefault(e => e.Event == SealEvent.Applied);
                    rows.Add(new ContainerContent(c.Kind, c.NetWeightKg, c.GrossWeightKg, c.OriginSealNumber, c.OriginSealIntact, null,
                        jar?.SealNumber ?? "", jar?.CounterSealNumber ?? "", jar?.JarCapIntactAtSealing == true, closing?.SealNumber ?? "", c.Closure, c.OrgContainer?.Code));
                    continue;
                }
                rows.Add(new ContainerContent(c.Kind, c.NetWeightKg, c.GrossWeightKg, c.OriginSealNumber, c.OriginSealIntact,
                    arrival?.Event == SealEvent.Applied ? arrival.SealNumber : null,
                    jar?.SealNumber ?? "", jar?.CounterSealNumber ?? "", jar?.JarCapIntactAtSealing == true, reseal?.SealNumber ?? ""));
            }
            return new IntakeContent(lot.RawHoneyProductionApiary?.ApiaryBeekeeperId ?? "", lot.RawHoneyProductionApiaryId ?? 0, lot.RawHoneyProductionHoneyType ?? 0,
                lot.RawHoneyProductionPeriodId ?? 0, lot.OriginCountry ?? Origin.Default, intake.DeliveryDate, intake.Province, intake.District,
                anyJar?.LabOrgId ?? 0, anyJar?.CounterLocation ?? "", anyJar?.RetainUntil ?? default, rows, intake.JarCapType);
        }

        // "Verified" when the stored delivery still has the hash the beekeeper confirmed; null for a legacy delivery
        public async Task<bool?> ContentMatchesAsync(int intakeId)
        {
            var intake = await _context.LotIntakes.AsNoTracking().FirstOrDefaultAsync(i => i.IntakeId == intakeId);
            if (intake == null || intake.IsLegacy || intake.ContentSha256 == null)
                return null;
            var content = await StoredContentAsync(intakeId);
            if (content == null)
                return false;
            if (string.Equals(content.Hash(), intake.ContentSha256.Trim(), StringComparison.OrdinalIgnoreCase))
                return true;
            // D-98: the beekeeper confirmed a honey type that the platform administrator later merged into the lot's current one
            var merged = await _context.HoneyTypeMerges.AsNoTracking().Where(m => m.TargetHoneyTypeId == content.HoneyTypeId).Select(m => m.SourceHoneyTypeId).ToListAsync();
            return merged.Any(id => string.Equals((content with { HoneyTypeId = id }).Hash(), intake.ContentSha256.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        // Lot, intake, containers, seal events (arrival, broken for sampling, new plant seal) and one jar per container, in one
        // transaction. SMS: confirmed at once. Photo: waits for the Compliance Officer (the lot cannot be finalized before).
        public async Task<Saved> SaveAsync(IntakeForm f, IntakeContent content, int orgId, int? legacyWholesalerId, string userId,
            long? codeId, (string Stored, string Original, string Sha256)? photo)
        {
            var now = Now;
            await using var transaction = await _context.Database.BeginTransactionAsync();
            // D-98: the new honey type ("Other") is created with the delivery; the stored hash binds its id
            // (the form keeps "Other", so a refused save shows the typed name again)
            var honeyTypeId = f.HoneyTypeId;
            if (honeyTypeId == HoneyTypeCatalog.Other)
            {
                honeyTypeId = await _honeyTypes.CreateAsync(new HoneyTypeCatalog.NewType(f.NewHoneyTypeName!, f.NewHoneyTypeClass!));
                content = content with { HoneyTypeId = honeyTypeId };
            }
            var lot = new RawHoneyProduction
            {
                RawHoneyProductionApiaryId = f.ApiaryId,
                RawHoneyProductionPeriodId = f.PeriodId,
                RawHoneyProductionHoneyType = honeyTypeId,
                RawHoneyProductionAmount = content.TotalKg,
                RawHoneyProductionDate = f.DeliveryDate,
                RawHoneyProductionWholesalerId = legacyWholesalerId,
                OwnerOrgId = orgId,
                OriginCountry = content.Origin
            };
            _context.RawHoneyProductions.Add(lot);
            await _context.SaveChangesAsync();

            foreach (var ccpId in await _context.CriticalControlPoints.Where(c => c.AppliesTo == CcpAppliesTo.RawLot).Select(c => c.Id).ToListAsync())
                _context.RawHoneyCCPAssignments.Add(new RawHoneyCCPAssignment
                {
                    RawHoneyProductionId = lot.RawHoneyProductionId,
                    RawHoneyCCPAssignmentCCP = ccpId,
                    RawHoneyCCPAssignmentStatus = AssignmentStatusIds.Assigned,
                    RawHoneyCCPAssignmentStartDate = DateTime.Now
                });

            var sms = codeId != null;
            var intake = new LotIntake
            {
                RawHoneyProductionId = lot.RawHoneyProductionId, ReceivingOrgId = orgId, ReceivedByUserId = userId, ReceivedAt = now,
                DeliveryDate = content.Date, Quantity = content.TotalKg, Province = content.Province, District = content.District,
                FormStoredFileName = photo?.Stored, FormOriginalFileName = photo?.Original, FormSha256 = photo?.Sha256,
                ConfirmationMethod = sms ? ConfirmationMethod.SmsCode : ConfirmationMethod.SignedPhoto,
                ContentSha256 = content.Hash(), ConfirmedAt = sms ? now : null, CodeId = codeId,
                FallbackReason = sms ? null : Clean(f.FallbackReason) is string r ? (r.Length > 300 ? r[..300] : r) : null,
                EvidenceStatus = sms ? EvidenceStatus.Accepted : EvidenceStatus.PendingReview,
                ContainersRecorded = true,
                JarCapType = content.CapType
            };
            _context.LotIntakes.Add(intake);
            await _context.SaveChangesAsync();

            // D-97: the pool codes of the delivery, checked again inside the transaction (another delivery may have used one meanwhile)
            var codes = content.Containers.SelectMany(c => new[] { c.OriginSeal, c.JarSeal, c.CounterSeal, c.Reseal }).OfType<string>().ToList();
            var pool = await _pool.TrackAsync(codes);
            if (codes.Any(code => !pool.TryGetValue(code, out var sc) || sc.Status != SealStatus.Issued))
                throw new InvalidOperationException("A seal code of the delivery was used meanwhile; record the delivery again.");
            var boxCodes = content.Containers.Select(c => c.OrgContainer).OfType<string>().ToList();
            var boxes = await _context.OrgContainers.Where(b => boxCodes.Contains(b.Code)).ToDictionaryAsync(b => b.Code);

            for (var i = 0; i < content.Containers.Count; i++)
            {
                var c = content.Containers[i];
                var seq = i + 1;
                var container = new Container
                {
                    IntakeId = intake.IntakeId, RawHoneyProductionId = lot.RawHoneyProductionId, Seq = seq,
                    ContainerCode = $"L{lot.RawHoneyProductionId}-C{seq}", Kind = c.Kind, NetWeightKg = c.Net, GrossWeightKg = c.Gross,
                    OriginSealNumber = c.OriginSeal, OriginSealIntact = c.OriginIntact, CreatedByUserId = userId, CreatedAt = now,
                    Closure = c.Closure, OrgContainerId = c.OrgContainer != null ? boxes[c.OrgContainer].OrgContainerId : null
                };
                // sealed at harvest: checked, opened for sampling, closed with one seal; unsealed: one seal after sampling
                if (c.OriginSeal != null)
                {
                    container.SealEvents.Add(new ContainerSealEvent { Event = SealEvent.Checked, SealNumber = c.OriginSeal, Intact = c.OriginIntact, UserId = userId, At = now, Note = "Sealed at harvest by the field inspector" });
                    container.SealEvents.Add(new ContainerSealEvent { Event = SealEvent.Broken, SealNumber = c.OriginSeal, UserId = userId, At = now, Note = "Opened for sampling" });
                    SealPoolService.Use(pool[c.OriginSeal], SealUse.Origin, userId, now, container);
                    SealPoolService.Break(pool[c.OriginSeal], now);
                }
                container.SealEvents.Add(new ContainerSealEvent { Event = SealEvent.Applied, SealNumber = c.Reseal, UserId = userId, At = now, Note = "Closed after sampling in front of the beekeeper" });
                SealPoolService.Use(pool[c.Reseal], SealUse.Closing, userId, now, container);
                var jar = new Sample
                {
                    SampleCode = $"S-{lot.RawHoneyProductionId}-C{seq}", RawHoneyProductionId = lot.RawHoneyProductionId, TakingOrgId = orgId,
                    LabOrgId = content.LabOrgId, SealNumber = c.JarSeal, CounterSealNumber = c.CounterSeal, CounterLocation = content.CounterLocation,
                    RetainUntil = content.RetainUntil, TakenByUserId = userId, TakenAt = now, Kind = SampleKind.Container,
                    JarCapIntactAtSealing = c.CapIntact, Status = SampleStatus.Sent, CapType = content.CapType
                };
                jar.Events.Add(new SampleCustodyEvent { Event = SampleStatus.Sent, FromOrgId = orgId, ToOrgId = content.LabOrgId, UserId = userId, At = now });
                container.Samples.Add(jar);
                SealPoolService.Use(pool[c.JarSeal], SealUse.Jar, userId, now, sample: jar);
                SealPoolService.Use(pool[c.CounterSeal], SealUse.CounterJar, userId, now, sample: jar);
                _context.Containers.Add(container);
            }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return new Saved(lot.RawHoneyProductionId, intake.IntakeId);
        }

        // The Compliance Officer accepts or rejects a signed photo (four-eyes: never the officer who recorded the delivery)
        public async Task ReviewAsync(LotIntake intake, bool accept, string? reason, string reviewerUserId)
        {
            intake.EvidenceStatus = accept ? EvidenceStatus.Accepted : EvidenceStatus.Rejected;
            intake.ReviewedByUserId = reviewerUserId;
            intake.ReviewedAt = Now;
            intake.ReviewReason = Clean(reason) is string r ? (r.Length > 500 ? r[..500] : r) : null;
            await _context.SaveChangesAsync();
        }

        // the seal currently on a container (the last applied or checked seal not broken since), or null
        public static string? CurrentSeal(IEnumerable<ContainerSealEvent> events)
        {
            string? current = null;
            foreach (var e in events.OrderBy(e => e.EventId))
                current = e.Event switch
                {
                    SealEvent.Applied or SealEvent.Checked or SealEvent.Replaced => e.SealNumber,
                    SealEvent.Broken when e.SealNumber == current => null,
                    _ => current
                };
            return current;
        }
    }
}
