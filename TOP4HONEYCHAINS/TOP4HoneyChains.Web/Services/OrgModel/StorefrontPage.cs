using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.ViewModels.Storefront;

namespace TOP4HoneyChains.Web.Services.OrgModel
{
    // Builds the public beekeeper page (D-30, D-43; design docs/design/beekeeper-page) from verified data only:
    // the beekeeper's lots in finalized batches, approved reports of verified laboratories, approved CCPs and
    // verified organizations. The caller has already checked that the storefront is published.
    public class StorefrontPage
    {
        // The storefront created by db/demo/create_demo.sql. Real codes never contain "0" (StorefrontService.NewCode),
        // and the association name must carry the demo marker too, so sample content cannot reach a real page.
        public const string DemoCode = "BK-DEMQ2026";
        public const string DemoMarker = "[DEMO]";
        public const string DemoImages = "/img/storefront-demo/";

        private readonly TOP4HoneyChainsContext _context;
        private readonly CcpIntegrityService _ccp;

        public StorefrontPage(TOP4HoneyChainsContext context, CcpIntegrityService ccp)
        {
            _context = context;
            _ccp = ccp;
        }

        public static bool IsDemo(string? code, string? associationName) =>
            code == DemoCode && associationName != null && associationName.StartsWith(DemoMarker, StringComparison.Ordinal);

        // Share of the beekeeper's honey in a batch, in whole percent (0..100)
        public static int SharePercent(decimal beekeeperKg, decimal batchKg)
        {
            if (batchKg <= 0 || beekeeperKg <= 0)
                return 0;
            var percent = Math.Round(beekeeperKg * 100m / batchKg, 0, MidpointRounding.AwayFromZero);
            return (int)Math.Clamp(percent, 0m, 100m);
        }

        // Bar of a measured value against its limit: a maximum sits at 80 % of the bar, a minimum alone at 40 %
        public static (double Value, double? Limit, double? Min)? LimitBar(decimal value, decimal? min, decimal? max)
        {
            decimal scale;
            if (max is > 0)
                scale = max.Value / 0.8m;
            else if (min is > 0)
                scale = min.Value / 0.4m;
            else
                return null;
            static double Pct(decimal v, decimal scale) => Math.Round((double)Math.Clamp(v / scale * 100m, 0m, 100m), 1);
            var bar = Math.Max(Pct(value, scale), 1.5);
            return max is > 0
                ? (bar, Pct(max.Value, scale), min is > 0 ? Pct(min.Value, scale) : null)
                : (bar, Pct(min!.Value, scale), null);
        }

        // Two letters for the profile mark: initials of the shown name without the demo marker, else of the code
        public static string Initials(string displayName)
        {
            var name = displayName.Replace(DemoMarker, "").Trim();
            var words = name.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => char.IsLetter(w[0])).ToList();
            if (words.Count == 0)
                return "BK";
            var tr = System.Globalization.CultureInfo.GetCultureInfo("tr-TR");
            return words.Count == 1 ? words[0][..Math.Min(2, words[0].Length)].ToUpper(tr)
                : (words[0][..1] + words[^1][..1]).ToUpper(tr);
        }

        public async Task<StorefrontPageViewModel> BuildAsync(StorefrontService.State state, Beekeeper beekeeper)
        {
            var bkId = beekeeper.BeekeeperId!;
            var registry = await _context.BeekeeperRegistry.AsNoTracking().FirstOrDefaultAsync(r => r.BeekeeperId == bkId);
            var displayName = StorefrontService.DisplayName(state.Consent!.NameConsented, beekeeper.BeekeeperFirstName, beekeeper.BeekeeperLastName, state.Code!);

            // The beekeeper's lots in finalized batches (verified lots)
            var lots = await _context.FinalHoneyProductionDetails.AsNoTracking()
                .Where(d => d.FinalHoneyProduction.FinalHoneyProductionIsFinalized
                            && d.RawHoneyProduction.RawHoneyProductionApiary!.ApiaryBeekeeperId == bkId)
                .Select(d => new
                {
                    d.RawHoneyProductionId,
                    d.FinalHoneyProductionId,
                    d.UsedAmount,
                    HoneyTypeId = d.RawHoneyProduction.RawHoneyProductionHoneyType,
                    HoneyType = d.RawHoneyProduction.RawHoneyProductionHoneyTypeNavigation!.HoneyTypeTitle,
                    HoneyClass = d.RawHoneyProduction.RawHoneyProductionHoneyTypeNavigation!.HoneyClass,
                    ProducedOn = d.RawHoneyProduction.RawHoneyProductionDate
                })
                .ToListAsync();
            var lotIds = lots.Select(l => l.RawHoneyProductionId).Distinct().ToList();
            var batchIds = lots.Select(l => l.FinalHoneyProductionId).Distinct().ToList();

            var batches = await _context.FinalHoneyProductions.AsNoTracking()
                .Where(b => batchIds.Contains(b.FinalHoneyProductionId))
                .Select(b => new
                {
                    b.FinalHoneyProductionId,
                    b.FinalHoneyProductionQR,
                    b.FinalHoneyProductionTitle,
                    Date = b.FinalHoneyProductionFinalizedDate ?? b.FinalHoneyProductionDate,
                    b.FinalHoneyProductionTotalAmount,
                    b.OwnerOrgId,
                    Wholesaler = b.FinalHoneyProductionWholesaler != null ? b.FinalHoneyProductionWholesaler.WholesalerTitle : null
                })
                .ToListAsync();
            var batchLots = await _context.FinalHoneyProductionDetails.AsNoTracking()
                .Where(d => batchIds.Contains(d.FinalHoneyProductionId))
                .Select(d => new { d.FinalHoneyProductionId, d.RawHoneyProductionId, d.UsedAmount, d.RawHoneyProduction.OriginCountry })
                .ToListAsync();
            var allLotIds = batchLots.Select(d => d.RawHoneyProductionId).Distinct().ToList();

            // Approved reports of verified laboratories, on the batches' lots (the beekeeper's and the others)
            var reports = await _context.LabReports.AsNoTracking()
                .Where(r => r.Status == LabReportStatus.Approved && allLotIds.Contains(r.Sample!.RawHoneyProductionId) && r.LabOrg!.Status == OrgStatus.Verified)
                .Select(r => new { r.ReportId, r.ReportCode, r.Sample!.RawHoneyProductionId, r.IsAccredited, r.LabOrgId, Lab = r.LabOrg!.DisplayName, r.ApprovedAt })
                .ToListAsync();
            var ownReports = reports.Where(r => lotIds.Contains(r.RawHoneyProductionId)).ToList();

            // CCPs of the beekeeper's lots
            var done = await _ccp.DonePredicateAsync();
            var lotCcps = await _context.RawHoneyCCPAssignments.AsNoTracking()
                .Where(a => lotIds.Contains(a.RawHoneyProductionId))
                .Select(a => new { a.RawHoneyProductionId, a.RawHoneyCCPAssignmentCCP, a.RawHoneyCCPAssignmentStatus, a.ApprovedAt, Code = a.CCP!.Code })
                .ToListAsync();
            bool Passed(int status, DateTime? approvedAt, int ccpId) => status == AssignmentStatusIds.Completed && done(status, approvedAt, ccpId);
            var ccp0Passed = lotCcps.Where(a => a.Code == "CCP 0" && Passed(a.RawHoneyCCPAssignmentStatus, a.ApprovedAt, a.RawHoneyCCPAssignmentCCP))
                .Select(a => a.RawHoneyProductionId).ToHashSet();

            var vm = new StorefrontPageViewModel
            {
                Code = state.Code!,
                DisplayName = displayName,
                Initials = state.Consent.NameConsented ? Initials(displayName) : "BK",
                IsDemo = IsDemo(state.Code, state.AssociationName),
                // PQ-12 is open: the registry's own fields if set, else the province of the verified registry number
                // and the district recorded most often at intake of the verified lots
                Province = registry?.Province ?? Provinces.FromRegistryNumber(registry?.RegistryNumber),
                District = registry?.District ?? await _context.LotIntakes.AsNoTracking().Where(i => lotIds.Contains(i.RawHoneyProductionId))
                    .GroupBy(i => i.District).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefaultAsync(),
                Association = state.AssociationName,
                MemberSinceYear = state.AssociationOrgId == null ? null : await _context.BeekeeperAssociations.AsNoTracking()
                    .Where(a => a.BeekeeperId == bkId && a.AssociationOrgId == state.AssociationOrgId)
                    .OrderBy(a => a.ValidFrom).Select(a => (int?)a.ValidFrom.Year).FirstOrDefaultAsync(),
                HoneyTypes = lots.Select(l => l.HoneyType).Where(t => t != null).Distinct().OrderBy(t => t).ToList()!,
                RegistryVerified = registry?.Status == RegistryStatus.Verified,
                AccreditedLab = ownReports.Any(r => r.IsAccredited),
                AllCcpsApproved = lotIds.Count > 0 && lotIds.All(id => lotCcps.Any(a => a.RawHoneyProductionId == id))
                                  && lotCcps.All(a => done(a.RawHoneyCCPAssignmentStatus, a.ApprovedAt, a.RawHoneyCCPAssignmentCCP)),
                VerifiedLots = lotIds.Count,
                BatchCount = batchIds.Count,
                LabReportCount = ownReports.Select(r => r.ReportId).Distinct().Count(),
                KgTraced = lots.Sum(l => l.UsedAmount),
                Ccp0PassedForAllLots = lotIds.Count > 0 && lotIds.All(ccp0Passed.Contains)
            };
            var province = ProvinceMap.ByName(vm.Province);
            vm.Region = province?.Region;

            // Recent activity
            var latestBatch = batches.OrderByDescending(b => b.Date).FirstOrDefault();
            if (latestBatch != null)
                vm.LatestBatch = new ActivityBatch(latestBatch.FinalHoneyProductionTitle ?? "", latestBatch.Date);
            var latestCcp0Lot = lots.Where(l => ccp0Passed.Contains(l.RawHoneyProductionId) && l.ProducedOn != null)
                .OrderByDescending(l => l.ProducedOn).FirstOrDefault();
            vm.Ccp0PassedSeason = latestCcp0Lot?.ProducedOn?.Year;

            var months = lots.Where(l => l.ProducedOn != null).Select(l => l.ProducedOn!.Value.Month).Distinct().OrderBy(m => m).ToList();
            if (months.Count > 0)
            {
                vm.HarvestFrom = months.First();
                vm.HarvestTo = months.Last();
            }

            // Map: the province centre, never the apiary
            if (province != null)
            {
                var (x, y) = ProvinceMap.ToMap(province.Latitude, province.Longitude);
                vm.Marker = new MapMarker(x, y, vm.Place);
            }

            // Calendar: harvest months of the lots per honey type; the demo shows sample flowering rows
            vm.Calendar = vm.IsDemo
                ? DemoCalendar()
                : lots.Where(l => l.HoneyType != null && l.ProducedOn != null).GroupBy(l => l.HoneyType!).OrderBy(g => g.Key)
                    .Select(g => new CalendarRow(g.Key, false, new HashSet<int>(), g.Select(l => l.ProducedOn!.Value.Month).ToHashSet())).ToList();

            // Latest approved report with its values against the limits
            var latest = ownReports.OrderByDescending(r => r.ApprovedAt).FirstOrDefault();
            if (latest != null)
            {
                var results = await _context.LabResults.AsNoTracking()
                    .Where(r => r.ReportId == latest.ReportId && r.MeasuredNumericValue != null)
                    .Include(r => r.Parameter!).ThenInclude(p => p.Limits)
                    .ToListAsync();
                var lot = lots.First(l => l.RawHoneyProductionId == latest.RawHoneyProductionId);
                var honey = new HoneyProfile(lot.HoneyTypeId, lot.HoneyClass);
                var siblings = results.ToDictionary(r => r.ParameterId, r => r.MeasuredNumericValue!.Value);
                var values = new List<LabValue>();
                foreach (var r in results.Where(r => r.IsInRange != null).OrderBy(r => r.Parameter!.ParameterId))
                {
                    var bounds = CcpLimitEvaluator.MainBounds(r.Parameter!, siblings, honey);
                    var bar = bounds == null ? null : LimitBar(r.MeasuredNumericValue!.Value, bounds.Value.Min, bounds.Value.Max);
                    values.Add(new LabValue(r.ParameterId, r.Parameter!.ParameterName ?? "", r.Parameter.ParameterUnit, r.MeasuredNumericValue!.Value, r.IsInRange,
                        bounds?.Min, bounds?.Max, bar?.Value ?? 0, bar?.Limit, bar?.Min));
                }
                vm.LatestReport = new ActivityReport(latest.ReportCode, values.Count(v => v.InRange == true), values.Count);
                if (values.Count > 0)
                    vm.Lab = new LabSection(latest.ReportCode, latest.Lab, latest.IsAccredited, values);
            }

            // Batches with the beekeeper's honey
            var orgIds = batches.Where(b => b.OwnerOrgId != null).Select(b => b.OwnerOrgId!.Value).ToList();
            var shipments = await _context.Shipments.AsNoTracking()
                .Where(s => batchIds.Contains(s.FinalHoneyProductionId) && s.Status == ShipmentStatus.Received)
                .Select(s => new { s.FinalHoneyProductionId, s.ToOrgId }).ToListAsync();
            orgIds.AddRange(shipments.Select(s => s.ToOrgId));
            var orgs = await _context.Organizations.AsNoTracking()
                .Where(o => orgIds.Contains(o.OrgId) && o.Status == OrgStatus.Verified)
                .Select(o => new { o.OrgId, o.DisplayName, Capabilities = o.Capabilities.Select(c => c.Capability).ToList() })
                .ToDictionaryAsync(o => o.OrgId);
            var stored = await _context.BatchOriginShares.AsNoTracking().Where(s => batchIds.Contains(s.FinalHoneyProductionId)).ToListAsync();
            foreach (var b in batches.OrderByDescending(b => b.Date))
            {
                var own = lots.Where(l => l.FinalHoneyProductionId == b.FinalHoneyProductionId).Sum(l => l.UsedAmount);
                var parts = batchLots.Where(d => d.FinalHoneyProductionId == b.FinalHoneyProductionId).ToList();
                var total = b.FinalHoneyProductionTotalAmount is > 0 ? b.FinalHoneyProductionTotalAmount.Value : parts.Sum(p => p.UsedAmount);
                var shares = stored.Where(s => s.FinalHoneyProductionId == b.FinalHoneyProductionId)
                    .Select(s => new Origin.Share(s.CountryCode, s.Amount, s.Percentage)).ToList();
                if (shares.Count == 0)
                    shares = Origin.Compute(parts.Select(p => (p.OriginCountry, p.UsedAmount)));
                var partLots = parts.Select(p => p.RawHoneyProductionId).Distinct().ToList();
                vm.Batches.Add(new BatchRow(
                    b.FinalHoneyProductionQR,
                    b.FinalHoneyProductionTitle ?? "",
                    b.Date,
                    b.OwnerOrgId != null && orgs.TryGetValue(b.OwnerOrgId.Value, out var owner) ? owner.DisplayName : b.Wholesaler,
                    shares.OrderByDescending(s => s.Percentage).Select(s => new OriginShare(Origin.Name(s.Country), s.Percentage)).ToList(),
                    SharePercent(own, total),
                    (await _ccp.GetFinalizeBlockersAsync(b.FinalHoneyProductionId)).Count == 0,
                    partLots.Count > 0 && partLots.All(id => reports.Any(r => r.RawHoneyProductionId == id && r.IsAccredited))));
            }

            // References: recorded relationships with verified organizations
            if (state.AssociationName != null)
                vm.References.Add(new Reference(ReferenceKinds.Association, state.AssociationName, 0, 0, vm.MemberSinceYear));
            foreach (var g in batches.Where(b => b.OwnerOrgId != null && orgs.ContainsKey(b.OwnerOrgId.Value)).GroupBy(b => b.OwnerOrgId!.Value))
            {
                var caps = orgs[g.Key].Capabilities;
                var kind = caps.Contains(Capabilities.Processing) && caps.Contains(Capabilities.Packing) ? ReferenceKinds.ProcessorAndPacker
                    : caps.Contains(Capabilities.Packing) ? ReferenceKinds.Packer : ReferenceKinds.Processor;
                var ids = g.Select(b => b.FinalHoneyProductionId).ToHashSet();
                vm.References.Add(new Reference(kind, orgs[g.Key].DisplayName, lots.Where(l => ids.Contains(l.FinalHoneyProductionId)).Select(l => l.RawHoneyProductionId).Distinct().Count(), ids.Count, null));
            }
            foreach (var g in ownReports.GroupBy(r => r.LabOrgId))
                vm.References.Add(new Reference(g.Any(r => r.IsAccredited) ? ReferenceKinds.AccreditedLaboratory : ReferenceKinds.Laboratory,
                    g.First().Lab, g.Select(r => r.ReportId).Distinct().Count(), 0, null));
            foreach (var g in shipments.Where(s => orgs.ContainsKey(s.ToOrgId)).GroupBy(s => s.ToOrgId))
            {
                var caps = orgs[g.Key].Capabilities;
                if (!caps.Contains(Capabilities.Retail) && !caps.Contains(Capabilities.Wholesale))
                    continue;
                vm.References.Add(new Reference(caps.Contains(Capabilities.Retail) ? ReferenceKinds.Retailer : ReferenceKinds.Wholesaler,
                    orgs[g.Key].DisplayName, 0, g.Select(s => s.FinalHoneyProductionId).Distinct().Count(), null));
            }

            // Photos: only the demo has sample photos; real pages show none until the association can add photos with consent
            if (vm.IsDemo)
                vm.Photos = new List<Photo>
                {
                    new(DemoImages + "apiary.jpg", "Sf.Demo.PhotoApiary", "Sf.Demo.PhotoApiaryAlt", "sf-pos-apiary", true),
                    new(DemoImages + "harvest.jpg", "Sf.Demo.PhotoHarvest", "Sf.Demo.PhotoHarvestAlt", "sf-pos-harvest", false),
                    new(DemoImages + "comb.jpg", "Sf.Demo.PhotoComb", "Sf.Demo.PhotoCombAlt", "sf-pos-comb", false)
                };
            return vm;
        }

        private static List<CalendarRow> DemoCalendar()
        {
            static CalendarRow Row(string key, int[] flowering, int[] harvest) => new(key, true, flowering.ToHashSet(), harvest.ToHashSet());
            return new()
            {
                Row("Sf.Demo.FloraChestnut", new[] { 6, 7 }, new[] { 8 }),
                Row("Sf.Demo.FloraLinden", new[] { 6 }, new[] { 7 }),
                Row("Sf.Demo.FloraWildflower", new[] { 4, 5, 6 }, new[] { 7 }),
                Row("Sf.Demo.FloraHeather", new[] { 8, 9 }, new[] { 10 })
            };
        }
    }
}
