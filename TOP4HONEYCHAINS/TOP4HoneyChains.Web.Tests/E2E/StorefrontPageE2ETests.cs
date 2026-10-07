using System.Globalization;
using System.Net;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // The redesigned beekeeper page: shown only with an active consent, never a phone number, birth date or
    // apiary location, the marker at the province centre, and the beekeeper's share in each batch
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class StorefrontPageE2ETests
    {
        private readonly E2EApp _app;

        public StorefrontPageE2ETests(E2EApp app) => _app = app;

        private const string Phone = "05329876543";
        private const decimal Latitude = 40.918273m, Longitude = 31.736451m;

        private record Seed(string Code, int Consent, Guid BatchQr, string OtherCode);

        private async Task<Seed> SeedAsync()
        {
            var association = await _app.CreateOrgAsync("SF Association", null, (Capabilities.Association, null));
            var processor = await _app.CreateOrgAsync("SF Processor", null, (Capabilities.Processing, null), (Capabilities.Packing, null));
            var lab = await _app.CreateOrgAsync("SF Laboratory", null, (Capabilities.Laboratory, "SF-ACC-1"));
            var retail = await _app.CreateOrgAsync("SF Market", null, (Capabilities.Retail, null));
            var suffix = Random.Shared.Next(1000, 9999).ToString(CultureInfo.InvariantCulture);
            return await _app.WithContextAsync(async db =>
            {
                var ccp0 = await db.CriticalControlPoints.Where(c => c.Code == "CCP 0").Select(c => c.Id).FirstAsync();
                var parameter = await db.CriticalControlPointParameters.Where(p => p.CCP!.Code == "CCP 1" && p.MaxValue != null && p.MaxValue > 0 && !p.Limits.Any()).FirstAsync();
                Beekeeper NewBeekeeper(string first) => new()
                {
                    BeekeeperId = Guid.NewGuid().ToString(), BeekeeperFirstName = first, BeekeeperLastName = "Keeper", BeekeeperBirthDate = new DateOnly(1971, 3, 14),
                    BeekeeperPhoneNumber = Phone, BeekeeperBusinessNumber = "SF-" + first + suffix, IsActive = true
                };
                var wholesaler = new Wholesaler { WholesalerTitle = "SF Wholesaler " + suffix, WholesalerIsActive = true, CreatedAt = DateTime.Now };
                db.Wholesalers.Add(wholesaler);
                var bk = NewBeekeeper("Sfbee");
                var other = NewBeekeeper("Sfother");
                db.Beekeepers.AddRange(bk, other);
                await db.SaveChangesAsync();
                var apiary = new Apiary { ApiaryBeekeeperId = bk.BeekeeperId!, ApiaryTitle = "SF Apiary", ApiaryBeehivesCount = 8, ApiaryQRCode = Guid.NewGuid(), ApiaryLocationLatitude = Latitude, ApiaryLocationLongitude = Longitude };
                var otherApiary = new Apiary { ApiaryBeekeeperId = other.BeekeeperId!, ApiaryTitle = "SF Other Apiary", ApiaryBeehivesCount = 8, ApiaryQRCode = Guid.NewGuid(), ApiaryLocationLatitude = Latitude, ApiaryLocationLongitude = Longitude };
                db.Apiaries.AddRange(apiary, otherApiary);
                foreach (var b in new[] { bk, other })
                {
                    db.BeekeeperAssociations.Add(new BeekeeperAssociation { BeekeeperId = b.BeekeeperId!, AssociationOrgId = association, ValidFrom = new DateOnly(2024, 4, 1) });
                    db.BeekeeperRegistry.Add(new BeekeeperRegistry { BeekeeperId = b.BeekeeperId!, RegistryNumber = "TR81" + Random.Shared.NextInt64(1_000_000_000, 9_999_999_999), Status = RegistryStatus.Verified, VerifiedAt = DateTime.UtcNow });
                }
                await db.SaveChangesAsync();

                RawHoneyProduction Lot(Apiary a, decimal kg) => new()
                {
                    RawHoneyProductionApiaryId = a.ApiaryId, RawHoneyProductionAmount = kg, RawHoneyProductionDate = new DateOnly(2026, 7, 10),
                    RawHoneyProductionHoneyType = db.HoneyTypes.Select(h => h.HoneyTypeId).First(),
                    RawHoneyProductionPeriodId = db.RawHoneyProductionPeriods.Select(p => p.RawHoneyProductionPeriodId).First(), OwnerOrgId = processor, OriginCountry = "TR",
                    RawHoneyProductionWholesalerId = wholesaler.WholesalerId
                };
                var own = Lot(apiary, 60);
                var foreign = Lot(otherApiary, 90);
                db.RawHoneyProductions.AddRange(own, foreign);
                await db.SaveChangesAsync();
                db.RawHoneyCCPAssignments.Add(new RawHoneyCCPAssignment
                {
                    RawHoneyProductionId = own.RawHoneyProductionId, RawHoneyCCPAssignmentCCP = ccp0, RawHoneyCCPAssignmentStatus = AssignmentStatusIds.Completed,
                    RawHoneyCCPAssignmentStartDate = DateTime.Now, ApprovedAt = DateTime.UtcNow, ApprovedByUserId = "seed", ApprovedByOrgId = association
                });
                // the lab value as written to the lot's CCP 1 when the report is approved
                db.RawHoneyCCPAssignments.Add(new RawHoneyCCPAssignment
                {
                    RawHoneyProductionId = own.RawHoneyProductionId, RawHoneyCCPAssignmentCCP = parameter.CCPId!.Value, RawHoneyCCPAssignmentStatus = AssignmentStatusIds.Completed,
                    RawHoneyCCPAssignmentStartDate = DateTime.Now, ApprovedAt = DateTime.UtcNow, ApprovedByUserId = "seed", ApprovedByOrgId = lab
                });
                db.CriticalControlPointMonitoringLogs.Add(new CriticalControlPointMonitoringLog
                {
                    CCPId = parameter.CCPId, ParameterId = parameter.ParameterId, RawHoneyProductionId = own.RawHoneyProductionId,
                    MeasuredNumericValue = parameter.MaxValue!.Value / 2, IsInRange = true, AppliedLimit = "max", MeasurementDate = DateTime.Now, RecordedByUserId = "seed"
                });

                var sample = new Sample
                {
                    SampleCode = "SF-" + suffix, RawHoneyProductionId = own.RawHoneyProductionId, TakingOrgId = processor, LabOrgId = lab, SealNumber = "SF-S" + suffix,
                    CounterSealNumber = "SF-C" + suffix, CounterLocation = "SF", RetainUntil = DateOnly.FromDateTime(DateTime.Today.AddYears(1)), TakenByUserId = "seed",
                    TakenAt = DateTime.UtcNow, Status = SampleStatus.Received, SealIntact = true
                };
                db.Samples.Add(sample);
                await db.SaveChangesAsync();
                var report = new LabReport
                {
                    SampleId = sample.SampleId, LabOrgId = lab, ReportCode = "SFREP" + suffix, EnteredByUserId = "seed", Status = LabReportStatus.Approved,
                    ApprovedByUserId = "seed2", ApprovedAt = DateTime.UtcNow, IsAccredited = true, AccreditationNumber = "SF-ACC-1", StoredFileName = "missing.pdf",
                    Sha256 = new string('0', 64), CreatedAt = DateTime.UtcNow
                };
                report.Results.Add(new LabResult { ParameterId = parameter.ParameterId, MeasuredNumericValue = parameter.MaxValue!.Value / 2, IsInRange = true, AppliedLimit = "max" });
                db.LabReports.Add(report);

                var batch = new FinalHoneyProduction
                {
                    FinalHoneyProductionTitle = "SF Batch " + suffix, FinalHoneyProductionDate = DateTime.Today, FinalHoneyProductionTotalAmount = 150,
                    FinalHoneyProductionIsFinalized = true, FinalHoneyProductionFinalizedDate = DateTime.Today, FinalHoneyProductionQR = Guid.NewGuid(), OwnerOrgId = processor,
                    FinalHoneyProductionWholesalerId = wholesaler.WholesalerId
                };
                db.FinalHoneyProductions.Add(batch);
                await db.SaveChangesAsync();
                db.FinalHoneyProductionDetails.AddRange(
                    new FinalHoneyProductionDetail { FinalHoneyProductionId = batch.FinalHoneyProductionId, RawHoneyProductionId = own.RawHoneyProductionId, UsedAmount = 60, CreatedAt = DateTime.Now },
                    new FinalHoneyProductionDetail { FinalHoneyProductionId = batch.FinalHoneyProductionId, RawHoneyProductionId = foreign.RawHoneyProductionId, UsedAmount = 90, CreatedAt = DateTime.Now });
                db.Shipments.Add(new Shipment
                {
                    FinalHoneyProductionId = batch.FinalHoneyProductionId, FromOrgId = processor, ToOrgId = retail, Quantity = 20, Status = ShipmentStatus.Received,
                    ShippedByUserId = "seed", ShippedAt = DateTime.UtcNow, DecidedByUserId = "seed", DecidedAt = DateTime.UtcNow
                });

                Consent NewConsent(string beekeeperId) => new()
                {
                    BeekeeperId = beekeeperId, AssociationOrgId = association, NameConsented = true, TextVersion = "v", TextSha256 = new string('0', 64),
                    GivenOn = DateOnly.FromDateTime(DateTime.Today), DocumentStoredFileName = "missing.pdf", DocumentOriginalFileName = "c.pdf",
                    DocumentSha256 = new string('0', 64), RecordedByUserId = "seed", RecordedAt = DateTime.UtcNow
                };
                var consent = NewConsent(bk.BeekeeperId!);
                db.Consents.Add(consent);
                var code = "BK-SF" + Letters(suffix);
                var otherCode = "BK-SG" + Letters(suffix);
                db.BeekeeperStorefronts.AddRange(
                    new BeekeeperStorefront { BeekeeperId = bk.BeekeeperId!, Code = code, CreatedAt = DateTime.UtcNow },
                    new BeekeeperStorefront { BeekeeperId = other.BeekeeperId!, Code = otherCode, CreatedAt = DateTime.UtcNow });   // no consent
                await db.SaveChangesAsync();
                return new Seed(code, consent.ConsentId, batch.FinalHoneyProductionQR, otherCode);
            });
        }

        // digits -> letters, so the test codes look like real ones (no "0")
        private static string Letters(string digits) => new(digits.Select(d => "ABCDEFGHJK"[d - '0']).ToArray());

        [Fact]
        public async Task The_page_shows_verified_data_only_and_disappears_without_consent()
        {
            var seed = await SeedAsync();
            var visitor = UserClient.Anonymous(_app);
            var html = WebUtility.HtmlDecode(await visitor.HtmlAsync($"/storefront/{seed.Code}"));

            // content from the verified data
            Assert.Contains("Sfbee Keeper", html);                         // the name was consented
            Assert.Contains("Düzce", html);                                // province of the verified registry number
            Assert.Contains($"data-qr=\"{seed.BatchQr}\"", html);
            Assert.Contains("class=\"sf-share-value\">40%", html);         // 60 of 150 kg
            Assert.Contains("SF Market", html);                            // retailer that received the batch
            Assert.Contains("SF Laboratory", html);
            Assert.Contains("id=\"sf-badge-lab\"", html);
            var centre = ProvinceMap.ByPlate(81)!;
            var (x, y) = ProvinceMap.ToMap(centre.Latitude, centre.Longitude);
            Assert.Contains($"class=\"tmap-mark-dot\" cx=\"{x.ToString(CultureInfo.InvariantCulture)}\" cy=\"{y.ToString(CultureInfo.InvariantCulture)}\"", html);
            Assert.Contains("id=\"sf-map-p81\"", html);                    // D-72: the shared map marks the province

            // never: phone number, exact location, birth date, sample content of the demo
            Assert.DoesNotContain("9876543", html);
            Assert.DoesNotContain("918273", html);
            Assert.DoesNotContain("736451", html);
            foreach (var birth in new[] { "14.03.1971", "1971-03-14", "3/14/1971", "14 Mar 1971", "14/03/1971" })
                Assert.DoesNotContain(birth, html);
            Assert.DoesNotContain("SF Apiary", html);
            Assert.DoesNotContain("sf-demo", html);
            Assert.DoesNotContain("storefront-demo", html);
            Assert.DoesNotContain("sf-gallery", html);
            Assert.DoesNotContain("tel:", html);
            Assert.DoesNotContain("googleapis", html);                    // fonts are self-hosted (D-57)
            Assert.DoesNotContain("sf-pill", html);                       // D-18: no in/out verdict for an unverified limit
            Assert.Contains("id=\"sf-limits-pending\"", html);

            // the Honey Story of the batch: same rules, beekeeper linked to the page, lab report to the verification page
            var story = WebUtility.HtmlDecode(await visitor.HtmlAsync($"/honeystory/{seed.BatchQr}"));
            foreach (var section in new[] { "hs-journey", "hs-origins", "hs-map", "hs-sources", "hs-lab", "lab-report", "process" })
                Assert.Contains($"id=\"{section}\"", story);
            // D-72: origins by province on the shared map, computed on the server; no coordinate of the apiary is sent
            var plate = TurkeyMap.PlateAt((double)Latitude, (double)Longitude);
            Assert.InRange(plate, 1, 81);
            Assert.Contains($"id=\"hs-tmap-p{plate}\"", story);
            Assert.Contains($"<b>{Provinces.ByPlate(plate)}</b>", story);
            Assert.DoesNotContain("hs-map-area", story);
            Assert.Contains("data-step", story);
            Assert.Contains($"href=\"/storefront/{seed.Code}\" class=\"storefront-link\"", story);
            Assert.Contains("/verify-report?code=SFREP", story);
            Assert.Contains("id=\"hs-limits-pending\"", story);
            Assert.DoesNotContain("hs-status", story);
            Assert.DoesNotContain("9876543", story);
            Assert.DoesNotContain("918273", story);
            Assert.DoesNotContain("736451", story);
            Assert.DoesNotContain("googleapis", story);

            // the parameter page shows the value of that lot only through the batch's Honey Story link
            var lot = await _app.WithContextAsync(db => db.FinalHoneyProductionDetails.Where(d => d.FinalHoneyProduction.FinalHoneyProductionQR == seed.BatchQr && d.UsedAmount == 60).Select(d => d.RawHoneyProductionId).FirstAsync());
            var parameterId = await _app.WithContextAsync(db => db.LabResults.Where(r => r.Report!.ReportCode.StartsWith("SFREP")).OrderByDescending(r => r.ResultId).Select(r => r.ParameterId).FirstAsync());
            Assert.DoesNotContain("in-this-jar", await visitor.HtmlAsync($"/honeystory/parameter/{parameterId}"));
            var jar = await visitor.HtmlAsync($"/honeystory/parameter/{parameterId}?qr={seed.BatchQr}&lot={lot}");
            Assert.Contains("id=\"in-this-jar\"", jar);
            Assert.DoesNotContain("hs-status", jar);                     // unverified limit: no verdict
            Assert.DoesNotContain("in-this-jar", await visitor.HtmlAsync($"/honeystory/parameter/{parameterId}?qr={Guid.NewGuid()}&lot={lot}"));

            // a storefront without consent is not shown
            Assert.Equal(HttpStatusCode.NotFound, (await visitor.GetAsync($"/storefront/{seed.OtherCode}")).StatusCode);

            // withdrawn consent hides the page and its QR code at once
            await _app.WithContextAsync(async db =>
            {
                var consent = await db.Consents.FirstAsync(c => c.ConsentId == seed.Consent);
                consent.WithdrawnAt = DateTime.UtcNow;
                consent.WithdrawnByUserId = "seed";
                consent.WithdrawReason = "test";
                return await db.SaveChangesAsync();
            });
            Assert.Equal(HttpStatusCode.NotFound, (await visitor.GetAsync($"/storefront/{seed.Code}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await visitor.GetAsync($"/storefront/{seed.Code}/qr")).StatusCode);
        }
    }
}
