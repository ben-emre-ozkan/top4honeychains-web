using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;

namespace TOP4HoneyChains.Web.Services
{
    // D-72: the admin home (Dashboard/Index). Figures come from real data only; a null figure means the data does not
    // exist in this mode (the organization tables are used only with the organization model) and the page shows an empty state.
    // D-87: ConsumerAccounts is null while Features:ConsumerAccounts is off
    public record AdminKpis(int? LotsThisMonth, int? BatchesFinalized, int? ApprovedLabReports, int? ApprovalsWaiting, int? VerifiedOrganizations,
        int? ConsumerAccounts = null);

    // One step of the chain strip: Apiary, Delivery, Laboratory, Processing, Shelf; Waiting = items currently at that step
    public record AdminChainStep(string Key, int? Waiting, string Url);

    public record AdminProvinceCount(int Plate, int Beekeepers, int Apiaries);

    // D-98: apiaries per district (District: the name on the district map), for the drill-down of the map
    public record AdminDistrictCount(int Plate, string District, int Apiaries);

    public record AdminHomeFigures(AdminKpis Kpis, IReadOnlyList<AdminChainStep> Chain, IReadOnlyList<AdminProvinceCount> Provinces, int UnlocatedApiaries,
        IReadOnlyList<AdminDistrictCount>? Districts = null);

    // Kind: ccp, report, storage, finalize, shipment, intake, org, registry. First = entered / requested / sent,
    // Second = approved / finalized / received / verified; a pair of the same record is shown as one entry
    public record AdminActivity(string Kind, string Subject, string? Url, string? FirstBy, DateTime? FirstAt, string? SecondBy, DateTime? SecondAt, bool SelfApproved)
    {
        public DateTime Latest => (SecondAt ?? FirstAt)!.Value;
    }

    // Kind: ccp, report, storage, finalize, org, registry, correction
    public record AdminWaiting(string Kind, string Title, string Url, DateTime? Since);

    public class AdminHomeService
    {
        public static readonly TimeSpan CacheFor = TimeSpan.FromMinutes(5);
        private const string CacheKey = "admin-home-figures";

        private readonly TOP4HoneyChainsContext _context;
        private readonly IMemoryCache _cache;
        private readonly FeatureFlags _flags;
        private readonly IConfiguration _config;

        public AdminHomeService(TOP4HoneyChainsContext context, IMemoryCache cache, IOptionsSnapshot<FeatureFlags> flags, IConfiguration config)
        {
            _context = context;
            _config = config;
            _cache = cache;
            _flags = flags.Value;
        }

        public bool OrgModel => _flags.OrgRoleModel;

        public async Task<AdminHomeFigures> FiguresAsync() =>
            (await _cache.GetOrCreateAsync(CacheKey + OrgModel + _flags.ConsumerAccounts, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheFor;
                var monthStart = DateOnly.FromDateTime(DateTime.Today).AddDays(1 - DateTime.Today.Day);
                var kpis = new AdminKpis(
                    await _context.RawHoneyProductions.CountAsync(r => r.RawHoneyProductionDate >= monthStart),
                    await _context.FinalHoneyProductions.CountAsync(f => f.FinalHoneyProductionIsFinalized),
                    OrgModel ? await _context.LabReports.CountAsync(r => r.Status == LabReportStatus.Approved) : null,
                    OrgModel ? (await WaitingAsync()).Count : null,
                    OrgModel ? await _context.Organizations.CountAsync(o => o.Status == OrgStatus.Verified) : null,
                    _flags.ConsumerAccounts ? await _context.ConsumerAccounts.CountAsync() : null);
                var (provinces, unlocated, districts) = await ProvincesAsync();
                return new AdminHomeFigures(kpis, await ChainAsync(), provinces, unlocated, districts);
            }))!;

        // ------------------------------------------------------------------ chain strip

        private async Task<List<AdminChainStep>> ChainAsync()
        {
            var lots = _context.RawHoneyProductions.AsNoTracking();
            var notBatched = lots.Where(r => !_context.FinalHoneyProductionDetails.Any(d => d.RawHoneyProductionId == r.RawHoneyProductionId));
            var openBatches = await _context.FinalHoneyProductions.CountAsync(f => !f.FinalHoneyProductionIsFinalized);
            if (!OrgModel)
                return new()
                {
                    new("apiary", await notBatched.CountAsync(), "/RawHoneyProductions/Index"),
                    new("delivery", null, "/RawHoneyProductions/Index"),
                    new("lab", null, "/RawHoneyProductions/Index"),
                    new("processing", openBatches, "/FinalHoneyProductions/Index"),
                    new("shelf", null, "/FinalHoneyProductions/Index")
                };

            // waiting at the apiary: not delivered and not in a batch; at the delivery: received, CCP 0 not approved yet;
            // at the laboratory: samples without an approved report; in processing: open batches; on the way to the
            // shelf: shipments to confirm and storage values (CCP 10) to approve
            var atApiary = await notBatched.CountAsync(r => !_context.LotIntakes.Any(i => i.RawHoneyProductionId == r.RawHoneyProductionId));
            var ccp0 = await _context.CriticalControlPoints.Where(c => c.Code == "CCP 0").Select(c => (int?)c.Id).FirstOrDefaultAsync();
            var atDelivery = await lots.CountAsync(r => _context.LotIntakes.Any(i => i.RawHoneyProductionId == r.RawHoneyProductionId)
                && r.RawHoneyCCPAssignments.Any(a => a.RawHoneyCCPAssignmentCCP == ccp0
                    && a.RawHoneyCCPAssignmentStatus != AssignmentStatusIds.Completed && a.RawHoneyCCPAssignmentStatus != AssignmentStatusIds.NotApplicable));
            // D-95: jars that went into a composite sample are counted through the composite
            var atLab = await _context.Samples.CountAsync(s => s.Status != SampleStatus.Rejected && (s.Report == null || s.Report.Status != LabReportStatus.Approved)
                && !_context.SampleSources.Any(src => src.SourceSampleId == s.SampleId && _context.Samples.Any(c => c.SampleId == src.CompositeSampleId && c.Status != SampleStatus.Rejected)));
            var toShelf = await _context.Shipments.CountAsync(s => s.Status == ShipmentStatus.Pending)
                + await _context.HolderStorageRecords.CountAsync(r => r.Status == AssignmentStatusIds.Submitted);
            return new()
            {
                new("apiary", atApiary, "/RawHoneyProductions/Index"),
                new("delivery", atDelivery, "/Intake/Index"),
                new("lab", atLab, "/Samples/Index"),
                new("processing", openBatches, "/FinalHoneyProductions/Index"),
                new("shelf", toShelf, "/Shipments/Index")
            };
        }

        // ------------------------------------------------------------------ map: beekeepers and apiaries per province

        private async Task<(List<AdminProvinceCount>, int, List<AdminDistrictCount>)> ProvincesAsync()
        {
            // the apiary's province and district are computed here; its coordinates never leave the server
            var apiaries = await _context.Apiaries.AsNoTracking()
                .Select(a => new { a.ApiaryBeekeeperId, a.ApiaryLocationLatitude, a.ApiaryLocationLongitude, a.DistrictId }).ToListAsync();
            var placed = apiaries.Select(a => (a.ApiaryBeekeeperId, Plate: TurkeyMap.PlateAt((double)a.ApiaryLocationLatitude, (double)a.ApiaryLocationLongitude))).ToList();
            // D-98: the district chosen for the apiary (DistrictId) when it lies in that province, else the district of the position
            var named = (await _context.Districts.AsNoTracking().Select(d => new { d.DistrictId, d.ProvinceCode, d.Name }).ToListAsync())
                .ToDictionary(d => d.DistrictId);
            var inDistricts = apiaries.Zip(placed, (a, p) =>
            {
                if (p.Plate == 0)
                    return null;
                var chosen = a.DistrictId is int id && named.TryGetValue(id, out var d) && d.ProvinceCode == p.Plate.ToString("00", System.Globalization.CultureInfo.InvariantCulture)
                    ? TurkeyDistricts.Find(p.Plate, d.Name) : null;
                return chosen ?? TurkeyDistricts.At(p.Plate, (double)a.ApiaryLocationLatitude, (double)a.ApiaryLocationLongitude);
            }).OfType<TurkeyDistricts.District>().ToList();
            var districts = inDistricts.GroupBy(d => (d.Plate, d.Name)).Select(g => new AdminDistrictCount(g.Key.Plate, g.Key.Name, g.Count()))
                .OrderBy(d => d.Plate).ThenBy(d => d.District).ToList();
            var beekeepers = placed.Where(p => p.Plate > 0 && p.ApiaryBeekeeperId != null).Select(p => (Id: p.ApiaryBeekeeperId!, p.Plate)).ToHashSet();
            if (OrgModel)
            {
                // a beekeeper without a located apiary counts in the province of the enterprise registry
                var located = beekeepers.Select(b => b.Id).ToHashSet();
                foreach (var r in await _context.BeekeeperRegistry.AsNoTracking().Select(r => new { r.BeekeeperId, r.Province, r.RegistryNumber }).ToListAsync())
                {
                    var plate = Provinces.PlateOf(r.Province ?? Provinces.FromRegistryNumber(r.RegistryNumber));
                    if (plate > 0 && !located.Contains(r.BeekeeperId))
                        beekeepers.Add((r.BeekeeperId, plate));
                }
            }
            var plates = placed.Where(p => p.Plate > 0).Select(p => p.Plate).Concat(beekeepers.Select(b => b.Plate)).Distinct();
            var counts = plates.Select(p => new AdminProvinceCount(p, beekeepers.Count(b => b.Plate == p), placed.Count(a => a.Plate == p)))
                .OrderBy(c => c.Plate).ToList();
            return (counts, placed.Count(p => p.Plate == 0), districts);
        }

        // ------------------------------------------------------------------ waiting for approval (organization model)

        public async Task<List<AdminWaiting>> WaitingAsync()
        {
            var items = new List<AdminWaiting>();
            if (!OrgModel)
                return items;
            foreach (var a in await _context.RawHoneyCCPAssignments.AsNoTracking().Where(a => a.RawHoneyCCPAssignmentStatus == AssignmentStatusIds.Submitted)
                         .Select(a => new { a.RawHoneyProductionId, a.RawHoneyCCPAssignmentCCP, a.CCP!.Code, a.RawHoneyCCPAssignmentStartDate }).ToListAsync())
                items.Add(new("ccp", $"{a.Code} · #{a.RawHoneyProductionId}",
                    $"/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId={a.RawHoneyProductionId}&ccpId={a.RawHoneyCCPAssignmentCCP}", a.RawHoneyCCPAssignmentStartDate));
            foreach (var a in await _context.FinalHoneyCCPAssignments.AsNoTracking().Where(a => a.FinalHoneyCCPAssignmentStatus == AssignmentStatusIds.Submitted)
                         .Select(a => new { a.FinalHoneyProductionId, a.FinalHoneyCCPAssignmentCCP, a.CCP!.Code, a.FinalHoneyProduction!.FinalHoneyProductionTitle, a.FinalHoneyCCPAssignmentStartDate }).ToListAsync())
                items.Add(new("ccp", $"{a.Code} · {a.FinalHoneyProductionTitle ?? "#" + a.FinalHoneyProductionId}",
                    $"/CCPMonitoring/FinalHoneyProductionManageCCP?finalHoneyProductionId={a.FinalHoneyProductionId}&ccpId={a.FinalHoneyCCPAssignmentCCP}", a.FinalHoneyCCPAssignmentStartDate));
            foreach (var r in await _context.LabReports.AsNoTracking().Where(r => r.Status == LabReportStatus.Submitted)
                         .Select(r => new { r.ReportCode, r.SampleId, r.SubmittedAt }).ToListAsync())
                items.Add(new("report", r.ReportCode, $"/LabReports/Edit/{r.SampleId}", r.SubmittedAt));
            foreach (var r in await _context.HolderStorageRecords.AsNoTracking().Where(r => r.Status == AssignmentStatusIds.Submitted)
                         .Select(r => new { r.FinalHoneyProductionId, r.Batch!.FinalHoneyProductionTitle, r.RecordedAt }).ToListAsync())
                items.Add(new("storage", r.FinalHoneyProductionTitle ?? $"#{r.FinalHoneyProductionId}", $"/Shipments/Storage?batchId={r.FinalHoneyProductionId}", r.RecordedAt));
            foreach (var b in await _context.FinalHoneyProductions.AsNoTracking().Where(f => !f.FinalHoneyProductionIsFinalized && f.FinalizeRequestedByUserId != null)
                         .Select(f => new { f.FinalHoneyProductionId, f.FinalHoneyProductionTitle, f.FinalizeRequestedAt }).ToListAsync())
                items.Add(new("finalize", b.FinalHoneyProductionTitle ?? $"#{b.FinalHoneyProductionId}", $"/OrgWorkspace/Batch/{b.FinalHoneyProductionId}", b.FinalizeRequestedAt));
            // C6 / O-16: with the central-association setting, associations wait for the central association, not for the administrators
            var central = TOP4HoneyChains.Web.Services.OrgModel.OrganizationAuthorizationHandler.CentralSetting(_config);
            foreach (var o in await _context.Organizations.AsNoTracking().Where(o => !o.IsPlatform && o.Status == OrgStatus.Pending && o.Documents.Any()
                             && (!central || !o.Capabilities.Any(c => c.Capability == Capabilities.Association) || o.Capabilities.Any(c => c.Capability == Capabilities.CentralAssociation)))
                         .Select(o => new { o.OrgId, o.DisplayName, o.CreatedAt }).ToListAsync())
                items.Add(new("org", o.DisplayName, $"/Organizations/Details/{o.OrgId}", o.CreatedAt));
            foreach (var r in await _context.RegistryVerifications.AsNoTracking().Where(v => v.Status == RegistryStatus.Pending)
                         .Select(v => new { v.BeekeeperId, v.EnterpriseNumber, v.RequestedAt }).ToListAsync())
                items.Add(new("registry", r.EnterpriseNumber, $"/Registry/Edit/{r.BeekeeperId}", r.RequestedAt));
            foreach (var c in await _context.CcpCorrections.AsNoTracking().Where(c => c.Status == CorrectionStatus.Pending)
                         .Select(c => new { c.Ccp!.Code, c.OwnerId, c.RequestedAt }).ToListAsync())
                items.Add(new("correction", $"{c.Code} · #{c.OwnerId}", "/CCPMonitoring/Corrections", c.RequestedAt));
            // D-78 P3: delivery corrections wait for a second administrator too
            foreach (var c in await _context.DeliveryCorrections.AsNoTracking().Where(c => c.Status == CorrectionStatus.Pending)
                         .Select(c => new { c.IntakeId, c.RawHoneyProductionId, c.RequestedAt }).ToListAsync())
                items.Add(new("correction", $"#{c.RawHoneyProductionId}", $"/Intake/Details/{c.IntakeId}", c.RequestedAt));
            return items.OrderBy(i => i.Since ?? DateTime.MaxValue).ToList();
        }

        // ------------------------------------------------------------------ recent activity from the audit log (D-28)

        private static readonly string[] Paired = { "RawHoneyCCPAssignments", "FinalHoneyCCPAssignments", "LabReports", "HolderStorageRecords", "FinalHoneyProductions", "Shipments" };

        public async Task<List<AdminActivity>> ActivityAsync(int take = 8)
        {
            // the newest events that mean a step of the chain: entered / submitted, approved, finalized, delivered, verified
            var rows = await _context.AuditEntries.AsNoTracking()
                .Where(e => e.Action == "Approve" || e.Action == "Finalize" || e.Action == "Verify"
                    || (e.Action == "Create" && (e.Entity == "LotIntakes" || e.Entity == "Shipments"))
                    || (e.Action == "Update" && Paired.Contains(e.Entity) && e.NewValues != null
                        && (e.NewValues.Contains("Status\":6") || e.NewValues.Contains("\"Status\":\"Submitted\"") || e.NewValues.Contains("\"Status\":\"Received\"")
                            || e.NewValues.Contains("\"FinalizeRequestedByUserId\":\""))))
                .OrderByDescending(e => e.AuditId).Take(400)
                .Select(e => new { e.At, e.UserName, e.Action, e.Entity, e.EntityKey, e.NewValues, e.Flags })
                .ToListAsync();
            rows.Reverse();

            // keys of assignment updates are assignment ids; approvals use "owner/ccp"
            int Id(string? key) => int.TryParse(key, out var v) ? v : 0;
            var rawIds = rows.Where(r => r.Entity == "RawHoneyCCPAssignments" && r.Action == "Update").Select(r => Id(r.EntityKey)).ToList();
            var finalIds = rows.Where(r => r.Entity == "FinalHoneyCCPAssignments" && r.Action == "Update").Select(r => Id(r.EntityKey)).ToList();
            var rawMap = await _context.RawHoneyCCPAssignments.AsNoTracking().Where(a => rawIds.Contains(a.RawHoneyCCPAssignmentId))
                .ToDictionaryAsync(a => a.RawHoneyCCPAssignmentId, a => $"{a.RawHoneyProductionId}/{a.RawHoneyCCPAssignmentCCP}");
            var finalMap = await _context.FinalHoneyCCPAssignments.AsNoTracking().Where(a => finalIds.Contains(a.FinalHoneyCCPAssignmentId))
                .ToDictionaryAsync(a => a.FinalHoneyCCPAssignmentId, a => $"{a.FinalHoneyProductionId}/{a.FinalHoneyCCPAssignmentCCP}");

            // (kind, record) of each event and whether it is the first or the second half of a pair
            var events = new List<(string Kind, string Ref, bool Second, string? By, DateTime At, bool Self)>();
            foreach (var r in rows)
            {
                var self = r.Flags?.Contains("SelfApproved") == true;
                var at = DateTime.SpecifyKind(r.At, DateTimeKind.Utc).ToLocalTime();
                (string Kind, string Ref, bool Second)? e = (r.Action, r.Entity) switch
                {
                    ("Update", "RawHoneyCCPAssignments") => rawMap.TryGetValue(Id(r.EntityKey), out var k) ? ("ccp", "raw:" + k, false) : null,
                    ("Approve", "RawHoneyCCPAssignments") => ("ccp", "raw:" + r.EntityKey, true),
                    ("Update", "FinalHoneyCCPAssignments") => finalMap.TryGetValue(Id(r.EntityKey), out var k) ? ("ccp", "batch:" + k, false) : null,
                    ("Approve", "FinalHoneyCCPAssignments") => ("ccp", "batch:" + r.EntityKey, true),
                    ("Update", "LabReports") when r.NewValues!.Contains("Submitted") => ("report", r.EntityKey!, false),
                    ("Approve", "LabReports") => ("report", r.EntityKey!, true),
                    ("Update", "HolderStorageRecords") when r.NewValues!.Contains("Status\":6") => ("storage", r.EntityKey!, false),
                    ("Approve", "HolderStorageRecords") => ("storage", r.EntityKey!, true),
                    ("Update", "FinalHoneyProductions") => ("finalize", r.EntityKey!, false),
                    ("Finalize", "FinalHoneyProductions") => ("finalize", r.EntityKey!, true),
                    ("Create", "Shipments") => ("shipment", r.EntityKey!, false),
                    ("Update", "Shipments") when r.NewValues!.Contains("Received") => ("shipment", r.EntityKey!, true),
                    ("Create", "LotIntakes") => ("intake", r.EntityKey!, true),
                    ("Verify", "Organizations") => ("org", r.EntityKey!, true),
                    ("Verify", "RegistryVerifications") when r.NewValues?.Contains("\"Approved\":true") == true => ("registry", r.EntityKey!, true),
                    _ => null
                };
                if (e is { } x && x.Ref != null)
                    events.Add((x.Kind, x.Ref, x.Second, r.UserName, at, self));
            }

            // pair: the latest first half before the second one; an approval without its entry (older than the window) stands alone
            var pairs = new List<(string Kind, string Ref, string? FirstBy, DateTime? FirstAt, string? SecondBy, DateTime? SecondAt, bool Self)>();
            var open = new Dictionary<string, int>();
            foreach (var e in events)
            {
                var key = e.Kind + "|" + e.Ref;
                if (!e.Second)
                {
                    pairs.Add((e.Kind, e.Ref, e.By, e.At, null, null, false));
                    if (open.TryGetValue(key, out var old) && pairs[old].SecondAt == null)
                        pairs[old] = default;   // superseded by a later entry
                    open[key] = pairs.Count - 1;
                }
                else if (open.TryGetValue(key, out var i) && pairs[i].SecondAt == null && pairs[i].Kind != null)
                {
                    pairs[i] = pairs[i] with { SecondBy = e.By, SecondAt = e.At, Self = e.Self };
                    open.Remove(key);
                }
                else
                    pairs.Add((e.Kind, e.Ref, null, null, e.By, e.At, e.Self));
            }
            var latest = pairs.Where(p => p.Kind != null).OrderByDescending(p => p.SecondAt ?? p.FirstAt).Take(take).ToList();
            return await DescribeAsync(latest);
        }

        // subjects and links of the records, looked up in one query per kind
        private async Task<List<AdminActivity>> DescribeAsync(List<(string Kind, string Ref, string? FirstBy, DateTime? FirstAt, string? SecondBy, DateTime? SecondAt, bool Self)> items)
        {
            static int N(string s) => int.TryParse(s, out var v) ? v : 0;
            var codes = await _context.CriticalControlPoints.AsNoTracking().ToDictionaryAsync(c => c.Id, c => c.Code);
            var batchIds = items.Where(i => i.Ref.StartsWith("batch:")).Select(i => N(i.Ref[6..].Split('/')[0]))
                .Concat(items.Where(i => i.Kind == "finalize").Select(i => N(i.Ref))).ToList();
            var storageIds = items.Where(i => i.Kind == "storage").Select(i => N(i.Ref)).ToList();
            var storage = await _context.HolderStorageRecords.AsNoTracking().Where(r => storageIds.Contains(r.RecordId)).ToDictionaryAsync(r => r.RecordId, r => r.FinalHoneyProductionId);
            var shipmentIds = items.Where(i => i.Kind == "shipment").Select(i => N(i.Ref)).ToList();
            var shipments = await _context.Shipments.AsNoTracking().Where(s => shipmentIds.Contains(s.ShipmentId)).ToDictionaryAsync(s => s.ShipmentId, s => s.FinalHoneyProductionId);
            batchIds.AddRange(storage.Values);
            batchIds.AddRange(shipments.Values);
            var batches = await _context.FinalHoneyProductions.AsNoTracking().Where(f => batchIds.Contains(f.FinalHoneyProductionId))
                .ToDictionaryAsync(f => f.FinalHoneyProductionId, f => f.FinalHoneyProductionTitle ?? "#" + f.FinalHoneyProductionId);
            var reportIds = items.Where(i => i.Kind == "report").Select(i => N(i.Ref)).ToList();
            var reports = await _context.LabReports.AsNoTracking().Where(r => reportIds.Contains(r.ReportId)).ToDictionaryAsync(r => r.ReportId, r => new { r.ReportCode, r.SampleId });
            var intakeIds = items.Where(i => i.Kind == "intake").Select(i => N(i.Ref)).ToList();
            var intakes = await _context.LotIntakes.AsNoTracking().Where(i => intakeIds.Contains(i.IntakeId)).ToDictionaryAsync(i => i.IntakeId, i => i.RawHoneyProductionId);
            var orgIds = items.Where(i => i.Kind == "org").Select(i => N(i.Ref)).ToList();
            var orgs = await _context.Organizations.AsNoTracking().Where(o => orgIds.Contains(o.OrgId)).ToDictionaryAsync(o => o.OrgId, o => o.DisplayName);
            var registryIds = items.Where(i => i.Kind == "registry").Select(i => N(i.Ref)).ToList();
            var registry = await _context.RegistryVerifications.AsNoTracking().Where(v => registryIds.Contains(v.Id)).ToDictionaryAsync(v => v.Id, v => new { v.BeekeeperId, v.EnterpriseNumber });
            string Batch(int id) => batches.GetValueOrDefault(id) ?? "#" + id;
            string BatchUrl(int id) => OrgModel ? $"/OrgWorkspace/Batch/{id}" : $"/FinalHoneyProductions/CCPOverview/{id}";

            var result = new List<AdminActivity>();
            foreach (var i in items)
            {
                string subject;
                string? url;
                switch (i.Kind)
                {
                    case "ccp":
                        var raw = i.Ref.StartsWith("raw:");
                        var parts = i.Ref[(raw ? 4 : 6)..].Split('/');
                        var (owner, ccp) = (N(parts[0]), parts.Length > 1 ? N(parts[1]) : 0);
                        var code = codes.GetValueOrDefault(ccp) ?? "CCP";
                        subject = raw ? $"{code} · #{owner}" : $"{code} · {Batch(owner)}";
                        url = raw ? $"/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId={owner}&ccpId={ccp}"
                                  : $"/CCPMonitoring/FinalHoneyProductionManageCCP?finalHoneyProductionId={owner}&ccpId={ccp}";
                        break;
                    case "report":
                        var report = reports.GetValueOrDefault(N(i.Ref));
                        subject = report?.ReportCode ?? "#" + i.Ref;
                        url = report != null && OrgModel ? $"/LabReports/Edit/{report.SampleId}" : null;
                        break;
                    case "storage":
                        var held = storage.TryGetValue(N(i.Ref), out var sb) ? sb : 0;
                        subject = $"CCP 10 · {Batch(held)}";
                        url = held > 0 ? $"/Shipments/Storage?batchId={held}" : null;
                        break;
                    case "finalize":
                        subject = Batch(N(i.Ref));
                        url = BatchUrl(N(i.Ref));
                        break;
                    case "shipment":
                        var shipped = shipments.TryGetValue(N(i.Ref), out var b) ? b : 0;
                        subject = Batch(shipped);
                        url = "/Shipments/Index";
                        break;
                    case "intake":
                        var lot = intakes.GetValueOrDefault(N(i.Ref));
                        subject = $"#{lot}";
                        url = OrgModel ? $"/Recall/Lot/{lot}" : null;
                        break;
                    case "org":
                        subject = orgs.GetValueOrDefault(N(i.Ref)) ?? "#" + i.Ref;
                        url = $"/Organizations/Details/{i.Ref}";
                        break;
                    default:
                        var rv = registry.GetValueOrDefault(N(i.Ref));
                        subject = rv?.EnterpriseNumber ?? "#" + i.Ref;
                        url = rv != null ? $"/Registry/Edit/{rv.BeekeeperId}" : null;
                        break;
                }
                result.Add(new AdminActivity(i.Kind, subject, url, i.FirstBy, i.FirstAt, i.SecondBy, i.SecondAt, i.Self));
            }
            return result;
        }
    }
}
