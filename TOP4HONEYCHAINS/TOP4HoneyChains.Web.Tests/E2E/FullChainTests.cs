using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    [CollectionDefinition("E2E")]
    public class E2ECollection : ICollectionFixture<E2EApp> { }

    // The whole chain with the organization model on (D-46): intake -> sample -> lab approval -> CCP entry and
    // approval -> batch -> finalize -> shipment -> CCP 10 by the receiver -> Honey Story badges -> storefront ->
    // recall report. Every step goes through the web endpoints as the user who does it in real life.
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class FullChainTests
    {
        private readonly E2EApp _app;

        public FullChainTests(E2EApp app) => _app = app;

        private async Task<string> SaveCcpAsync(UserClient user, string kind, int ownerId, int ccpId, string number = "1")
        {
            var parameters = await _app.WithContextAsync(db => db.CriticalControlPointParameters.AsNoTracking()
                .Where(p => p.CCPId == ccpId && p.DataType != CcpDataType.File).OrderBy(p => p.ParameterId).ToListAsync());
            var page = kind == "raw" ? "RawHoneyProductionManageCCP" : "FinalHoneyProductionManageCCP";
            var idField = kind == "raw" ? "RawHoneyProductionId" : "FinalHoneyProductionId";
            var form = new Dictionary<string, string> { ["CCPId"] = ccpId.ToString(), [idField] = ownerId.ToString() };
            for (var i = 0; i < parameters.Count; i++)
            {
                var p = parameters[i];
                form[$"Groups[0].Parameters[{i}].ParameterId"] = p.ParameterId.ToString();
                if (p.DataType == CcpDataType.YesNo) form[$"Groups[0].Parameters[{i}].MeasuredTextValue"] = "Yes";
                else if (p.DataType == CcpDataType.Text) form[$"Groups[0].Parameters[{i}].MeasuredTextValue"] = "e2e";
                else form[$"Groups[0].Parameters[{i}].MeasuredNumericValue"] = number;
            }
            var from = $"/CCPMonitoring/{page}?{(kind == "raw" ? "rawHoneyProductionId" : "finalHoneyProductionId")}={ownerId}&ccpId={ccpId}";
            return await user.PostFlashAsync($"/CCPMonitoring/{page}", form, from);
        }

        private static async Task<string> ApproveCcpAsync(UserClient user, string kind, int ownerId, int ccpId)
        {
            var from = kind == "raw"
                ? $"/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId={ownerId}&ccpId={ccpId}"
                : $"/CCPMonitoring/FinalHoneyProductionManageCCP?finalHoneyProductionId={ownerId}&ccpId={ccpId}";
            return await user.PostFlashAsync("/CCPMonitoring/Approve", new() { ["ownerKind"] = kind, ["ownerId"] = ownerId.ToString(), ["ccpId"] = ccpId.ToString() }, from);
        }

        private Task<int> CcpIdAsync(string code) => _app.WithContextAsync(db => db.CriticalControlPoints.Where(c => c.Code == code).Select(c => c.Id).FirstAsync());

        [Fact]
        public async Task Intake_to_recall_report()
        {
            // ---- organizations, members and master data (set up directly; the chain itself goes through the app)
            var (ws, beekeeperId, apiaryId) = await _app.WithContextAsync(async db =>
            {
                var w = new Wholesaler { WholesalerTitle = "E2E Wholesaler", WholesalerIsActive = true, CreatedAt = DateTime.Now };
                db.Wholesalers.Add(w);
                var bk = new Beekeeper { BeekeeperId = Guid.NewGuid().ToString(), BeekeeperFirstName = "E2E", BeekeeperLastName = "Beekeeper", BeekeeperBirthDate = new DateOnly(1980, 1, 1), BeekeeperBusinessNumber = "E2E-BEE-0001", IsActive = true };
                db.Beekeepers.Add(bk);
                await db.SaveChangesAsync();
                var ap = new Apiary { ApiaryBeekeeperId = bk.BeekeeperId, ApiaryTitle = "E2E Apiary", ApiaryBeehivesCount = 10, ApiaryQRCode = Guid.NewGuid() };
                db.Apiaries.Add(ap);
                await db.SaveChangesAsync();
                return (w.WholesalerId, bk.BeekeeperId, ap.ApiaryId);
            });
            var processor = await _app.CreateOrgAsync("E2E Processor", ws, (Capabilities.Processing, null), (Capabilities.Packing, null), (Capabilities.Wholesale, null));
            var lab = await _app.CreateOrgAsync("E2E Lab", null, (Capabilities.Laboratory, "E2E-ACC-1"));
            var association = await _app.CreateOrgAsync("E2E Association", null, (Capabilities.Association, null));
            var receiver = await _app.CreateOrgAsync("E2E Receiver", null, (Capabilities.Wholesale, null));
            await _app.CreateUserAsync("e2e_admin", "Administrator");
            foreach (var (org, role, user) in new[]
            {
                (processor, OrgRoles.SampleAcceptanceOfficer, "e2e_sao"), (processor, OrgRoles.Staff, "e2e_staff"), (processor, OrgRoles.QualityApprover, "e2e_qa"),
                (lab, OrgRoles.Analyst, "e2e_analyst"), (lab, OrgRoles.LabManager, "e2e_labmgr"),
                (association, OrgRoles.FieldInspector, "e2e_fi"), (association, OrgRoles.QualityApprover, "e2e_aqa"), (association, OrgRoles.OrgAdmin, "e2e_aadmin"),
                (receiver, OrgRoles.Staff, "e2e_wstaff"), (receiver, OrgRoles.QualityApprover, "e2e_wqa")
            })
                await _app.AddMemberAsync(org, await _app.CreateUserAsync(user), role);

            var admin = await UserClient.LoginAsync(_app, "e2e_admin");
            Assert.Contains("Association saved.", await admin.PostFlashAsync($"/Registry/SetAssociation/{beekeeperId}", new() { ["associationOrgId"] = association.ToString() }, $"/Registry/Edit/{beekeeperId}"));

            // ---- 1. intake by the SampleAcceptanceOfficer
            var sao = await UserClient.LoginAsync(_app, "e2e_sao");
            var period = await _app.WithContextAsync(db => db.RawHoneyProductionPeriods.Select(p => p.RawHoneyProductionPeriodId).FirstAsync());
            var honey = await _app.WithContextAsync(db => db.HoneyTypes.Select(h => h.HoneyTypeId).FirstAsync());
            var intake = await sao.PostMultipartAsync("/Intake/Create", new()
            {
                ["number"] = "E2E-BEE-0001", ["apiaryId"] = apiaryId.ToString(), ["honeyTypeId"] = honey.ToString(), ["periodId"] = period.ToString(),
                ["deliveryDate"] = DateTime.Today.ToString("yyyy-MM-dd"), ["quantity"] = "100", ["province"] = "E2E Il", ["district"] = "E2E Ilce", ["originCountry"] = "TR"
            }, Pdf.File("deliveryForm"), "/Intake/Create?number=E2E-BEE-0001");
            var location = intake.Headers.Location?.ToString() ?? "";
            Assert.True(location.Contains("lotId="), "intake did not redirect to the sample form: " + await sao.FlashAfterAsync(intake));
            var lot = int.Parse(Regex.Match(location, @"lotId=(\d+)").Groups[1].Value);

            // ---- 2. sample sent to the laboratory
            Assert.Contains("sent to the laboratory", await sao.PostFlashAsync("/Samples/Create", new()
            {
                ["lotId"] = lot.ToString(), ["labOrgId"] = lab.ToString(), ["sealNumber"] = "E2E-S1", ["counterSealNumber"] = "E2E-C1",
                ["counterLocation"] = "E2E shelf", ["retainUntil"] = DateTime.Today.AddYears(1).ToString("yyyy-MM-dd")
            }, $"/Samples/Create?lotId={lot}"));
            var sampleId = await _app.WithContextAsync(db => db.Samples.Where(s => s.RawHoneyProductionId == lot).Select(s => s.SampleId).FirstAsync());

            // ---- 3. lab: receipt, results with the report PDF, approval by another person
            var analyst = await UserClient.LoginAsync(_app, "e2e_analyst");
            Assert.Contains("seal is intact", await analyst.PostFlashAsync("/Samples/Receive", new() { ["id"] = sampleId.ToString(), ["sealIntact"] = "true" }, "/Samples/Index"));
            var ccp1 = await CcpIdAsync("CCP 1");
            var labParams = await _app.WithContextAsync(db => db.CriticalControlPointParameters.Where(p => p.CCPId == ccp1 && p.DataType != CcpDataType.File).ToListAsync());
            var results = new Dictionary<string, string> { ["reportNumber"] = "E2E-R-1", ["issuedOn"] = DateTime.Today.ToString("yyyy-MM-dd") };
            foreach (var p in labParams)
                results[$"values[{p.ParameterId}]"] = p.DataType == CcpDataType.Text ? "e2e" : p.DataType == CcpDataType.YesNo ? "Yes" : "1";
            Assert.Contains("Lab results saved.", await analyst.FlashAfterAsync(await analyst.PostMultipartAsync($"/LabReports/Save/{sampleId}", results, Pdf.File("reportFile"), $"/LabReports/Edit/{sampleId}")));
            Assert.Contains("Report submitted", await analyst.PostFlashAsync($"/LabReports/Submit/{sampleId}", new(), $"/LabReports/Edit/{sampleId}"));
            var labManager = await UserClient.LoginAsync(_app, "e2e_labmgr");
            Assert.Contains("Lab report approved", await labManager.PostFlashAsync($"/LabReports/Decide/{sampleId}", new() { ["approve"] = "true" }, $"/LabReports/Edit/{sampleId}"));
            var ccp1State = await _app.WithContextAsync(db => db.RawHoneyCCPAssignments.Where(a => a.RawHoneyProductionId == lot && a.RawHoneyCCPAssignmentCCP == ccp1)
                .Select(a => new { a.RawHoneyCCPAssignmentStatus, a.ApprovedAt }).FirstAsync());
            Assert.Equal(AssignmentStatusIds.Completed, ccp1State.RawHoneyCCPAssignmentStatus);
            Assert.NotNull(ccp1State.ApprovedAt);

            // ---- 4. CCP entry and approval: association (CCP 0), processor (CCP 2, CCP 3)
            var fi = await UserClient.LoginAsync(_app, "e2e_fi");
            var assocQa = await UserClient.LoginAsync(_app, "e2e_aqa");
            var staff = await UserClient.LoginAsync(_app, "e2e_staff");
            var qa = await UserClient.LoginAsync(_app, "e2e_qa");
            var ccp0 = await CcpIdAsync("CCP 0");
            Assert.Contains("values saved", await SaveCcpAsync(fi, "raw", lot, ccp0));
            Assert.Contains("approved", await ApproveCcpAsync(assocQa, "raw", lot, ccp0));
            foreach (var ccpCode in new[] { "CCP 2", "CCP 3" })
            {
                var id = await CcpIdAsync(ccpCode);
                Assert.Contains("values saved", await SaveCcpAsync(staff, "raw", lot, id));
                Assert.Contains("approved", await ApproveCcpAsync(qa, "raw", lot, id));
            }

            // ---- 5. batch (administrator master data) with every batch CCP
            var create = await admin.PostAsync("/FinalHoneyProductions/Create", new()
            {
                ["FinalHoneyProductionWholesalerId"] = ws.ToString(), ["FinalHoneyProductionTitle"] = "E2E Batch", ["FinalHoneyProductionDate"] = DateTime.Today.ToString("yyyy-MM-dd")
            }, $"/FinalHoneyProductions/Create?wholesalerId={ws}");
            var batch = int.Parse(Regex.Match(create.Headers.Location?.ToString() ?? "", @"ManageBatch/(\d+)").Groups[1].Value);
            await admin.PostAsync("/FinalHoneyProductions/ManageBatchPost", new() { ["batchId"] = batch.ToString(), [$"usedAmounts[{lot}]"] = "80" }, $"/FinalHoneyProductions/ManageBatch/{batch}");
            var batchCcps = await _app.WithContextAsync(db => db.CriticalControlPoints.Where(c => c.AppliesTo == CcpAppliesTo.Batch).Select(c => c.Id).ToListAsync());
            var assign = new Dictionary<string, string> { ["FinalHoneyProductionId"] = batch.ToString() };
            for (var i = 0; i < batchCcps.Count; i++)
                assign[$"SelectedCCPIds[{i}]"] = batchCcps[i].ToString();
            await admin.PostAsync($"/FinalHoneyProductions/ManageCCP/{batch}", assign, $"/FinalHoneyProductions/ManageCCP/{batch}");
            Assert.Equal(80m, await _app.WithContextAsync(db => db.FinalHoneyProductions.Where(f => f.FinalHoneyProductionId == batch).Select(f => f.FinalHoneyProductionTotalAmount ?? 0).FirstAsync()));
            Assert.Equal(batchCcps.Count, await _app.WithContextAsync(db => db.FinalHoneyCCPAssignments.CountAsync(a => a.FinalHoneyProductionId == batch)));
            // D-49: the details page shows only what is still missing until every required test is done
            var openDetails = await admin.HtmlAsync($"/FinalHoneyProductions/Details/{batch}");
            Assert.Contains("details-not-completed", openDetails);
            Assert.DoesNotContain("details-lots", openDetails);
            foreach (var id in batchCcps)
            {
                Assert.Contains("values saved", await SaveCcpAsync(staff, "batch", batch, id));
                Assert.Contains("approved", await ApproveCcpAsync(qa, "batch", batch, id));
            }

            // ---- 6. finalize: request (Staff) and confirmation (QualityApprover)
            Assert.Contains("Finalizing requested", await staff.PostFlashAsync($"/OrgWorkspace/RequestFinalize/{batch}", new(), $"/OrgWorkspace/Batch/{batch}"));
            Assert.Contains("Batch finalized.", await qa.PostFlashAsync($"/OrgWorkspace/Finalize/{batch}", new(), $"/OrgWorkspace/Batch/{batch}"));
            var details = await admin.HtmlAsync($"/FinalHoneyProductions/Details/{batch}");
            foreach (var part in new[] { "details-completed", "details-lots", "details-ccps", "details-reports", "details-honeystory" })
                Assert.Contains(part, details);
            var shares = await _app.WithContextAsync(db => db.BatchOriginShares.Where(s => s.FinalHoneyProductionId == batch).ToListAsync());
            Assert.Equal(("TR", 100m), (Assert.Single(shares).CountryCode, shares[0].Percentage));

            // ---- 7. shipment and receipt
            Assert.Contains("Shipment created", await staff.PostFlashAsync("/Shipments/Create", new() { ["batchId"] = batch.ToString(), ["toOrgId"] = receiver.ToString(), ["quantity"] = "30" }, "/Shipments/Index"));
            var receiverStaff = await UserClient.LoginAsync(_app, "e2e_wstaff");
            var shipmentId = await _app.WithContextAsync(db => db.Shipments.Where(s => s.FinalHoneyProductionId == batch).Select(s => s.ShipmentId).FirstAsync());
            Assert.Contains("Receipt confirmed", await receiverStaff.PostFlashAsync("/Shipments/Decide", new() { ["id"] = shipmentId.ToString(), ["confirm"] = "true" }, "/Shipments/Index"));

            // ---- 8. CCP 10 by the receiver, approved by its QualityApprover
            var ccp10 = await CcpIdAsync("CCP 10");
            var storage = new Dictionary<string, string> { ["batchId"] = batch.ToString() };
            foreach (var p in await _app.WithContextAsync(db => db.CriticalControlPointParameters.Where(p => p.CCPId == ccp10).ToListAsync()))
                storage[$"values[{p.ParameterId}]"] = p.DataType == CcpDataType.Number ? "18" : "e2e";
            Assert.Contains("Storage values saved.", await receiverStaff.PostFlashAsync("/Shipments/Storage", storage, $"/Shipments/Storage?batchId={batch}"));
            var receiverQa = await UserClient.LoginAsync(_app, "e2e_wqa");
            Assert.Contains("CCP record approved.", await receiverQa.PostFlashAsync("/Shipments/ApproveStorage", new() { ["batchId"] = batch.ToString() }, $"/Shipments/Storage?batchId={batch}"));

            // ---- 9. Honey Story badges (public)
            var qr = await _app.WithContextAsync(db => db.FinalHoneyProductions.Where(f => f.FinalHoneyProductionId == batch).Select(f => f.FinalHoneyProductionQR).FirstAsync());
            var visitor = UserClient.Anonymous(_app);
            var story = await visitor.HtmlAsync($"/honeystory/{qr}");
            Assert.Contains("id=\"badge-accredited\"", story);
            Assert.Contains("E2E-ACC-1", story);
            Assert.Contains("id=\"badge-approved\"", story);
            Assert.Contains("id=\"badge-origin\"", story);
            Assert.DoesNotContain("storefront-link", story);   // no consent yet

            // ---- 10. storefront: verified registry number + consent recorded by the association
            Assert.Contains("Submitted", await admin.FlashAfterAsync(await admin.PostMultipartAsync($"/Registry/Submit/{beekeeperId}",
                new() { ["enterpriseNumber"] = "TR810000000099" }, Pdf.File("proof"), $"/Registry/Edit/{beekeeperId}")));
            var requestId = await _app.WithContextAsync(db => db.RegistryVerifications.Where(r => r.BeekeeperId == beekeeperId).Select(r => r.Id).FirstAsync());
            Assert.Contains("verified", await admin.PostFlashAsync("/Registry/Decide", new() { ["requestId"] = requestId.ToString(), ["approve"] = "true" }, $"/Registry/Edit/{beekeeperId}"));
            var assocAdmin = await UserClient.LoginAsync(_app, "e2e_aadmin");
            Assert.Contains("storefront is published", await assocAdmin.FlashAfterAsync(await assocAdmin.PostMultipartAsync($"/Storefronts/Consent/{beekeeperId}",
                new() { ["textRead"] = "true", ["nameConsented"] = "false", ["givenOn"] = DateTime.Today.ToString("yyyy-MM-dd") }, Pdf.File("document"), $"/Storefronts/Consent/{beekeeperId}")));
            var code = await _app.WithContextAsync(db => db.BeekeeperStorefronts.Where(s => s.BeekeeperId == beekeeperId).Select(s => s.Code).FirstAsync());
            var storefront = System.Net.WebUtility.HtmlDecode(await visitor.HtmlAsync($"/storefront/{code}"));
            Assert.Contains(code, storefront);
            Assert.DoesNotContain("E2E Beekeeper", storefront);   // the name was not consented
            Assert.Contains("Düzce", storefront);                 // province from the verified registry number
            Assert.Contains($"/storefront/{code}", await visitor.HtmlAsync($"/honeystory/{qr}"));

            // ---- 11. recall report: owner sees upstream and its shipment; the receiver only its part
            var recall = await staff.HtmlAsync($"/Recall/Batch/{batch}");
            Assert.Contains($"data-lot-id=\"{lot}\"", recall);
            Assert.Contains($"data-shipment-id=\"{shipmentId}\"", recall);
            var csv = await (await staff.GetAsync($"/Recall/Batch/{batch}?format=csv")).Content.ReadAsStringAsync();
            Assert.Contains("Intake,", csv);
            Assert.Contains("Shipment,", csv);
            var receiverRecall = await receiverStaff.HtmlAsync($"/Recall/Batch/{batch}");
            Assert.DoesNotContain($"data-lot-id=\"{lot}\"", receiverRecall);
            Assert.Contains("id=\"hidden-lots\"", receiverRecall);

            // ---- audit chain is intact after the whole run
            Assert.Contains("verify", await admin.PostFlashAsync("/Audit/Verify", new(), "/Audit/Index"));
        }
    }
}
