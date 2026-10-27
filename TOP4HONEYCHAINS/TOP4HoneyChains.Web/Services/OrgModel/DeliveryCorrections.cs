using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.Messaging;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // D-78 P3 (D-82, P2 follow-up): the two-admin correction of a delivery recorded with containers. A platform administrator
    // requests it with a reason (date, weights, container kind, seal numbers); a different administrator confirms it. The old and
    // the new confirmed content stay in the correction row (JSON, readable), the intake gets the new content hash, the audit chain
    // logs every changed row, and the beekeeper gets an information SMS (M6). Containers are neither added nor removed.
    public class DeliveryCorrectionForm
    {
        public DateOnly? DeliveryDate { get; set; }
        public List<CorrectedContainer> Containers { get; set; } = new();
        public string? Reason { get; set; }
    }

    public class CorrectedContainer
    {
        public string? Kind { get; set; }
        public decimal? NetKg { get; set; }
        public decimal? GrossKg { get; set; }
        // the seal it arrived with (sealed arrival) or the plant seal applied at intake (unsealed arrival)
        public string? ArrivalSeal { get; set; }
        public string? JarSeal { get; set; }
        public string? CounterSeal { get; set; }
        public string? Reseal { get; set; }
    }

    public class DeliveryCorrectionService
    {
        // set while a confirmed correction is applied; the delivery locks of migration 024 let the change through
        public const string SessionKey = "top4.delivery_correction";

        private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

        private readonly TOP4HoneyChainsContext _context;
        private readonly CustodyIntakeService _custody;
        private readonly IMessageService _messages;
        private readonly AuditWriter _audit;
        private readonly TimeProvider _clock;

        public DeliveryCorrectionService(TOP4HoneyChainsContext context, CustodyIntakeService custody, IMessageService messages, AuditWriter audit, TimeProvider clock)
        {
            _context = context;
            _custody = custody;
            _messages = messages;
            _audit = audit;
            _clock = clock;
        }

        private DateTime Now => _clock.GetUtcNow().UtcDateTime;
        private DateOnly Today => DateOnly.FromDateTime(_clock.GetLocalNow().DateTime);

        public static string ToJson(IntakeContent content) => JsonSerializer.Serialize(content, Json);
        public static IntakeContent FromJson(string json) => JsonSerializer.Deserialize<IntakeContent>(json, Json)!;

        // The form prefilled with the delivery as stored
        public static DeliveryCorrectionForm FormOf(IntakeContent c) => new()
        {
            DeliveryDate = c.Date,
            Containers = c.Containers.Select(x => new CorrectedContainer
            {
                Kind = x.Kind, NetKg = x.Net, GrossKg = x.Gross, ArrivalSeal = x.OriginSeal ?? x.ArrivalPlantSeal, JarSeal = x.JarSeal,
                CounterSeal = x.CounterSeal, Reseal = x.Reseal
            }).ToList()
        };

        public async Task<(DeliveryCorrection? Correction, string? Error)> RequestAsync(int intakeId, DeliveryCorrectionForm f, string adminUserId)
        {
            var intake = await _context.LotIntakes.AsNoTracking().FirstOrDefaultAsync(i => i.IntakeId == intakeId);
            if (intake == null || intake.IsLegacy)
                return (null, "Only a delivery recorded with containers can be corrected here.");
            if (await _context.DeliveryCorrections.AnyAsync(c => c.IntakeId == intakeId && c.Status == CorrectionStatus.Pending))
                return (null, "A correction of this delivery already waits for a second administrator.");
            var old = await _custody.StoredContentAsync(intakeId);
            if (old == null)
                return (null, "Only a delivery recorded with containers can be corrected here.");
            var reason = f.Reason?.Trim();
            if (string.IsNullOrEmpty(reason))
                return (null, "A reason is required for a correction.");
            if (reason.Length > 500)
                return (null, "The reason can be at most 500 characters.");
            if (f.DeliveryDate == null || f.DeliveryDate > Today)
                return (null, "Enter the delivery date (not in the future).");
            if (f.Containers.Count != old.Containers.Count)
                return (null, "A correction keeps the containers of the delivery; it neither adds nor removes one.");

            var rows = new List<ContainerContent>();
            var seals = new List<string>();
            for (var i = 0; i < f.Containers.Count; i++)
            {
                var n = i + 1;
                var c = f.Containers[i];
                var o = old.Containers[i];
                string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
                var arrival = Clean(c.ArrivalSeal);
                var jar = Clean(c.JarSeal);
                var counter = Clean(c.CounterSeal);
                var reseal = Clean(c.Reseal);
                if (!ContainerKind.All.Contains(c.Kind ?? ""))
                    return (null, string.Format(CultureInfo.InvariantCulture, "Container {0}: choose the kind of container.", n));
                if (c.NetKg is not (> 0 and <= 100000))
                    return (null, string.Format(CultureInfo.InvariantCulture, "Container {0}: enter the net weight (more than 0 kg).", n));
                if (c.GrossKg != null && c.GrossKg < c.NetKg)
                    return (null, string.Format(CultureInfo.InvariantCulture, "Container {0}: the gross weight cannot be lower than the net weight.", n));
                if (arrival == null || jar == null || counter == null || reseal == null)
                    return (null, string.Format(CultureInfo.InvariantCulture, "Container {0}: every seal number is required.", n));
                if (new[] { arrival, jar, counter, reseal }.Any(s => s.Length > 50))
                    return (null, string.Format(CultureInfo.InvariantCulture, "Container {0}: seal numbers have at most 50 characters.", n));
                seals.AddRange(new[] { arrival, jar, counter, reseal });
                var sealedArrival = o.OriginSeal != null;
                rows.Add(o with
                {
                    Kind = c.Kind!, Net = c.NetKg!.Value, Gross = c.GrossKg,
                    OriginSeal = sealedArrival ? arrival : null, ArrivalPlantSeal = sealedArrival ? null : arrival,
                    JarSeal = jar, CounterSeal = counter, Reseal = reseal
                });
            }
            if (seals.GroupBy(s => s, StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1))
                return (null, "Every seal number of the delivery must be different.");
            // seal numbers used later (new jars, seals after re-sampling) stay taken
            var oldSeals = old.Containers.SelectMany(c => new[] { c.OriginSeal, c.ArrivalPlantSeal, c.JarSeal, c.CounterSeal, c.Reseal }).OfType<string>().ToHashSet(StringComparer.OrdinalIgnoreCase);
            var used = await _context.ContainerSealEvents.Where(e => e.Container!.IntakeId == intakeId).Select(e => e.SealNumber)
                .Concat(_context.Samples.Where(s => s.Container!.IntakeId == intakeId).Select(s => s.SealNumber))
                .Concat(_context.Samples.Where(s => s.Container!.IntakeId == intakeId).Select(s => s.CounterSealNumber)).ToListAsync();
            if (seals.Any(s => !oldSeals.Contains(s) && used.Contains(s, StringComparer.OrdinalIgnoreCase)))
                return (null, "Every seal number of the delivery must be different.");

            var updated = old with { Date = f.DeliveryDate.Value, Containers = rows };
            if (updated.Hash() == old.Hash())
                return (null, "The correction does not change anything.");
            var usedInBatches = await _context.FinalHoneyProductionDetails.Where(d => d.RawHoneyProductionId == intake.RawHoneyProductionId).SumAsync(d => (decimal?)d.UsedAmount) ?? 0;
            if (updated.TotalKg < usedInBatches)
                return (null, string.Format(CultureInfo.InvariantCulture, "Batches already use {0} kg of this lot; the corrected amount cannot be lower.", usedInBatches.ToString("0.##", CultureInfo.InvariantCulture)));

            var correction = new DeliveryCorrection
            {
                IntakeId = intakeId, RawHoneyProductionId = intake.RawHoneyProductionId,
                OldContent = ToJson(old), OldContentSha256 = old.Hash(), NewContent = ToJson(updated), NewContentSha256 = updated.Hash(),
                Reason = reason, RequestedByUserId = adminUserId, RequestedAt = Now
            };
            _context.DeliveryCorrections.Add(correction);
            await _context.SaveChangesAsync();
            return (correction, null);
        }

        // A different administrator confirms (applies) or rejects the correction
        public async Task<string?> DecideAsync(int correctionId, bool approve, string? note, string adminUserId)
        {
            var c = await _context.DeliveryCorrections.FirstOrDefaultAsync(x => x.CorrectionId == correctionId);
            if (c == null)
                return "The correction was not found.";
            note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
            if (c.Status != CorrectionStatus.Pending)
                return "This correction was already decided.";
            if (c.RequestedByUserId == adminUserId)
                return "Four-eyes: the person who requested a correction cannot decide it.";
            if (!approve && note == null)
                return "A note is required to reject a correction.";
            if (note?.Length > 500)
                return "The note can be at most 500 characters.";

            await using var transaction = await _context.Database.BeginTransactionAsync();
            c.Status = approve ? CorrectionStatus.Applied : CorrectionStatus.Rejected;
            c.DecidedByUserId = adminUserId;
            c.DecidedAt = Now;
            c.DecisionNote = note;
            if (approve)
            {
                var current = await _custody.StoredContentAsync(c.IntakeId);
                if (current == null || current.Hash() != c.OldContentSha256.Trim())
                {
                    await transaction.RollbackAsync();
                    return "The delivery changed after the correction was requested; request it again.";
                }
                await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC sp_set_session_context @key = {SessionKey}, @value = {c.CorrectionId}");
                await ApplyAsync(c, current, FromJson(c.NewContent));
                await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC sp_set_session_context @key = {SessionKey}, @value = NULL");
                var check = await _custody.StoredContentAsync(c.IntakeId);
                if (check == null || check.Hash() != c.NewContentSha256.Trim())
                    throw new InvalidOperationException("The corrected delivery does not have the requested content.");
            }
            await _context.SaveChangesAsync();
            await _audit.WriteAsync(approve ? "Correct" : "Reject", "DeliveryCorrections", c.CorrectionId.ToString(CultureInfo.InvariantCulture),
                new { c.OldContentSha256 }, new { c.NewContentSha256, c.Reason, c.DecisionNote }, "Correction");
            await transaction.CommitAsync();

            if (approve)
            {
                // M6: the beekeeper is told about the correction (after the commit: a failed SMS never undoes it)
                c.InfoSmsId = await InformAsync(c, FromJson(c.NewContent), adminUserId);
                if (c.InfoSmsId != null)
                    await _context.SaveChangesAsync();
            }
            return null;
        }

        private async Task ApplyAsync(DeliveryCorrection c, IntakeContent old, IntakeContent updated)
        {
            var intake = await _context.LotIntakes.FirstAsync(i => i.IntakeId == c.IntakeId);
            intake.DeliveryDate = updated.Date;
            intake.Quantity = updated.TotalKg;
            intake.ContentSha256 = updated.Hash();
            var lot = await _context.RawHoneyProductions.FirstAsync(r => r.RawHoneyProductionId == c.RawHoneyProductionId);
            lot.RawHoneyProductionAmount = updated.TotalKg;
            lot.RawHoneyProductionDate = updated.Date;

            var containers = await _context.Containers.Include(x => x.SealEvents).Include(x => x.Samples)
                .Where(x => x.IntakeId == c.IntakeId).OrderBy(x => x.Seq).ToListAsync();
            for (var i = 0; i < containers.Count; i++)
            {
                var k = containers[i];
                var was = old.Containers[i];
                var now = updated.Containers[i];
                k.Kind = now.Kind;
                k.NetWeightKg = now.Net;
                k.GrossWeightKg = now.Gross;
                if (was.OriginSeal != null)
                    k.OriginSealNumber = now.OriginSeal;
                var oldArrival = was.OriginSeal ?? was.ArrivalPlantSeal;
                var newArrival = now.OriginSeal ?? now.ArrivalPlantSeal;
                foreach (var e in k.SealEvents)
                {
                    if (e.SealNumber == oldArrival && newArrival != null)
                        e.SealNumber = newArrival;
                    else if (e.SealNumber == was.Reseal)
                        e.SealNumber = now.Reseal;
                }
                var jar = k.Samples.Where(s => s.Kind == SampleKind.Container).OrderBy(s => s.SampleId).FirstOrDefault();
                if (jar != null)
                {
                    jar.SealNumber = now.JarSeal;
                    jar.CounterSealNumber = now.CounterSeal;
                }
            }
            await _context.SaveChangesAsync();
        }

        private async Task<long?> InformAsync(DeliveryCorrection c, IntakeContent updated, string adminUserId)
        {
            var lot = await _context.RawHoneyProductions.AsNoTracking().Include(r => r.RawHoneyProductionApiary!).ThenInclude(a => a.ApiaryBeekeeper)
                .Include(r => r.RawHoneyProductionHoneyTypeNavigation).FirstAsync(r => r.RawHoneyProductionId == c.RawHoneyProductionId);
            var bk = lot.RawHoneyProductionApiary?.ApiaryBeekeeper;
            if (string.IsNullOrWhiteSpace(bk?.BeekeeperPhoneNumber))
                return null;
            var orgName = await _context.LotIntakes.Where(i => i.IntakeId == c.IntakeId).Select(i => i.ReceivingOrg!.DisplayName).FirstAsync();
            var language = bk.PreferredLanguage;
            var values = new Dictionary<string, string>
            {
                ["org"] = orgName,
                ["date"] = updated.Date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                ["containers"] = SmsTemplates.ContainersText(updated.Containers.Select(x => x.Kind), language),
                ["kg"] = updated.TotalKg.ToString("0.##", CultureInfo.GetCultureInfo(SmsTemplates.Language(language))),
                ["honey"] = lot.RawHoneyProductionHoneyTypeNavigation?.HoneyTypeTitle ?? ""
            };
            var result = await _messages.SendTextAsync(SmsPurpose.DeliveryCorrected, "DeliveryCorrection", c.CorrectionId.ToString(CultureInfo.InvariantCulture),
                bk.BeekeeperPhoneNumber!, language, values, senderUserId: adminUserId);
            return result.SmsId;
        }
    }
}
