using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.Messaging;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-78 P2 (D78-SPEC 4.4, 4.5): one container of a delivery as the sample acceptance officer records it: the container, how it
    // arrived (origin seal checked, or a plant seal applied in front of the beekeeper), its jar and counter-sample, the cap check,
    // and the new plant seal after sampling
    public class ContainerRow
    {
        public string? Kind { get; set; } = ContainerKind.Tin;
        public decimal? NetKg { get; set; }
        public decimal? GrossKg { get; set; }
        public bool ArrivedSealed { get; set; }
        public string? OriginSeal { get; set; }
        public bool OriginSealIntact { get; set; }
        public string? PlantSeal { get; set; }
        public string? JarSeal { get; set; }
        public string? CounterSeal { get; set; }
        public bool CapIntact { get; set; }
        public string? Reseal { get; set; }
    }

    public class IntakeForm
    {
        public string? DraftKey { get; set; }
        public string? Number { get; set; }
        public int ApiaryId { get; set; }
        public int HoneyTypeId { get; set; }
        public int PeriodId { get; set; }
        public DateOnly? DeliveryDate { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? OriginCountry { get; set; }
        public int LabOrgId { get; set; }
        public string? CounterLocation { get; set; }
        public DateOnly? RetainUntil { get; set; }
        public List<ContainerRow> Containers { get; set; } = new();
        public string? Code { get; set; }
        public string? FallbackReason { get; set; }
    }

    // What the beekeeper confirms, in a form that can be rebuilt from the stored delivery (recall check, D78-SPEC 3.10)
    public record ContainerContent(string Kind, decimal Net, decimal? Gross, string? OriginSeal, bool? OriginIntact, string? ArrivalPlantSeal,
        string JarSeal, string CounterSeal, bool CapIntact, string Reseal);

    public record IntakeContent(string BeekeeperId, int ApiaryId, int HoneyTypeId, int PeriodId, string Origin, DateOnly Date, string Province,
        string District, int LabOrgId, string CounterLocation, DateOnly RetainUntil, IReadOnlyList<ContainerContent> Containers)
    {
        public decimal TotalKg => Containers.Sum(c => c.Net);

        public string Canonical()
        {
            static string D(decimal? v) => v?.ToString("0.##", CultureInfo.InvariantCulture) ?? "";
            static string B(bool? v) => v == null ? "" : v.Value ? "1" : "0";
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

        public CustodyIntakeService(TOP4HoneyChainsContext context, TimeProvider clock)
        {
            _context = context;
            _clock = clock;
        }

        private DateTime Now => _clock.GetUtcNow().UtcDateTime;

        private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        // The container part of the checks (the delivery fields are checked by the controller as for the legacy intake)
        public List<string> CheckContainers(IntakeForm f)
        {
            var errors = new List<string>();
            if (f.Containers.Count == 0)
                errors.Add("Record at least one container.");
            if (f.Containers.Count > MaxContainers)
                errors.Add("At most 100 containers per delivery.");
            var seals = new List<string>();
            for (var i = 0; i < f.Containers.Count; i++)
            {
                var c = f.Containers[i];
                var n = i + 1;
                c.OriginSeal = Clean(c.OriginSeal);
                c.PlantSeal = Clean(c.PlantSeal);
                c.JarSeal = Clean(c.JarSeal);
                c.CounterSeal = Clean(c.CounterSeal);
                c.Reseal = Clean(c.Reseal);
                if (!ContainerKind.All.Contains(c.Kind ?? ""))
                    errors.Add($"Container {n}: choose the kind of container.");
                if (c.NetKg is not (> 0 and <= 100000))
                    errors.Add($"Container {n}: enter the net weight (more than 0 kg).");
                if (c.GrossKg != null && c.GrossKg < c.NetKg)
                    errors.Add($"Container {n}: the gross weight cannot be lower than the net weight.");
                var arrival = c.ArrivedSealed ? c.OriginSeal : c.PlantSeal;
                if (arrival == null)
                    errors.Add(c.ArrivedSealed ? $"Container {n}: enter the number of the seal it arrived with." : $"Container {n}: apply a plant seal in front of the beekeeper and enter its number.");
                if (c.JarSeal == null || c.CounterSeal == null)
                    errors.Add($"Container {n}: enter the seal numbers of the sample jar and of the counter-sample jar.");
                if (!c.CapIntact)
                    errors.Add($"Container {n}: the jar's cap must be intact (button down) when sealed. A popped cap: discard the jar and fill a new one.");
                if (c.Reseal == null)
                    errors.Add($"Container {n}: enter the new plant seal applied after sampling.");
                foreach (var s in new[] { arrival, c.JarSeal, c.CounterSeal, c.Reseal })
                    if (s != null && s.Length > 50)
                        errors.Add($"Container {n}: seal numbers have at most 50 characters.");
                seals.AddRange(new[] { arrival, c.JarSeal, c.CounterSeal, c.Reseal }.Where(s => s != null)!);
            }
            if (seals.GroupBy(s => s, StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1))
                errors.Add("Every seal number of the delivery must be different.");
            return errors;
        }

        public static IntakeContent ContentOf(IntakeForm f, string beekeeperId) =>
            new(beekeeperId, f.ApiaryId, f.HoneyTypeId, f.PeriodId, f.OriginCountry ?? Origin.Default, f.DeliveryDate!.Value, f.Province!.Trim(), f.District!.Trim(),
                f.LabOrgId, f.CounterLocation!.Trim(), f.RetainUntil!.Value,
                f.Containers.Select(c => new ContainerContent(c.Kind!, c.NetKg!.Value, c.GrossKg,
                    c.ArrivedSealed ? c.OriginSeal : null, c.ArrivedSealed ? c.OriginSealIntact : null, c.ArrivedSealed ? null : c.PlantSeal,
                    c.JarSeal!, c.CounterSeal!, c.CapIntact, c.Reseal!)).ToList());

        // The content of a stored delivery with containers, rebuilt from the lot, the containers, their seal events and jars
        public async Task<IntakeContent?> StoredContentAsync(int intakeId)
        {
            var intake = await _context.LotIntakes.AsNoTracking().FirstOrDefaultAsync(i => i.IntakeId == intakeId);
            if (intake == null || intake.IsLegacy)
                return null;
            var lot = await _context.RawHoneyProductions.AsNoTracking().Include(r => r.RawHoneyProductionApiary)
                .FirstAsync(r => r.RawHoneyProductionId == intake.RawHoneyProductionId);
            var containers = await _context.Containers.AsNoTracking().Include(c => c.SealEvents).Include(c => c.Samples)
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
                rows.Add(new ContainerContent(c.Kind, c.NetWeightKg, c.GrossWeightKg, c.OriginSealNumber, c.OriginSealIntact,
                    arrival?.Event == SealEvent.Applied ? arrival.SealNumber : null,
                    jar?.SealNumber ?? "", jar?.CounterSealNumber ?? "", jar?.JarCapIntactAtSealing == true, reseal?.SealNumber ?? ""));
            }
            return new IntakeContent(lot.RawHoneyProductionApiary?.ApiaryBeekeeperId ?? "", lot.RawHoneyProductionApiaryId ?? 0, lot.RawHoneyProductionHoneyType ?? 0,
                lot.RawHoneyProductionPeriodId ?? 0, lot.OriginCountry ?? Origin.Default, intake.DeliveryDate, intake.Province, intake.District,
                anyJar?.LabOrgId ?? 0, anyJar?.CounterLocation ?? "", anyJar?.RetainUntil ?? default, rows);
        }

        // "Verified" when the stored delivery still has the hash the beekeeper confirmed; null for a legacy delivery
        public async Task<bool?> ContentMatchesAsync(int intakeId)
        {
            var intake = await _context.LotIntakes.AsNoTracking().FirstOrDefaultAsync(i => i.IntakeId == intakeId);
            if (intake == null || intake.IsLegacy || intake.ContentSha256 == null)
                return null;
            var content = await StoredContentAsync(intakeId);
            return content != null && string.Equals(content.Hash(), intake.ContentSha256.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        // Lot, intake, containers, seal events (arrival, broken for sampling, new plant seal) and one jar per container, in one
        // transaction. SMS: confirmed at once. Photo: waits for the Compliance Officer (the lot cannot be finalized before).
        public async Task<Saved> SaveAsync(IntakeForm f, IntakeContent content, int orgId, int? legacyWholesalerId, string userId,
            long? codeId, (string Stored, string Original, string Sha256)? photo)
        {
            var now = Now;
            await using var transaction = await _context.Database.BeginTransactionAsync();
            var lot = new RawHoneyProduction
            {
                RawHoneyProductionApiaryId = f.ApiaryId,
                RawHoneyProductionPeriodId = f.PeriodId,
                RawHoneyProductionHoneyType = f.HoneyTypeId,
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
                ContainersRecorded = true
            };
            _context.LotIntakes.Add(intake);
            await _context.SaveChangesAsync();

            for (var i = 0; i < content.Containers.Count; i++)
            {
                var c = content.Containers[i];
                var seq = i + 1;
                var container = new Container
                {
                    IntakeId = intake.IntakeId, RawHoneyProductionId = lot.RawHoneyProductionId, Seq = seq,
                    ContainerCode = $"L{lot.RawHoneyProductionId}-C{seq}", Kind = c.Kind, NetWeightKg = c.Net, GrossWeightKg = c.Gross,
                    OriginSealNumber = c.OriginSeal, OriginSealIntact = c.OriginIntact, CreatedByUserId = userId, CreatedAt = now
                };
                // arrival, opened for sampling, sealed again (D78-SPEC 3.2)
                if (c.OriginSeal != null)
                    container.SealEvents.Add(new ContainerSealEvent { Event = SealEvent.Checked, SealNumber = c.OriginSeal, Intact = c.OriginIntact, UserId = userId, At = now });
                else
                    container.SealEvents.Add(new ContainerSealEvent { Event = SealEvent.Applied, SealNumber = c.ArrivalPlantSeal!, UserId = userId, At = now, Note = "Applied at intake in front of the beekeeper" });
                container.SealEvents.Add(new ContainerSealEvent { Event = SealEvent.Broken, SealNumber = c.OriginSeal ?? c.ArrivalPlantSeal!, UserId = userId, At = now, Note = "Opened for sampling" });
                container.SealEvents.Add(new ContainerSealEvent { Event = SealEvent.Applied, SealNumber = c.Reseal, UserId = userId, At = now, Note = "Sealed again after sampling" });
                var jar = new Sample
                {
                    SampleCode = $"S-{lot.RawHoneyProductionId}-C{seq}", RawHoneyProductionId = lot.RawHoneyProductionId, TakingOrgId = orgId,
                    LabOrgId = content.LabOrgId, SealNumber = c.JarSeal, CounterSealNumber = c.CounterSeal, CounterLocation = content.CounterLocation,
                    RetainUntil = content.RetainUntil, TakenByUserId = userId, TakenAt = now, Kind = SampleKind.Container,
                    JarCapIntactAtSealing = c.CapIntact, Status = SampleStatus.Sent
                };
                jar.Events.Add(new SampleCustodyEvent { Event = SampleStatus.Sent, FromOrgId = orgId, ToOrgId = content.LabOrgId, UserId = userId, At = now });
                container.Samples.Add(jar);
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
