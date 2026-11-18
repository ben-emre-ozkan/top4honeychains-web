using System.Net;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-47: an organization cannot read or change another organization's records by guessing ids. Organization
    // A owns a lot, intake, samples, a lab report, a finalized batch, a shipment and a storefront; organization B
    // has every capability and role and tries each new endpoint with A's ids.
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class CrossOrgAccessTests
    {
        private readonly E2EApp _app;

        public CrossOrgAccessTests(E2EApp app) => _app = app;

        private record Ids(int Lot, int Intake, int SentSample, int ReceivedSample, int Batch, int Shipment, string Beekeeper, int Consent, int OrgA, int Document, int Ccp0, int Ccp10);

        private async Task<Ids> SeedOrganizationAAsync()
        {
            var processor = await _app.CreateOrgAsync("XA Processor", null, (Capabilities.Processing, null), (Capabilities.Packing, null), (Capabilities.Wholesale, null), (Capabilities.Laboratory, "XA-ACC"));
            var association = await _app.CreateOrgAsync("XA Association", null, (Capabilities.Association, null));
            var receiver = await _app.CreateOrgAsync("XA Receiver", null, (Capabilities.Wholesale, null));
            return await _app.WithContextAsync(async db =>
            {
                var ccp0 = await db.CriticalControlPoints.Where(c => c.Code == "CCP 0").Select(c => c.Id).FirstAsync();
                var ccp10 = await db.CriticalControlPoints.Where(c => c.Code == "CCP 10").Select(c => c.Id).FirstAsync();
                var wholesaler = new Wholesaler { WholesalerTitle = "XA Wholesaler", WholesalerIsActive = true, CreatedAt = DateTime.Now };
                db.Wholesalers.Add(wholesaler);
                var bk = new Beekeeper { BeekeeperId = Guid.NewGuid().ToString(), BeekeeperFirstName = "XA", BeekeeperLastName = "Beekeeper", BeekeeperBirthDate = new DateOnly(1980, 1, 1), BeekeeperBusinessNumber = "XA-BEE-1", IsActive = true };
                db.Beekeepers.Add(bk);
                await db.SaveChangesAsync();
                var apiary = new Apiary { ApiaryBeekeeperId = bk.BeekeeperId, ApiaryTitle = "XA Apiary", ApiaryBeehivesCount = 5, ApiaryQRCode = Guid.NewGuid() };
                db.Apiaries.Add(apiary);
                db.BeekeeperAssociations.Add(new BeekeeperAssociation { BeekeeperId = bk.BeekeeperId, AssociationOrgId = association, ValidFrom = DateOnly.FromDateTime(DateTime.Today) });
                await db.SaveChangesAsync();
                var lot = new RawHoneyProduction
                {
                    RawHoneyProductionApiaryId = apiary.ApiaryId, RawHoneyProductionAmount = 50, RawHoneyProductionDate = DateOnly.FromDateTime(DateTime.Today),
                    RawHoneyProductionHoneyType = await db.HoneyTypes.Select(h => h.HoneyTypeId).FirstAsync(),
                    RawHoneyProductionPeriodId = await db.RawHoneyProductionPeriods.Select(p => p.RawHoneyProductionPeriodId).FirstAsync(), OwnerOrgId = processor,
                    RawHoneyProductionWholesalerId = wholesaler.WholesalerId
                };
                db.RawHoneyProductions.Add(lot);
                await db.SaveChangesAsync();
                db.RawHoneyCCPAssignments.Add(new RawHoneyCCPAssignment { RawHoneyProductionId = lot.RawHoneyProductionId, RawHoneyCCPAssignmentCCP = ccp0, RawHoneyCCPAssignmentStatus = AssignmentStatusIds.Assigned, RawHoneyCCPAssignmentStartDate = DateTime.Now });
                var intake = new LotIntake
                {
                    RawHoneyProductionId = lot.RawHoneyProductionId, ReceivingOrgId = processor, ReceivedByUserId = "seed", DeliveryDate = DateOnly.FromDateTime(DateTime.Today),
                    Quantity = 50, Province = "XA", District = "XA", FormStoredFileName = "missing.pdf", FormOriginalFileName = "form.pdf", FormSha256 = new string('0', 64)
                };
                db.LotIntakes.Add(intake);
                var sent = new Sample { SampleCode = "XA-1", RawHoneyProductionId = lot.RawHoneyProductionId, TakingOrgId = processor, LabOrgId = processor, SealNumber = "XA-S1", CounterSealNumber = "XA-C1", CounterLocation = "XA", RetainUntil = DateOnly.FromDateTime(DateTime.Today.AddYears(1)), TakenByUserId = "seed", TakenAt = DateTime.UtcNow };
                var received = new Sample { SampleCode = "XA-2", RawHoneyProductionId = lot.RawHoneyProductionId, TakingOrgId = processor, LabOrgId = processor, SealNumber = "XA-S2", CounterSealNumber = "XA-C2", CounterLocation = "XA", RetainUntil = DateOnly.FromDateTime(DateTime.Today.AddYears(1)), TakenByUserId = "seed", TakenAt = DateTime.UtcNow, Status = SampleStatus.Received, SealIntact = true };
                db.Samples.AddRange(sent, received);
                await db.SaveChangesAsync();
                db.LabReports.Add(new LabReport { SampleId = received.SampleId, LabOrgId = processor, ReportCode = "XAREPORT0001", EnteredByUserId = "seed", Status = LabReportStatus.Submitted, StoredFileName = "missing.pdf", Sha256 = new string('0', 64), CreatedAt = DateTime.UtcNow });
                var batch = new FinalHoneyProduction { FinalHoneyProductionTitle = "XA Batch", FinalHoneyProductionDate = DateTime.Today, FinalHoneyProductionTotalAmount = 50, FinalHoneyProductionIsFinalized = true, FinalHoneyProductionQR = Guid.NewGuid(), OwnerOrgId = processor, FinalHoneyProductionWholesalerId = wholesaler.WholesalerId };
                db.FinalHoneyProductions.Add(batch);
                await db.SaveChangesAsync();
                db.FinalHoneyProductionDetails.Add(new FinalHoneyProductionDetail { FinalHoneyProductionId = batch.FinalHoneyProductionId, RawHoneyProductionId = lot.RawHoneyProductionId, UsedAmount = 50, CreatedAt = DateTime.Now });
                db.FinalHoneyCCPAssignments.Add(new FinalHoneyCCPAssignment { FinalHoneyProductionId = batch.FinalHoneyProductionId, FinalHoneyCCPAssignmentCCP = ccp10, FinalHoneyCCPAssignmentStatus = AssignmentStatusIds.Assigned, FinalHoneyCCPAssignmentStartDate = DateTime.Now });
                var shipment = new Shipment { FinalHoneyProductionId = batch.FinalHoneyProductionId, FromOrgId = processor, ToOrgId = receiver, Quantity = 10, ShippedByUserId = "seed", ShippedAt = DateTime.UtcNow };
                db.Shipments.Add(shipment);
                var consent = new Consent
                {
                    BeekeeperId = bk.BeekeeperId, AssociationOrgId = association, TextVersion = "v", TextSha256 = new string('0', 64), GivenOn = DateOnly.FromDateTime(DateTime.Today),
                    DocumentStoredFileName = "missing.pdf", DocumentOriginalFileName = "c.pdf", DocumentSha256 = new string('0', 64), RecordedByUserId = "seed", RecordedAt = DateTime.UtcNow
                };
                db.Consents.Add(consent);
                db.BeekeeperStorefronts.Add(new BeekeeperStorefront { BeekeeperId = bk.BeekeeperId, Code = "BK-XAXAXAXA", CreatedAt = DateTime.UtcNow });
                var document = new OrgDocument { OrgId = processor, Kind = "Licence", StoredFileName = "missing.pdf", OriginalFileName = "l.pdf", Sha256 = new string('0', 64), UploadedByUserId = "seed", UploadedAt = DateTime.UtcNow };
                db.OrgDocuments.Add(document);
                await db.SaveChangesAsync();
                return new Ids(lot.RawHoneyProductionId, intake.IntakeId, sent.SampleId, received.SampleId, batch.FinalHoneyProductionId, shipment.ShipmentId,
                    bk.BeekeeperId, consent.ConsentId, processor, document.DocumentId, ccp0, ccp10);
            });
        }

        [Fact]
        public async Task Organization_B_cannot_reach_organization_A_records_by_id()
        {
            var a = await SeedOrganizationAAsync();
            var orgB = await _app.CreateOrgAsync("XB Everything", null, (Capabilities.Processing, null), (Capabilities.Packing, null), (Capabilities.Wholesale, null),
                (Capabilities.Laboratory, "XB-ACC"), (Capabilities.Association, null));
            var users = new Dictionary<string, UserClient>();
            foreach (var role in new[] { OrgRoles.OrgAdmin, OrgRoles.Staff, OrgRoles.SampleAcceptanceOfficer, OrgRoles.Analyst, OrgRoles.LabManager, OrgRoles.QualityApprover, OrgRoles.FieldInspector })
            {
                var name = "xb_" + role.ToLowerInvariant();
                await _app.AddMemberAsync(orgB, await _app.CreateUserAsync(name), role);
                users[role] = await UserClient.LoginAsync(_app, name);
            }

            // reads: 404 (the record is not revealed) or a redirect to "access denied" for platform pages
            var reads = new (string Role, string Url)[]
            {
                (OrgRoles.SampleAcceptanceOfficer, $"/Intake/Form/{a.Intake}"),
                (OrgRoles.Analyst, $"/LabReports/Edit/{a.ReceivedSample}"), (OrgRoles.LabManager, $"/LabReports/Pdf/{a.ReceivedSample}"),
                (OrgRoles.Staff, $"/Flow/Lot/{a.Lot}"), (OrgRoles.Staff, $"/Flow/Batch/{a.Batch}"),
                (OrgRoles.Staff, $"/Recall/Lot/{a.Lot}"), (OrgRoles.Staff, $"/Recall/Batch/{a.Batch}"), (OrgRoles.Staff, $"/Recall/Batch/{a.Batch}?format=csv"),
                (OrgRoles.Staff, $"/OrgWorkspace/Batch/{a.Batch}"), (OrgRoles.Staff, $"/Shipments/Create?batchId={a.Batch}"), (OrgRoles.Staff, $"/Shipments/Storage?batchId={a.Batch}"),
                (OrgRoles.FieldInspector, $"/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId={a.Lot}&ccpId={a.Ccp0}"),
                (OrgRoles.Staff, $"/CCPMonitoring/FinalHoneyProductionManageCCP?finalHoneyProductionId={a.Batch}&ccpId={a.Ccp10}"),
                (OrgRoles.OrgAdmin, $"/Storefronts/Consent/{a.Beekeeper}"), (OrgRoles.OrgAdmin, $"/Storefronts/Card/{a.Beekeeper}"), (OrgRoles.OrgAdmin, $"/Storefronts/Document/{a.Consent}"),
                (OrgRoles.OrgAdmin, $"/Organizations/Details/{a.OrgA}"), (OrgRoles.OrgAdmin, $"/Organizations/Document/{a.Document}"),
                (OrgRoles.OrgAdmin, $"/Organizations/Edit/{a.OrgA}"), (OrgRoles.OrgAdmin, "/Registry/Index"), (OrgRoles.OrgAdmin, "/CcpResponsibilities/Index"),
                (OrgRoles.OrgAdmin, "/CCPMonitoring/Corrections"), (OrgRoles.OrgAdmin, "/Audit/Index")
            };
            var leaks = new List<string>();
            foreach (var (role, url) in reads)
            {
                var response = await users[role].GetAsync(url);
                var location = response.Headers.Location?.ToString() ?? "";
                var ok = response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden   // D-73: a refused CCP read answers 403
                         || (response.StatusCode == HttpStatusCode.Redirect && (location.Contains("AccessDenied") || location.Contains("/Index") || location.EndsWith("/Samples") || location.EndsWith("/Storefronts")));
                if (!ok)
                    leaks.Add($"{role} GET {url} -> {(int)response.StatusCode} {location}");
            }

            // writes: refused and nothing changes
            async Task Post(string role, string url, Dictionary<string, string> form, string tokenFrom = "/MyWork/Index")
            {
                var response = await users[role].PostAsync(url, form, tokenFrom);
                if (response.StatusCode == HttpStatusCode.OK)
                    leaks.Add($"{role} POST {url} -> 200");
            }
            await Post(OrgRoles.Analyst, "/Samples/Receive", new() { ["id"] = a.SentSample.ToString(), ["sealIntact"] = "true" });
            await Post(OrgRoles.Analyst, $"/LabReports/Save/{a.ReceivedSample}", new() { ["reportNumber"] = "XB" });
            await Post(OrgRoles.Analyst, $"/LabReports/Submit/{a.ReceivedSample}", new());
            await Post(OrgRoles.LabManager, $"/LabReports/Decide/{a.ReceivedSample}", new() { ["approve"] = "true" });
            await Post(OrgRoles.SampleAcceptanceOfficer, "/Samples/Create", new() { ["lotId"] = a.Lot.ToString(), ["labOrgId"] = orgB.ToString(), ["sealNumber"] = "XB1", ["counterSealNumber"] = "XB2", ["counterLocation"] = "XB", ["retainUntil"] = DateTime.Today.AddYears(1).ToString("yyyy-MM-dd") });
            await Post(OrgRoles.Staff, "/Shipments/Decide", new() { ["id"] = a.Shipment.ToString(), ["confirm"] = "true" });
            await Post(OrgRoles.Staff, "/Shipments/Create", new() { ["batchId"] = a.Batch.ToString(), ["toOrgId"] = orgB.ToString(), ["quantity"] = "5" });
            await Post(OrgRoles.Staff, $"/OrgWorkspace/RequestFinalize/{a.Batch}", new());
            await Post(OrgRoles.QualityApprover, $"/OrgWorkspace/Finalize/{a.Batch}", new());
            await Post(OrgRoles.FieldInspector, "/CCPMonitoring/RawHoneyProductionManageCCP", new() { ["CCPId"] = a.Ccp0.ToString(), ["RawHoneyProductionId"] = a.Lot.ToString() });
            await Post(OrgRoles.QualityApprover, "/CCPMonitoring/Approve", new() { ["ownerKind"] = "raw", ["ownerId"] = a.Lot.ToString(), ["ccpId"] = a.Ccp0.ToString() });
            await Post(OrgRoles.OrgAdmin, "/Storefronts/Withdraw", new() { ["consentId"] = a.Consent.ToString() });
            await Post(OrgRoles.OrgAdmin, $"/Organizations/ChangeStatus/{a.OrgA}", new() { ["toStatus"] = "Suspended", ["reason"] = "XB" });

            Assert.True(leaks.Count == 0, "Cross-organization access:\n" + string.Join("\n", leaks));
            var state = await _app.WithContextAsync(async db => new
            {
                Sent = await db.Samples.Where(s => s.SampleId == a.SentSample).Select(s => s.Status).FirstAsync(),
                Report = await db.LabReports.Where(r => r.SampleId == a.ReceivedSample).Select(r => new { r.Status, r.ReportNumber }).FirstAsync(),
                Samples = await db.Samples.CountAsync(s => s.RawHoneyProductionId == a.Lot),
                Shipment = await db.Shipments.Where(s => s.ShipmentId == a.Shipment).Select(s => s.Status).FirstAsync(),
                Shipments = await db.Shipments.CountAsync(s => s.FinalHoneyProductionId == a.Batch),
                Logs = await db.CriticalControlPointMonitoringLogs.CountAsync(l => l.RawHoneyProductionId == a.Lot),
                Ccp0 = await db.RawHoneyCCPAssignments.Where(x => x.RawHoneyProductionId == a.Lot && x.RawHoneyCCPAssignmentCCP == a.Ccp0).Select(x => x.ApprovedAt).FirstAsync(),
                Consent = await db.Consents.Where(c => c.ConsentId == a.Consent).Select(c => c.WithdrawnAt).FirstAsync(),
                OrgA = await db.Organizations.Where(o => o.OrgId == a.OrgA).Select(o => o.Status).FirstAsync()
            });
            Assert.Equal(SampleStatus.Sent, state.Sent);
            Assert.Equal((LabReportStatus.Submitted, (string?)null), (state.Report.Status, state.Report.ReportNumber));
            Assert.Equal(2, state.Samples);
            Assert.Equal(ShipmentStatus.Pending, state.Shipment);
            Assert.Equal(1, state.Shipments);
            Assert.Equal(0, state.Logs);
            Assert.Null(state.Ccp0);
            Assert.Null(state.Consent);
            Assert.Equal(OrgStatus.Verified, state.OrgA);

            // and B's work list shows nothing of A
            var work = await users[OrgRoles.QualityApprover].HtmlAsync("/MyWork/Index");
            // anti-forgery tokens (also the tour's data-token) and the file version hashes (?v=…) can contain "XA" by chance; only the content counts
            work = System.Text.RegularExpressions.Regex.Replace(work, "name=\"__RequestVerificationToken\"[^>]*value=\"[^\"]*\"|value=\"[^\"]*\"[^>]*name=\"__RequestVerificationToken\"|data-token=\"[^\"]*\"|[?&]v=[A-Za-z0-9_-]+", "");
            Assert.DoesNotContain("XA", work);
        }
    }
}
