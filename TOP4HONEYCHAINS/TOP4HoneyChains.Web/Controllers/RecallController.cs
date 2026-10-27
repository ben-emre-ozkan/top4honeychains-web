using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Controllers
{
    // Recall report (ROLE-MODEL section 6, D-41). From a batch: the upstream lots, beekeepers and intake
    // forms and the downstream shipments and holders. From a lot: the batches and shipments it went into.
    // Each organization sees only its part; platform administrators see everything, read-only.
    [RequireOrgModel]
    [Authorize(Policy = AuthPolicies.PanelUser)]
    public class RecallController : Controller
    {
        private readonly TOP4HoneyChainsContext _context;
        private readonly ICurrentOrg _current;
        private readonly CcpResponsibilityService _resp;
        private readonly CustodyService _custody;
        // D-56: rights of organization roles (editable matrix)
        private TOP4HoneyChains.Web.Services.OrgModel.Permissions Perm => HttpContext.RequestServices.GetRequiredService<TOP4HoneyChains.Web.Services.OrgModel.Permissions>();

        public RecallController(TOP4HoneyChainsContext context, ICurrentOrg current, CcpResponsibilityService resp, CustodyService custody)
        {
            _context = context;
            _current = current;
            _resp = resp;
            _custody = custody;
        }

        public record LotRow(int LotId, decimal? Amount, decimal? UsedAmount, string? HoneyType, string? Apiary, string? Beekeeper, string? BeekeeperNumber,
            int? IntakeId, DateOnly? DeliveryDate, string? IntakeOrg, decimal? IntakeQuantity, string? Origin, string? FormSha256, string? SmsStatus,
            IntakeCustody? Custody = null);
        // D-78 P2 (D78-SPEC 3.11): a delivery recorded with containers: how it was confirmed, whether the stored content still matches
        // the confirmed hash, and every container with its seal history and sample jars. Null for a legacy intake.
        public record IntakeCustody(string? Method, string? EvidenceStatus, string? ContentSha256, bool? ContentMatches, List<ContainerRow> Containers,
            List<ContainerLabRow>? Lab = null);
        // D-82: the approved lab values of every container (the lot's CCP 1 value is the worst of them in per-container mode)
        public record ContainerLabRow(string ContainerCode, string ReportCode, List<(int ParameterId, string Parameter, string Value)> Values);
        public record ContainerRow(string Code, string Kind, decimal NetKg, string? CurrentSeal, List<SealRow> Seals, List<JarRow> Jars);
        public record SealRow(string Event, string SealNumber, bool? Intact, DateTime At);
        public record JarRow(string Code, string Seal, string Status, string? Kind);
        public record BatchRow(int BatchId, string? Title, DateTime? Date, bool Finalized, decimal? Total, decimal? UsedAmount, string? Owner);
        public record ShipmentRow(int ShipmentId, int BatchId, string From, string To, decimal Quantity, string Status, DateTime ShippedAt, DateTime? DecidedAt, string? RejectReason);
        public record HolderRow(int BatchId, string Org, decimal Held);

        public class Report
        {
            public string Kind { get; set; } = "";
            public int Id { get; set; }
            public string Title { get; set; } = "";
            public string Scope { get; set; } = "";
            public List<LotRow> Lots { get; } = new();
            public int HiddenLots { get; set; }
            public List<BatchRow> Batches { get; } = new();
            public int HiddenBatches { get; set; }
            public List<ShipmentRow> Shipments { get; } = new();
            public int HiddenShipments { get; set; }
            public List<HolderRow> Holders { get; } = new();
        }

        // ---------------------------------------------------------------- batch

        public async Task<IActionResult> Batch(int id, string? format)
        {
            var batch = await _context.FinalHoneyProductions.AsNoTracking().FirstOrDefaultAsync(f => f.FinalHoneyProductionId == id);
            if (batch == null)
                return NotFound();
            var admin = _current.IsPlatformAdmin;
            var active = await _current.GetAsync();
            if (!admin && active != null && !Perm.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Recall, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Read, active.OrgRole))
                return NotFound();
            var orgId = active?.OrgId ?? 0;
            var owner = await _resp.BatchOwnerAsync(id);
            var isOwner = !admin && owner == orgId && orgId != 0;
            if (!admin && !isOwner && (orgId == 0 || !await _custody.TakesPartAsync(id, orgId)))
                return NotFound();

            var report = new Report { Kind = "batch", Id = id, Title = batch.FinalHoneyProductionTitle ?? $"#{id}",
                Scope = admin ? "Scope.All" : isOwner ? "Scope.Owner" : "Scope.Holder" };

            // upstream: owner and administrators only (beekeepers are personal data)
            var lotIds = await _context.FinalHoneyProductionDetails.Where(d => d.FinalHoneyProductionId == id)
                .Select(d => new { d.RawHoneyProductionId, d.UsedAmount }).ToListAsync();
            if (admin || isOwner)
                foreach (var l in lotIds)
                    report.Lots.Add(await LotRowAsync(l.RawHoneyProductionId, l.UsedAmount));
            else
                report.HiddenLots = lotIds.Count;

            await AddShipmentsAsync(report, new[] { id }, admin, orgId);
            var holdings = await _custody.HoldingsAsync(id);
            var names = await OrgNamesAsync(holdings.Keys);
            foreach (var h in holdings.Values.Where(h => h.Held > 0 && (admin || h.OrgId == orgId)))
                report.Holders.Add(new HolderRow(id, names.GetValueOrDefault(h.OrgId, $"#{h.OrgId}"), h.Held));
            return Output(report, format);
        }

        // ---------------------------------------------------------------- lot

        public async Task<IActionResult> Lot(int id, string? format)
        {
            var lot = await _context.RawHoneyProductions.AsNoTracking().FirstOrDefaultAsync(r => r.RawHoneyProductionId == id);
            if (lot == null)
                return NotFound();
            var admin = _current.IsPlatformAdmin;
            var active = await _current.GetAsync();
            if (!admin && active != null && !Perm.Allows(TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Recall, TOP4HoneyChains.Web.Models.Entities.PermissionAreas.Read, active.OrgRole))
                return NotFound();
            var orgId = active?.OrgId ?? 0;
            var batches = await _context.FinalHoneyProductionDetails.Where(d => d.RawHoneyProductionId == id)
                .Select(d => new { d.FinalHoneyProductionId, d.UsedAmount }).ToListAsync();
            var ownedBatches = new List<int>();
            foreach (var b in batches)
                if (orgId != 0 && await _resp.BatchOwnerAsync(b.FinalHoneyProductionId) == orgId)
                    ownedBatches.Add(b.FinalHoneyProductionId);
            var lotOwner = !admin && orgId != 0 && await _resp.LotOwnerAsync(id) == orgId;
            var association = !admin && orgId != 0 && active!.Has(Capabilities.Association)
                && await _context.BeekeeperAssociations.AnyAsync(a => a.AssociationOrgId == orgId && a.ValidTo == null
                    && a.BeekeeperId == _context.Apiaries.Where(ap => ap.ApiaryId == lot.RawHoneyProductionApiaryId).Select(ap => ap.ApiaryBeekeeperId).FirstOrDefault());
            if (!admin && !lotOwner && ownedBatches.Count == 0 && !association)
                return NotFound();

            var report = new Report { Kind = "lot", Id = id, Title = $"#{id}",
                Scope = admin ? "Scope.All" : lotOwner || ownedBatches.Count > 0 ? "Scope.Owner" : "Scope.Association" };
            var row = await LotRowAsync(id, null);
            // the association sees its member's lot, not the processor's intake form
            report.Lots.Add(admin || lotOwner || ownedBatches.Count > 0 ? row
                : row with { IntakeId = null, DeliveryDate = null, IntakeOrg = null, IntakeQuantity = null, FormSha256 = null, SmsStatus = null, Custody = null });

            var visibleBatches = admin ? batches.Select(b => b.FinalHoneyProductionId).ToList() : association && !lotOwner ? new() : ownedBatches;
            report.HiddenBatches = batches.Count - visibleBatches.Count;
            foreach (var b in batches.Where(b => visibleBatches.Contains(b.FinalHoneyProductionId)))
            {
                var f = await _context.FinalHoneyProductions.AsNoTracking().FirstAsync(x => x.FinalHoneyProductionId == b.FinalHoneyProductionId);
                var ownerId = await _resp.BatchOwnerAsync(f.FinalHoneyProductionId);
                report.Batches.Add(new BatchRow(f.FinalHoneyProductionId, f.FinalHoneyProductionTitle, f.FinalHoneyProductionDate,
                    f.FinalHoneyProductionIsFinalized, f.FinalHoneyProductionTotalAmount, b.UsedAmount,
                    ownerId == null ? null : (await OrgNamesAsync(new[] { ownerId.Value })).GetValueOrDefault(ownerId.Value)));
            }
            if (visibleBatches.Count > 0)
                await AddShipmentsAsync(report, visibleBatches, admin, orgId);
            return Output(report, format);
        }

        // ---------------------------------------------------------------- helpers

        private async Task<LotRow> LotRowAsync(int lotId, decimal? used)
        {
            var r = await _context.RawHoneyProductions.AsNoTracking()
                .Include(x => x.RawHoneyProductionApiary!).ThenInclude(a => a.ApiaryBeekeeper)
                .Include(x => x.RawHoneyProductionHoneyTypeNavigation)
                .FirstAsync(x => x.RawHoneyProductionId == lotId);
            var intake = await _context.LotIntakes.AsNoTracking().Include(i => i.ReceivingOrg).FirstOrDefaultAsync(i => i.RawHoneyProductionId == lotId);
            var bk = r.RawHoneyProductionApiary?.ApiaryBeekeeper;
            IntakeCustody? custody = null;
            if (intake is { ContainersRecorded: true })
            {
                var containers = await _context.Containers.AsNoTracking().Include(c => c.SealEvents).Include(c => c.Samples)
                    .Where(c => c.IntakeId == intake.IntakeId).OrderBy(c => c.Seq).ToListAsync();
                var composite = await _context.Samples.AsNoTracking().Where(s => s.RawHoneyProductionId == lotId && s.Kind == SampleKind.Composite)
                    .Select(s => new JarRow(s.SampleCode, s.SealNumber, s.Status, s.Kind)).ToListAsync();
                var check = HttpContext.RequestServices.GetRequiredService<CustodyIntakeService>();
                custody = new IntakeCustody(intake.ConfirmationMethod, intake.EvidenceStatus, intake.ContentSha256?.Trim(), await check.ContentMatchesAsync(intake.IntakeId),
                    containers.Select(c => new ContainerRow(c.ContainerCode, c.Kind, c.NetWeightKg, CustodyIntakeService.CurrentSeal(c.SealEvents),
                        c.SealEvents.OrderBy(e => e.EventId).Select(e => new SealRow(e.Event, e.SealNumber, e.Intact, e.At)).ToList(),
                        c.Samples.OrderBy(s => s.SampleId).Select(s => new JarRow(s.SampleCode, s.SealNumber, s.Status, s.Kind)).ToList())).ToList());
                if (composite.Count > 0)
                    custody.Containers.Add(new ContainerRow("", SampleKind.Composite, 0, null, new(), composite));
                var lab = await HttpContext.RequestServices.GetRequiredService<LabCustodyService>().ContainerResultsAsync(lotId);
                if (lab.Count > 0)
                {
                    var names = await _context.CriticalControlPointParameters.AsNoTracking().ToDictionaryAsync(p => p.ParameterId, p => p.ParameterName);
                    custody = custody with
                    {
                        Lab = lab.Select(r => new ContainerLabRow(r.ContainerCode, r.ReportCode, r.Results.OrderBy(x => x.ParameterId)
                            .Select(x => (x.ParameterId, names.GetValueOrDefault(x.ParameterId, $"#{x.ParameterId}"),
                                x.MeasuredNumericValue?.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture) ?? x.MeasuredTextValue ?? "")).ToList())).ToList()
                    };
                }
            }
            return new LotRow(lotId, r.RawHoneyProductionAmount, used, r.RawHoneyProductionHoneyTypeNavigation?.HoneyTypeTitle,
                r.RawHoneyProductionApiary?.ApiaryTitle, bk == null ? null : $"{bk.BeekeeperFirstName} {bk.BeekeeperLastName}", bk?.BeekeeperBusinessNumber,
                intake?.IntakeId, intake?.DeliveryDate, intake?.ReceivingOrg?.DisplayName, intake?.Quantity,
                intake == null ? null : $"{intake.Province} / {intake.District}", intake?.FormSha256, intake?.SmsStatus, custody);
        }

        private async Task AddShipmentsAsync(Report report, IEnumerable<int> batchIds, bool admin, int orgId)
        {
            var ids = batchIds.ToList();
            var all = await _context.Shipments.AsNoTracking().Include(s => s.FromOrg).Include(s => s.ToOrg)
                .Where(s => ids.Contains(s.FinalHoneyProductionId)).OrderBy(s => s.ShipmentId).ToListAsync();
            foreach (var s in all)
            {
                if (!admin && s.FromOrgId != orgId && s.ToOrgId != orgId)
                {
                    report.HiddenShipments++;
                    continue;
                }
                report.Shipments.Add(new ShipmentRow(s.ShipmentId, s.FinalHoneyProductionId, s.FromOrg!.DisplayName, s.ToOrg!.DisplayName,
                    s.Quantity, s.Status, s.ShippedAt, s.DecidedAt, s.RejectReason));
            }
        }

        private Task<Dictionary<int, string>> OrgNamesAsync(IEnumerable<int> ids)
        {
            var list = ids.ToList();
            return _context.Organizations.Where(o => list.Contains(o.OrgId)).ToDictionaryAsync(o => o.OrgId, o => o.DisplayName);
        }

        private IActionResult Output(Report report, string? format)
        {
            if (!string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Controller = "Recall";
                ViewBag.Action = "Kind." + report.Kind;
                return View("Report", report);
            }
            // D-51: the export follows the selected language, like the page
            var L = HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Localization.IStringLocalizer<SharedResource>>();
            var refText = HttpContext.RequestServices.GetRequiredService<RefText>();
            var sb = new StringBuilder();
            void Row(params object?[] cells) => sb.AppendLine(string.Join(",", cells.Select(Csv)));
            Row(L["Section"].Value, "Id", L["Name"].Value, L["Detail"].Value, L["Quantity (kg)"].Value, L["Date"].Value, L["Status"].Value, L["Extra"].Value);
            Row(L["Report"].Value, report.Id, report.Title, L["Kind." + report.Kind].Value, null, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm 'UTC'"), report.Scope switch
            {
                "Scope.All" => L["all data (platform administrator)"].Value,
                "Scope.Owner" => L["owning organization's part"].Value,
                "Scope.Holder" => L["shipments to and from the organization"].Value,
                _ => L["association's member lot"].Value
            }, null);
            foreach (var l in report.Lots)
            {
                Row(L["Lot"].Value, l.LotId, l.Apiary, refText.Text(Models.Entities.RefEntity.HoneyType, "Title", l.HoneyType), l.UsedAmount ?? l.Amount, null, null, null);
                Row(L["Beekeeper"].Value, l.LotId, l.Beekeeper, l.BeekeeperNumber, null, null, null, l.Origin);
                if (l.IntakeId != null && l.Custody == null)
                    Row(L["Intake"].Value, l.IntakeId, l.IntakeOrg, l.Origin, l.IntakeQuantity, l.DeliveryDate?.ToString("yyyy-MM-dd"), $"{l.SmsStatus} ({L["Ci.Legacy"].Value})", "SHA-256 " + l.FormSha256);
                else if (l.Custody is { } c)
                {
                    // D-78 P2: the confirmed content, every container with its seal history and its jars
                    Row(L["Intake"].Value, l.IntakeId, l.IntakeOrg, l.Origin, l.IntakeQuantity, l.DeliveryDate?.ToString("yyyy-MM-dd"),
                        $"{L["Reg78.Method." + c.Method].Value} / {L["Reg78.Evidence." + c.EvidenceStatus].Value}",
                        $"{L["Ci.ContentHash"].Value} SHA-256 {c.ContentSha256}: {(c.ContentMatches == true ? L["Ci.ContentVerified"].Value : L["Ci.ContentChanged"].Value)}");
                    foreach (var k in c.Containers)
                    {
                        if (k.Kind != SampleKind.Composite)
                            Row(L["Ci.Container"].Value, k.Code, L["Ci.Kind." + k.Kind].Value, k.CurrentSeal, k.NetKg, null, null,
                                string.Join(" > ", k.Seals.Select(e => $"{L["Ci.Seal." + e.Event].Value} {e.SealNumber}{(e.Intact == false ? " (" + L["Ci.NotIntact"].Value + ")" : "")} {e.At:yyyy-MM-dd HH:mm} UTC")));
                        foreach (var j in k.Jars)
                            Row(j.Kind == SampleKind.Composite ? L["Lab.Composite"].Value : L["Ci.SampleJar"].Value, j.Code, k.Code, j.Seal, null, null, L["Lab.Status" + j.Status].Value, null);
                    }
                    foreach (var r in c.Lab ?? new())
                        Row(L["Lab.ContainerValues"].Value, r.ContainerCode, r.ReportCode, null, null, null, null,
                            string.Join("; ", r.Values.Select(v => $"{refText.Text(Models.Entities.RefEntity.Parameter, "Name", v.Parameter)} {v.Value}")));
                }
            }
            if (report.HiddenLots > 0) Row(L["Lot"].Value, null, L["not shown"].Value, L["{0} lot(s) of another organization", report.HiddenLots].Value, null, null, null, null);
            foreach (var b in report.Batches)
                Row(L["Batch"].Value, b.BatchId, b.Title, b.Owner, b.UsedAmount, b.Date?.ToString("yyyy-MM-dd"), b.Finalized ? L["Finalized"].Value : L["Draft"].Value, b.Total);
            if (report.HiddenBatches > 0) Row(L["Batch"].Value, null, L["not shown"].Value, L["{0} batch(es) of another organization", report.HiddenBatches].Value, null, null, null, null);
            foreach (var s in report.Shipments)
                Row(L["Shipment"].Value, s.ShipmentId, $"{s.From} -> {s.To}", $"{L["Kind.batch"].Value} {s.BatchId}", s.Quantity, s.ShippedAt.ToString("yyyy-MM-dd HH:mm"), L[s.Status].Value, s.RejectReason);
            if (report.HiddenShipments > 0) Row(L["Shipment"].Value, null, L["not shown"].Value, L["{0} shipment(s) between other organizations", report.HiddenShipments].Value, null, null, null, null);
            foreach (var h in report.Holders)
                Row(L["Holder"].Value, h.BatchId, h.Org, null, h.Held, null, null, null);
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv; charset=utf-8", $"recall-{report.Kind}-{report.Id}.csv");
        }

        // RFC 4180 quoting; a leading = + - @ is prefixed so spreadsheets do not run it as a formula
        private static string Csv(object? value)
        {
            var s = value switch
            {
                null => "",
                decimal d => d.ToString("0.##", CultureInfo.InvariantCulture),
                IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString() ?? ""
            };
            if (s.Length > 0 && "=+-@".Contains(s[0]) && value is string)
                s = "'" + s;
            return s.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0 ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
        }
    }
}
