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

        // answer: a text answer for a parameter ("" = leave it empty, null = the default answer)
        private async Task<string> SaveCcpAsync(UserClient user, string kind, int ownerId, int ccpId, string number = "1",
            Func<CriticalControlPointParameter, string?>? answer = null)
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
                if (answer?.Invoke(p) is string given)
                {
                    if (given.Length > 0)
                        form[$"Groups[0].Parameters[{i}].MeasuredTextValue"] = given;
                }
                else if (p.DataType == CcpDataType.YesNo) form[$"Groups[0].Parameters[{i}].MeasuredTextValue"] = "Yes";
                else if (p.DataType == CcpDataType.Text) form[$"Groups[0].Parameters[{i}].MeasuredTextValue"] = "e2e";
                else form[$"Groups[0].Parameters[{i}].MeasuredNumericValue"] = number;
            }
            var from = $"/CCPMonitoring/{page}?{(kind == "raw" ? "rawHoneyProductionId" : "finalHoneyProductionId")}={ownerId}&ccpId={ccpId}";
            return await user.PostFlashAsync($"/CCPMonitoring/{page}", form, from);
        }

        private static async Task<string> ApproveCcpAsync(UserClient user, string kind, int ownerId, int ccpId, bool confirmOutOfLimit = false)
        {
            var from = kind == "raw"
                ? $"/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId={ownerId}&ccpId={ccpId}"
                : $"/CCPMonitoring/FinalHoneyProductionManageCCP?finalHoneyProductionId={ownerId}&ccpId={ccpId}";
            var form = new Dictionary<string, string> { ["ownerKind"] = kind, ["ownerId"] = ownerId.ToString(), ["ccpId"] = ccpId.ToString() };
            if (confirmOutOfLimit)
                form["confirmOutOfLimit"] = "true";
            return await user.PostFlashAsync("/CCPMonitoring/Approve", form, from);
        }

        // D-64: with values outside their limits the approval needs the in-page confirmation; returns whether it was needed
        private async Task<bool> ApproveCcpConfirmingAsync(UserClient user, string kind, int ownerId, int ccpId)
        {
            var outOfLimit = await _app.WithContextAsync(db => db.CriticalControlPointMonitoringLogs.Where(l => l.CCPId == ccpId && l.IsInRange == false
                && (kind == "raw" ? l.RawHoneyProductionId == ownerId : l.FinalHoneyProductionId == ownerId)).Select(l => l.ParameterId).ToListAsync());
            if (outOfLimit.Count > 0)
            {
                var page = await user.HtmlAsync(kind == "raw"
                    ? $"/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId={ownerId}&ccpId={ccpId}"
                    : $"/CCPMonitoring/FinalHoneyProductionManageCCP?finalHoneyProductionId={ownerId}&ccpId={ccpId}");
                Assert.Contains("id=\"approve-note\"", page);
                Assert.Contains("id=\"oolc-approve-form\"", page);
                foreach (var id in outOfLimit)
                    Assert.Contains($"data-parameter-id=\"{id}\"", page);
                Assert.Contains("The record has values outside their limits; confirm them to approve.", await ApproveCcpAsync(user, kind, ownerId, ccpId));
                Assert.Null(await _app.WithContextAsync(db => kind == "raw"
                    ? db.RawHoneyCCPAssignments.Where(a => a.RawHoneyProductionId == ownerId && a.RawHoneyCCPAssignmentCCP == ccpId).Select(a => a.ApprovedAt).FirstAsync()
                    : db.FinalHoneyCCPAssignments.Where(a => a.FinalHoneyProductionId == ownerId && a.FinalHoneyCCPAssignmentCCP == ccpId).Select(a => a.ApprovedAt).FirstAsync()));
            }
            Assert.Contains("approved", await ApproveCcpAsync(user, kind, ownerId, ccpId, outOfLimit.Count > 0));
            return outOfLimit.Count > 0;
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
            // D-52: province and district come from the lists; a district of another province is refused
            Dictionary<string, string> IntakeForm(string district) => new()
            {
                ["number"] = "E2E-BEE-0001", ["apiaryId"] = apiaryId.ToString(), ["honeyTypeId"] = honey.ToString(), ["periodId"] = period.ToString(),
                ["deliveryDate"] = DateTime.Today.ToString("yyyy-MM-dd"), ["quantity"] = "100", ["province"] = "Düzce", ["district"] = district, ["originCountry"] = "TR"
            };
            Assert.Contains("Choose a district of the selected province.", await sao.FlashAfterAsync(
                await sao.PostMultipartAsync("/Intake/Create", IntakeForm("Kadıköy"), Pdf.File("deliveryForm"), "/Intake/Create?number=E2E-BEE-0001")));
            var intake = await sao.PostMultipartAsync("/Intake/Create", IntakeForm("Akçakoca"), Pdf.File("deliveryForm"), "/Intake/Create?number=E2E-BEE-0001");
            var location = intake.Headers.Location?.ToString() ?? "";
            Assert.True(location.Contains("lotId="), "intake did not redirect to the sample form: " + await sao.FlashAfterAsync(intake));
            // D-63: right after the delivery the reception officer has the lot's CCP 0 in My work
            Assert.Contains($"ccpId={(await CcpIdAsync("CCP 0"))}", await sao.HtmlAsync("/MyWork/Index"));
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
            var partial = new Dictionary<string, string> { ["reportNumber"] = "E2E-R-1", ["issuedOn"] = DateTime.Today.ToString("yyyy-MM-dd"), [$"values[{labParams[0].ParameterId}]"] = "1" };

            // D-61: no lab test before the lot's CCP 0 (reception officer, approved by the quality approver; D-63) is done
            Assert.Contains("work-waiting", await analyst.HtmlAsync("/MyWork/Index"));
            Assert.Contains("CCP 0 (primary audit) of this lot is not completed yet", await analyst.FlashAfterAsync(
                await analyst.PostMultipartAsync($"/LabReports/Save/{sampleId}", new Dictionary<string, string>(partial), Pdf.File("reportFile"), $"/LabReports/Edit/{sampleId}")));
            // D-63: the reception officer who recorded the delivery does CCP 0; a quality approver of the lot owner approves
            var qa = await UserClient.LoginAsync(_app, "e2e_qa");
            var ccp0 = await CcpIdAsync("CCP 0");
            // D-62: the follow-up question is required only when its yes/no question is answered "Yes"
            var followUp = await _app.WithContextAsync(db => db.CriticalControlPointParameters.Where(p => p.CCPId == ccp0 && p.RequiredIfParameterId != null)
                .Select(p => new { p.ParameterId, Condition = p.RequiredIfParameterId!.Value }).FirstAsync());
            async Task<int> Ccp0StatusAsync() => await _app.WithContextAsync(db => db.RawHoneyCCPAssignments
                .Where(a => a.RawHoneyProductionId == lot && a.RawHoneyCCPAssignmentCCP == ccp0).Select(a => a.RawHoneyCCPAssignmentStatus).FirstAsync());
            Assert.Contains("values saved", await SaveCcpAsync(sao, "raw", lot, ccp0, answer: p => p.ParameterId == followUp.ParameterId ? "" : null));
            Assert.Equal(AssignmentStatusIds.InProgress, await Ccp0StatusAsync());   // "Yes" without the follow-up: not complete
            Assert.Contains("values saved", await SaveCcpAsync(sao, "raw", lot, ccp0,
                answer: p => p.ParameterId == followUp.ParameterId ? "" : p.ParameterId == followUp.Condition ? "No" : null));
            Assert.Equal(AssignmentStatusIds.Submitted, await Ccp0StatusAsync());    // "No": complete without it
            Assert.Contains($"ccpId={ccp0}", await qa.HtmlAsync("/MyWork/Index"));   // the approver gets the task
            Assert.Contains("approved", await ApproveCcpAsync(qa, "raw", lot, ccp0));
            Assert.DoesNotContain("work-waiting", await analyst.HtmlAsync("/MyWork/Index"));

            // D-54: with a required value missing the report cannot be submitted
            Assert.Contains("Lab results saved.", await analyst.FlashAfterAsync(await analyst.PostMultipartAsync($"/LabReports/Save/{sampleId}", partial, Pdf.File("reportFile"), $"/LabReports/Edit/{sampleId}")));
            Assert.Contains("Enter all required values before submitting", await analyst.PostFlashAsync($"/LabReports/Submit/{sampleId}", new(), $"/LabReports/Edit/{sampleId}"));
            Assert.Matches(@"id=""submit-report"" disabled", await analyst.HtmlAsync($"/LabReports/Edit/{sampleId}"));
            var results = new Dictionary<string, string> { ["reportNumber"] = "E2E-R-1", ["issuedOn"] = DateTime.Today.ToString("yyyy-MM-dd") };
            foreach (var p in labParams)
                results[$"values[{p.ParameterId}]"] = p.DataType == CcpDataType.Text ? "e2e" : p.DataType == CcpDataType.YesNo ? "Yes" : "1";
            var tooHigh = labParams.First(p => p.DataType == CcpDataType.Number && p.MaxValue != null);   // D-64: one value outside its limit
            results[$"values[{tooHigh.ParameterId}]"] = (tooHigh.MaxValue!.Value + 1000).ToString(System.Globalization.CultureInfo.InvariantCulture);
            Assert.Contains("Lab results saved.", await analyst.FlashAfterAsync(await analyst.PostMultipartAsync($"/LabReports/Save/{sampleId}", results, Pdf.File("reportFile"), $"/LabReports/Edit/{sampleId}")));
            Assert.Contains("Report submitted", await analyst.PostFlashAsync($"/LabReports/Submit/{sampleId}", new(), $"/LabReports/Edit/{sampleId}"));
            var labManager = await UserClient.LoginAsync(_app, "e2e_labmgr");
            // D-64: the manager sees the note and the confirmation listing the value; without confirming, the report stays unapproved
            var decidePage = System.Net.WebUtility.HtmlDecode(await labManager.HtmlAsync($"/LabReports/Edit/{sampleId}"));
            Assert.Contains("not that the honey complies", decidePage);
            Assert.Contains("id=\"oolc-decide-form\"", decidePage);
            Assert.Contains($"data-parameter-id=\"{tooHigh.ParameterId}\"", decidePage);
            if (!tooHigh.LimitVerified)
                Assert.Contains("limit not verified yet", decidePage);
            Assert.Contains("The record has values outside their limits; confirm them to approve.", await labManager.PostFlashAsync($"/LabReports/Decide/{sampleId}", new() { ["approve"] = "true" }, $"/LabReports/Edit/{sampleId}"));
            Assert.Equal(LabReportStatus.Submitted, await _app.WithContextAsync(db => db.LabReports.Where(r => r.SampleId == sampleId).Select(r => r.Status).FirstAsync()));
            Assert.Contains("Lab report approved", await labManager.PostFlashAsync($"/LabReports/Decide/{sampleId}",
                new() { ["approve"] = "true", ["confirmOutOfLimit"] = "true" }, $"/LabReports/Edit/{sampleId}"));
            Assert.True(await _app.WithContextAsync(db => db.LabReports.Where(r => r.SampleId == sampleId)
                .AnyAsync(r => r.Results.Any(x => x.ParameterId == tooHigh.ParameterId && x.IsInRange == false))));   // stays flagged
            var ccp1State = await _app.WithContextAsync(db => db.RawHoneyCCPAssignments.Where(a => a.RawHoneyProductionId == lot && a.RawHoneyCCPAssignmentCCP == ccp1)
                .Select(a => new { a.RawHoneyCCPAssignmentStatus, a.ApprovedAt }).FirstAsync());
            Assert.Equal(AssignmentStatusIds.Completed, ccp1State.RawHoneyCCPAssignmentStatus);
            Assert.NotNull(ccp1State.ApprovedAt);

            // ---- 4. CCP entry and approval: processor (CCP 2, CCP 3); CCP 0 was done before the lab test
            var staff = await UserClient.LoginAsync(_app, "e2e_staff");
            foreach (var ccpCode in new[] { "CCP 2", "CCP 3" })
            {
                var id = await CcpIdAsync(ccpCode);
                Assert.Contains("values saved", await SaveCcpAsync(staff, "raw", lot, id));
                var needed = await ApproveCcpConfirmingAsync(qa, "raw", lot, id);
                if (ccpCode == "CCP 2")
                    Assert.True(needed, "CCP 2 with \"1\" is below its minimums and needs the confirmation");
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
                await ApproveCcpConfirmingAsync(qa, "batch", batch, id);
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

            // D-73: the receiver reads the processor's control points of the batch and of its lots, read-only
            foreach (var url in new[] { $"/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId={lot}&ccpId={ccp0}",
                                        $"/CCPMonitoring/FinalHoneyProductionManageCCP?finalHoneyProductionId={batch}&ccpId={batchCcps.Min()}" })
            {
                var response = await receiverStaff.GetAsync(url);
                Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
                var page = await response.Content.ReadAsStringAsync();
                Assert.Contains("<fieldset disabled", page);
                Assert.DoesNotContain("id=\"approve-form\"", page);
                Assert.DoesNotContain("btn btn-success px-5 shadow", page);   // no save button
            }

            // ---- 8. CCP 10 by the receiver, approved by its QualityApprover
            var ccp10 = await CcpIdAsync("CCP 10");
            var storage = new Dictionary<string, string> { ["batchId"] = batch.ToString() };
            var ccp10Params = await _app.WithContextAsync(db => db.CriticalControlPointParameters.Where(p => p.CCPId == ccp10).ToListAsync());
            foreach (var p in ccp10Params)
                storage[$"values[{p.ParameterId}]"] = p.DataType == CcpDataType.Number ? "18" : "e2e";
            var tooWarm = ccp10Params.First(p => p.DataType == CcpDataType.Number && p.MaxValue != null);   // D-64: one value outside its limit
            storage[$"values[{tooWarm.ParameterId}]"] = (tooWarm.MaxValue!.Value + 5).ToString(System.Globalization.CultureInfo.InvariantCulture);
            Assert.Contains("Storage values saved.", await receiverStaff.PostFlashAsync("/Shipments/Storage", storage, $"/Shipments/Storage?batchId={batch}"));
            var receiverQa = await UserClient.LoginAsync(_app, "e2e_wqa");
            var storagePage = await receiverQa.HtmlAsync($"/Shipments/Storage?batchId={batch}");
            Assert.Contains("id=\"oolc-storage-approve-form\"", storagePage);
            Assert.Contains($"data-parameter-id=\"{tooWarm.ParameterId}\"", storagePage);
            Assert.Contains("The record has values outside their limits; confirm them to approve.", await receiverQa.PostFlashAsync("/Shipments/ApproveStorage", new() { ["batchId"] = batch.ToString() }, $"/Shipments/Storage?batchId={batch}"));
            Assert.Contains("CCP record approved.", await receiverQa.PostFlashAsync("/Shipments/ApproveStorage",
                new() { ["batchId"] = batch.ToString(), ["confirmOutOfLimit"] = "true" }, $"/Shipments/Storage?batchId={batch}"));

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
            // D-52: a beekeeper found by registry number (TR + plate 81) gets the province preselected on intake
            var intakeForm = System.Net.WebUtility.HtmlDecode(await sao.HtmlAsync("/Intake/Create?number=TR810000000099"));
            Assert.Matches(@"<option value=""Düzce"" data-plate=""81""[^>]*selected", intakeForm);
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
            // B9 (D-84): a member sees only its own holding, and the heading says so; an administrator sees all holders
            Assert.Contains("Your current holding", receiverRecall);
            Assert.Contains("Current holders", await admin.HtmlAsync($"/Recall/Batch/{batch}"));
            // B6: the shipment tables become cards on phones (labels for every cell)
            var shipments = await receiverStaff.HtmlAsync("/Shipments/Index");
            Assert.Contains("table-cards\" id=\"holdings\"", shipments);
            Assert.Contains("data-label=", shipments);

            // ---- audit chain is intact after the whole run
            Assert.Contains("verify", await admin.PostFlashAsync("/Audit/Verify", new(), "/Audit/Index"));
        }
    }
}
