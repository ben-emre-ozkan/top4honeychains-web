using System.Net;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-98 (migration 034): the two paths after the laboratory (association: "ready for packing", CCP 2-6 not applicable and not
    // shown; company: CCP 2 onwards), returning a CCP record, expected yes/no answers, the optional laboratory values and the
    // final honey result of a blended batch.
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class D98PathTests
    {
        private readonly E2EApp _app;
        public D98PathTests(E2EApp app) => _app = app;

        private record Org(int Id, UserClient Staff, UserClient Qa, string StaffId, int Lab);

        private async Task<Org> OrgAsync(string tag, bool association)
        {
            var caps = association
                ? new[] { (Capabilities.Association, (string?)null), (Capabilities.Processing, null), (Capabilities.Packing, null) }
                : new[] { (Capabilities.Processing, (string?)null), (Capabilities.Packing, null) };
            var org = await _app.CreateOrgAsync($"zz_test d98 {tag}", null, caps);
            var lab = await _app.CreateOrgAsync($"zz_test d98 {tag} lab", null, (Capabilities.Laboratory, "zz-acc"));
            var staffId = await _app.CreateUserAsync($"zz_test_d98_{tag}_staff");
            await _app.AddMemberAsync(org, staffId, OrgRoles.Staff);
            await _app.AddMemberAsync(org, await _app.CreateUserAsync($"zz_test_d98_{tag}_qa"), OrgRoles.QualityApprover);
            return new Org(org, await UserClient.LoginAsync(_app, $"zz_test_d98_{tag}_staff"), await UserClient.LoginAsync(_app, $"zz_test_d98_{tag}_qa"), staffId, lab);
        }

        // a delivered lot: CCP 0 and 1 done and approved, CCP 2 and 3 waiting; with labReport an approved lab report of the lot
        private Task<int> LotAsync(Org o, bool labReport = true) => _app.WithContextAsync(async db =>
        {
            var bk = new Beekeeper
            {
                BeekeeperId = Guid.NewGuid().ToString(), BeekeeperFirstName = "zz_test", BeekeeperLastName = "D98", BeekeeperBirthDate = new DateOnly(1980, 1, 1),
                BeekeeperBusinessNumber = "ZZ-D98-" + Guid.NewGuid().ToString("N")[..8], IsActive = true
            };
            db.Beekeepers.Add(bk);
            await db.SaveChangesAsync();
            var ap = new Apiary { ApiaryBeekeeperId = bk.BeekeeperId, ApiaryTitle = "zz_test d98 apiary", ApiaryBeehivesCount = 5, ApiaryQRCode = Guid.NewGuid() };
            db.Apiaries.Add(ap);
            await db.SaveChangesAsync();
            var lot = new RawHoneyProduction
            {
                RawHoneyProductionApiaryId = ap.ApiaryId, RawHoneyProductionAmount = 100, RawHoneyProductionDate = DateOnly.FromDateTime(DateTime.Today),
                OwnerOrgId = o.Id, RawHoneyProductionHoneyType = await db.HoneyTypes.Select(h => h.HoneyTypeId).FirstAsync(),
                RawHoneyProductionPeriodId = await db.RawHoneyProductionPeriods.Select(p => p.RawHoneyProductionPeriodId).FirstAsync(), OriginCountry = "TR"
            };
            db.RawHoneyProductions.Add(lot);
            await db.SaveChangesAsync();
            foreach (var c in await db.CriticalControlPoints.Where(c => c.AppliesTo == CcpAppliesTo.RawLot).ToListAsync())
            {
                var early = c.Code is "CCP 0" or "CCP 1";
                db.RawHoneyCCPAssignments.Add(new RawHoneyCCPAssignment
                {
                    RawHoneyProductionId = lot.RawHoneyProductionId, RawHoneyCCPAssignmentCCP = c.Id, RawHoneyCCPAssignmentStartDate = DateTime.Now,
                    RawHoneyCCPAssignmentStatus = early ? AssignmentStatusIds.Completed : AssignmentStatusIds.Assigned,
                    ApprovedAt = early ? DateTime.Now : null, ApprovedByUserId = early ? "zz_test_seed" : null
                });
            }
            if (labReport)
            {
                var sample = new Sample
                {
                    SampleCode = $"S-{lot.RawHoneyProductionId}-1", RawHoneyProductionId = lot.RawHoneyProductionId, TakingOrgId = o.Id, LabOrgId = o.Lab,
                    SealNumber = "ZZ-" + Guid.NewGuid().ToString("N")[..8], CounterSealNumber = "ZZ-" + Guid.NewGuid().ToString("N")[..8], CounterLocation = "zz",
                    RetainUntil = DateOnly.FromDateTime(DateTime.Today.AddYears(1)), Status = SampleStatus.Received, TakenByUserId = "zz_test_seed", TakenAt = DateTime.UtcNow
                };
                db.Samples.Add(sample);
                await db.SaveChangesAsync();
                db.LabReports.Add(new LabReport
                {
                    SampleId = sample.SampleId, LabOrgId = o.Lab, ReportCode = "ZZ" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant(), Status = LabReportStatus.Approved,
                    EnteredByUserId = "zz_test_seed", ApprovedByUserId = "zz_test_seed2", ApprovedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow
                });
            }
            await db.SaveChangesAsync();
            return lot.RawHoneyProductionId;
        });

        private Task<int> CcpIdAsync(string code) => _app.WithContextAsync(db => db.CriticalControlPoints.Where(c => c.Code == code).Select(c => c.Id).FirstAsync());

        private static string CcpPage(int lot, int ccp) => $"/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId={lot}&ccpId={ccp}";

        private static async Task<int> NewBatchAsync(UserClient user, string title, params int[] lots)
        {
            var pairs = new List<KeyValuePair<string, string>> { new("title", title), new("date", DateTime.Today.ToString("yyyy-MM-dd")) };
            foreach (var lot in lots)
            {
                pairs.Add(new("lots", lot.ToString()));
                pairs.Add(new($"kg[{lot}]", "40"));
            }
            var created = await user.PostPairsAsync("/OrgWorkspace/NewBatch", pairs, "/OrgWorkspace/NewBatch");
            var m = Regex.Match(created.Headers.Location?.ToString() ?? "", @"/OrgWorkspace/Batch/(\d+)");
            Assert.True(m.Success, "the batch was not created: " + await user.FlashAfterAsync(created));
            return int.Parse(m.Groups[1].Value);
        }

        [Fact]
        public async Task The_association_path_goes_from_the_approved_lab_report_straight_to_packing()
        {
            var o = await OrgAsync("as", association: true);
            var lot = await LotAsync(o);
            var noReport = await LotAsync(o, labReport: false);
            int ccp1 = await CcpIdAsync("CCP 1"), ccp2 = await CcpIdAsync("CCP 2");

            // CCP 2 and 3 are not shown on the association path; the lot is not ready for a batch yet
            var workspace = await o.Staff.HtmlAsync("/OrgWorkspace/Index");
            Assert.DoesNotContain("data-ccp=\"CCP 2\"", workspace);
            Assert.Equal(403, await o.Staff.StatusAsync(CcpPage(lot, ccp2)));
            Assert.DoesNotContain($"id=\"use-{lot}\"", await o.Staff.HtmlAsync("/OrgWorkspace/NewBatch"));

            // only the Quality Assurance Officer, only after the approved lab report
            var page = WebUtility.HtmlDecode(await o.Qa.HtmlAsync(CcpPage(lot, ccp1)));
            Assert.Contains("id=\"packing-ready-button\"", page);
            Assert.Contains("Honey ready for packing", page);
            Assert.Contains("id=\"packing-ready-denied\"", await o.Staff.HtmlAsync(CcpPage(lot, ccp1)));
            Assert.Equal("Only the Quality Assurance Officer marks the honey ready for packing.",
                await o.Staff.PostFlashAsync("/CCPMonitoring/PackingReady", new() { ["lotId"] = lot.ToString(), ["ccpId"] = ccp1.ToString() }, CcpPage(lot, ccp1)));
            Assert.Equal("The lot's laboratory report is not approved yet.",
                await o.Qa.PostFlashAsync("/CCPMonitoring/PackingReady", new() { ["lotId"] = noReport.ToString(), ["ccpId"] = ccp1.ToString() }, CcpPage(noReport, ccp1)));
            Assert.Contains("Ready for packing?", WebUtility.HtmlDecode(await o.Qa.HtmlAsync("/MyWork/Index")));

            Assert.Equal("The honey is ready for packing: CCP 2 to 6 do not apply to this lot.",
                await o.Qa.PostFlashAsync("/CCPMonitoring/PackingReady", new() { ["lotId"] = lot.ToString(), ["ccpId"] = ccp1.ToString() }, CcpPage(lot, ccp1)));
            var (lotRow, skipped) = await _app.WithContextAsync(async db => (
                await db.RawHoneyProductions.FirstAsync(r => r.RawHoneyProductionId == lot),
                await db.RawHoneyCCPAssignments.Where(a => a.RawHoneyProductionId == lot && (a.CCP!.Code == "CCP 2" || a.CCP.Code == "CCP 3")).ToListAsync()));
            Assert.NotNull(lotRow.PackingReadyAt);
            Assert.Equal(2, skipped.Count);
            Assert.All(skipped, a => Assert.Equal((AssignmentStatusIds.NotApplicable, CcpPaths.Reason, true), (a.RawHoneyCCPAssignmentStatus, a.NotApplicableReason, a.ApprovedAt != null)));
            Assert.True(await _app.WithContextAsync(db => db.AuditEntries.AnyAsync(l => l.Action == "PackingReady" && l.EntityKey == lot.ToString())));
            Assert.Equal("The honey of this lot is already marked ready for packing.",
                await o.Qa.PostFlashAsync("/CCPMonitoring/PackingReady", new() { ["lotId"] = lot.ToString(), ["ccpId"] = ccp1.ToString() }, CcpPage(lot, ccp1)));

            // the batch: CCP 4 to 6 do not apply, are not shown and do not block finalizing
            var batch = await NewBatchAsync(o.Staff, "zz_test d98 association batch", lot);
            var batchCcps = await _app.WithContextAsync(db => db.FinalHoneyCCPAssignments.Where(a => a.FinalHoneyProductionId == batch).Select(a => new { a.CCP!.Code, a.FinalHoneyCCPAssignmentStatus, a.NotApplicableReason, a.ApprovedAt }).ToListAsync());
            Assert.All(batchCcps.Where(a => a.Code is "CCP 4" or "CCP 5" or "CCP 6"), a => Assert.Equal((AssignmentStatusIds.NotApplicable, CcpPaths.Reason, true), (a.FinalHoneyCCPAssignmentStatus, a.NotApplicableReason, a.ApprovedAt != null)));
            Assert.All(batchCcps.Where(a => a.Code is "CCP 7" or "CCP 8"), a => Assert.Equal(AssignmentStatusIds.Assigned, a.FinalHoneyCCPAssignmentStatus));
            var batchPage = await o.Staff.HtmlAsync($"/OrgWorkspace/Batch/{batch}");
            Assert.Contains(">CCP 7</a>", batchPage);
            Assert.DoesNotContain(">CCP 5</a>", batchPage);
            Assert.DoesNotContain("id=\"final-result\"", batchPage);   // no final honey result on the association path
            Assert.DoesNotContain(await BlockersAsync(batch), b => b.Contains("CCP 4") || b.Contains("CCP 5") || b.Contains("CCP 6"));
        }

        private async Task<List<string>> BlockersAsync(int batchId)
        {
            using var scope = _app.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<TOP4HoneyChains.Web.Services.CcpIntegrityService>().GetFinalizeBlockersAsync(batchId);
        }

        [Fact]
        public async Task The_company_path_keeps_ccp_2_and_the_platform_administrator_sets_the_path()
        {
            var o = await OrgAsync("co", association: false);
            var lot = await LotAsync(o);
            int ccp1 = await CcpIdAsync("CCP 1"), ccp2 = await CcpIdAsync("CCP 2");
            Assert.Equal(200, await o.Staff.StatusAsync(CcpPage(lot, ccp2)));
            Assert.DoesNotContain("id=\"packing-ready\"", await o.Qa.HtmlAsync(CcpPage(lot, ccp1)));
            Assert.Equal("Only organizations on the association path mark honey ready for packing.",
                await o.Qa.PostFlashAsync("/CCPMonitoring/PackingReady", new() { ["lotId"] = lot.ToString(), ["ccpId"] = ccp1.ToString() }, CcpPage(lot, ccp1)));

            // CCP 2: foam check required, heating time optional, temperature at most 45 °C (to verify), lab values optional
            var p2 = await _app.WithContextAsync(db => db.CriticalControlPointParameters.Where(p => p.CCPId == ccp2).ToListAsync());
            Assert.Equal((CcpDataType.YesNo, true, "Yes"), p2.Where(p => p.ParameterName == "Foam check").Select(p => (p.DataType!, p.IsRequired, p.CriticalLimit!)).Single());
            Assert.False(p2.Single(p => p.ParameterName == "Heating time").IsRequired);
            Assert.Equal((45m, false), p2.Where(p => p.ParameterName == "Temperature").Select(p => (p.MaxValue!.Value, p.LimitVerified)).Single());
            Assert.All(p2.Where(p => p.ParameterName is "HMF" or "Diastase" or "Proline" or "Moisture"), p => Assert.False(p.IsRequired));

            // the platform administrator switches the path; a wrong value is refused
            await _app.CreateUserAsync("zz_test_d98_admin", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "zz_test_d98_admin");
            Assert.Contains("id=\"ccp-path-card\"", await admin.HtmlAsync($"/Organizations/Details/{o.Id}"));
            Assert.Equal("Choose the association path or the company path.", await admin.PostFlashAsync($"/Organizations/SetCcpPath/{o.Id}", new() { ["path"] = "Other" }, $"/Organizations/Details/{o.Id}"));
            Assert.Equal("The path after the laboratory was saved.", await admin.PostFlashAsync($"/Organizations/SetCcpPath/{o.Id}", new() { ["path"] = CcpPaths.Association }, $"/Organizations/Details/{o.Id}"));
            Assert.Equal(CcpPaths.Association, await _app.WithContextAsync(db => db.Organizations.Where(x => x.OrgId == o.Id).Select(x => x.CcpPath).FirstAsync()));
            Assert.Contains("/Account/AccessDenied", ((await o.Qa.PostAsync($"/Organizations/SetCcpPath/{o.Id}", new() { ["path"] = "" }, "/MyWork/Index")).Headers.Location?.ToString() ?? ""));
        }

        [Fact]
        public async Task An_approver_sends_a_record_back_to_the_person_who_entered_it()
        {
            var o = await OrgAsync("rt", association: false);
            var lot = await LotAsync(o);
            var ccp2 = await CcpIdAsync("CCP 2");
            // the staff member's submitted CCP 2 record
            await _app.WithContextAsync(async db =>
            {
                foreach (var p in await db.CriticalControlPointParameters.Where(p => p.CCPId == ccp2 && p.IsRequired).ToListAsync())
                    db.CriticalControlPointMonitoringLogs.Add(new CriticalControlPointMonitoringLog
                    {
                        RawHoneyProductionId = lot, CCPId = ccp2, ParameterId = p.ParameterId, RecordedByUserId = o.StaffId, RecordedByOrgId = o.Id, MeasurementDate = DateTime.Now,
                        MeasuredNumericValue = p.DataType == CcpDataType.Number ? 40 : null, MeasuredTextValue = p.DataType == CcpDataType.YesNo ? "Yes" : null
                    });
                var a = await db.RawHoneyCCPAssignments.FirstAsync(x => x.RawHoneyProductionId == lot && x.RawHoneyCCPAssignmentCCP == ccp2);
                a.RawHoneyCCPAssignmentStatus = AssignmentStatusIds.Submitted;
                return await db.SaveChangesAsync();
            });
            var page = CcpPage(lot, ccp2);
            Assert.Contains("id=\"return-form\"", await o.Qa.HtmlAsync(page));
            Assert.DoesNotContain("id=\"return-form\"", await o.Staff.HtmlAsync(page));
            Assert.Equal("A reason is required to send the record back.", await o.Qa.PostFlashAsync("/CCPMonitoring/Return", new() { ["ownerKind"] = "raw", ["ownerId"] = lot.ToString(), ["ccpId"] = ccp2.ToString(), ["reason"] = " " }, page));
            Assert.Contains("cannot", await o.Staff.PostFlashAsync("/CCPMonitoring/Return", new() { ["ownerKind"] = "raw", ["ownerId"] = lot.ToString(), ["ccpId"] = ccp2.ToString(), ["reason"] = "zz" }, page));
            Assert.Equal("The record was sent back to the person who entered it.",
                await o.Qa.PostFlashAsync("/CCPMonitoring/Return", new() { ["ownerKind"] = "raw", ["ownerId"] = lot.ToString(), ["ccpId"] = ccp2.ToString(), ["reason"] = "zz_test the thermometer reading looks wrong" }, page));
            var a = await _app.WithContextAsync(db => db.RawHoneyCCPAssignments.AsNoTracking().FirstAsync(x => x.RawHoneyProductionId == lot && x.RawHoneyCCPAssignmentCCP == ccp2));
            Assert.Equal((AssignmentStatusIds.Rejected, "zz_test the thermometer reading looks wrong"), (a.RawHoneyCCPAssignmentStatus, a.ReturnReason));
            Assert.True(await _app.WithContextAsync(db => db.AuditEntries.AnyAsync(l => l.Action == "Return" && l.EntityKey == $"{lot}/{ccp2}")));
            Assert.Equal("Only a submitted record can be sent back.", await o.Qa.PostFlashAsync("/CCPMonitoring/Return", new() { ["ownerKind"] = "raw", ["ownerId"] = lot.ToString(), ["ccpId"] = ccp2.ToString(), ["reason"] = "again" }, page));

            // it is in the enterer's My work (not the approver's), with the reason on the page
            var work = WebUtility.HtmlDecode(await o.Staff.HtmlAsync("/MyWork/Index"));
            Assert.Contains("Sent back to you", work);
            Assert.DoesNotContain("Sent back to you", WebUtility.HtmlDecode(await o.Qa.HtmlAsync("/MyWork/Index")));
            Assert.Contains("zz_test the thermometer reading looks wrong", WebUtility.HtmlDecode(await o.Staff.HtmlAsync(page)));
            Assert.Contains("id=\"ccp-returned\"", await o.Staff.HtmlAsync(page));

            // saving it again submits it again
            var parameters = await _app.WithContextAsync(db => db.CriticalControlPointParameters.Where(p => p.CCPId == ccp2).OrderBy(p => p.ParameterId).ToListAsync());
            var form = new List<KeyValuePair<string, string>> { new("CCPId", ccp2.ToString()), new("RawHoneyProductionId", lot.ToString()) };
            for (var i = 0; i < parameters.Count; i++)
            {
                form.Add(new($"Groups[0].Parameters[{i}].ParameterId", parameters[i].ParameterId.ToString()));
                if (parameters[i].DataType == CcpDataType.YesNo)
                    form.Add(new($"Groups[0].Parameters[{i}].MeasuredTextValue", "Yes"));
                else if (parameters[i].IsRequired)
                    form.Add(new($"Groups[0].Parameters[{i}].MeasuredNumericValue", "41"));
            }
            await o.Staff.PostPairsAsync("/CCPMonitoring/RawHoneyProductionManageCCP", form, page);
            Assert.Equal(AssignmentStatusIds.Submitted, await _app.WithContextAsync(db => db.RawHoneyCCPAssignments.Where(x => x.RawHoneyProductionId == lot && x.RawHoneyCCPAssignmentCCP == ccp2).Select(x => x.RawHoneyCCPAssignmentStatus).FirstAsync()));
        }

        [Fact]
        public async Task Yes_no_questions_have_an_expected_answer_and_six_lab_values_are_optional()
        {
            var parameters = await _app.WithContextAsync(db => db.CriticalControlPointParameters.Include(p => p.CCP).ToListAsync());
            var residue = parameters.Single(p => p.CCP!.Code == "CCP 7" && p.ParameterName!.StartsWith("Is there any physical residue in the jar"));
            var dark = parameters.Single(p => p.CCP!.Code == "CCP 10" && p.ParameterName == "Are the jars kept in the dark?");
            Assert.Equal(("No", true), (residue.CriticalLimit, residue.IsRequired));
            Assert.Equal(("Yes", true), (dark.CriticalLimit, dark.IsRequired));
            Assert.Equal((false, "expected No"), (Web.Services.CcpLimitEvaluator.EvaluateAnswer(residue, "Yes").IsInRange!.Value, Web.Services.CcpLimitEvaluator.EvaluateAnswer(residue, "Yes").AppliedLimit));
            Assert.True(Web.Services.CcpLimitEvaluator.EvaluateAnswer(dark, "Yes").IsInRange);
            Assert.Null(Web.Services.CcpLimitEvaluator.EvaluateAnswer(dark, null).IsInRange);
            Assert.Equal("Clarification", await _app.WithContextAsync(db => db.CriticalControlPoints.Where(c => c.Code == "CCP 6").Select(c => c.Title).FirstAsync()));
            foreach (var name in new[] { "Naphthalene", "Sensory Analysis", "C4 Sugar", "Pesticides", "Antibiotics", "Maltoz" })
                Assert.False(parameters.Single(p => p.CCP!.Code == "CCP 1" && p.ParameterName == name).IsRequired, name);
            Assert.True(parameters.Single(p => p.CCP!.Code == "CCP 1" && p.ParameterName == "HMF").IsRequired);
        }

        [Fact]
        public async Task A_blended_company_batch_gets_a_final_honey_result_apart_from_the_lot_results()
        {
            var o = await OrgAsync("fr", association: false);
            await CustodyIntakeTests.SealPoolAsync(_app, o.Id, SealKind.Label, new[] { "FR-J1", "FR-K1", "FR-J2", "FR-K2" });
            var other = await _app.CreateOrgAsync("zz_test d98 fr other", null, (Capabilities.Processing, null));
            await CustodyIntakeTests.SealPoolAsync(_app, other, SealKind.Label, new[] { "FRX-J1" });
            await _app.AddMemberAsync(o.Id, await _app.CreateUserAsync("zz_test_d98_fr_sao"), OrgRoles.SampleAcceptanceOfficer);
            var sao = await UserClient.LoginAsync(_app, "zz_test_d98_fr_sao");
            var single = await NewBatchAsync(o.Staff, "zz_test d98 single", await ApprovedLotAsync(o));
            var blend = await NewBatchAsync(o.Staff, "zz_test d98 blend", await ApprovedLotAsync(o), await ApprovedLotAsync(o));

            // only for honey from more than one apiary; the finalize card warns
            Assert.DoesNotContain("id=\"final-result\"", await sao.HtmlAsync($"/OrgWorkspace/Batch/{single}"));
            var page = await sao.HtmlAsync($"/OrgWorkspace/Batch/{blend}");
            Assert.Contains("id=\"final-sample-form\"", page);
            Assert.Contains("id=\"final-warning\"", page);
            Dictionary<string, string> Form(string seal, string counter) => new()
            {
                ["labOrgId"] = o.Lab.ToString(), ["sealNumber"] = seal, ["counterSealNumber"] = counter, ["counterLocation"] = "zz_test shelf",
                ["retainUntil"] = DateTime.Today.AddYears(1).ToString("yyyy-MM-dd")
            };
            var url = $"/OrgWorkspace/FinalSample/{blend}";
            Assert.Equal("Seal FRX-J1 is not in the seal pool of your organization.", await sao.PostFlashAsync(url, Form("frx-j1", "FR-K1"), $"/OrgWorkspace/Batch/{blend}"));
            Assert.Equal("A final honey result is taken on the company path for a batch with honey from more than one apiary.",
                await sao.PostFlashAsync($"/OrgWorkspace/FinalSample/{single}", Form("FR-J2", "FR-K2"), $"/OrgWorkspace/Batch/{single}"));
            Assert.Equal("Sample S-B" + blend + "-1 sent to the laboratory.", await sao.PostFlashAsync(url, Form($"https://example.invalid/s/fr-j1", "FR-K1"), $"/OrgWorkspace/Batch/{blend}"));
            Assert.Equal("A sample of this batch is already at the laboratory.", await sao.PostFlashAsync(url, Form("FR-J2", "FR-K2"), $"/OrgWorkspace/Batch/{blend}"));
            var sample = await _app.WithContextAsync(db => db.Samples.AsNoTracking().SingleAsync(s => s.FinalHoneyProductionId == blend));
            Assert.Equal((SampleKind.Batch, "FR-J1"), (sample.Kind, sample.SealNumber));
            Assert.Equal(SealStatus.Used, await _app.WithContextAsync(db => db.SealCodes.Where(c => c.Code == "FR-J1").Select(c => c.Status).FirstAsync()));

            // the laboratory approves it: the batch shows the final honey result; the lot keeps its own CCP 1 values
            var lotLogsBefore = await _app.WithContextAsync(db => db.CriticalControlPointMonitoringLogs.CountAsync(l => l.RawHoneyProductionId == sample.RawHoneyProductionId));
            var report = await _app.WithContextAsync(async db =>
            {
                var hmf = await db.CriticalControlPointParameters.Where(p => p.CCP!.Code == "CCP 1" && p.ParameterName == "HMF").Select(p => p.ParameterId).FirstAsync();
                var r = new LabReport
                {
                    SampleId = sample.SampleId, LabOrgId = o.Lab, ReportCode = "ZZFR" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(), Status = LabReportStatus.Approved,
                    EnteredByUserId = "zz_test_seed", ApprovedByUserId = "zz_test_seed2", ApprovedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow
                };
                r.Results.Add(new LabResult { ParameterId = hmf, MeasuredNumericValue = 12.5m, IsInRange = true, AppliedLimit = "max 40" });
                db.LabReports.Add(r);
                await db.SaveChangesAsync();
                return r.ReportCode;
            });
            page = await sao.HtmlAsync($"/OrgWorkspace/Batch/{blend}");
            Assert.Contains("id=\"final-report\"", page);
            Assert.Contains(report, page);
            Assert.DoesNotContain("id=\"final-warning\"", page);
            Assert.Equal(lotLogsBefore, await _app.WithContextAsync(db => db.CriticalControlPointMonitoringLogs.CountAsync(l => l.RawHoneyProductionId == sample.RawHoneyProductionId)));
        }

        // a lot of a company with every lot CCP done and approved (ready for a batch)
        private async Task<int> ApprovedLotAsync(Org o)
        {
            var lot = await LotAsync(o);
            await _app.WithContextAsync(async db =>
            {
                foreach (var a in await db.RawHoneyCCPAssignments.Where(a => a.RawHoneyProductionId == lot).ToListAsync())
                {
                    a.RawHoneyCCPAssignmentStatus = AssignmentStatusIds.Completed;
                    a.ApprovedAt = DateTime.Now;
                    a.ApprovedByUserId = "zz_test_seed";
                }
                return await db.SaveChangesAsync();
            });
            return lot;
        }
    }
}
