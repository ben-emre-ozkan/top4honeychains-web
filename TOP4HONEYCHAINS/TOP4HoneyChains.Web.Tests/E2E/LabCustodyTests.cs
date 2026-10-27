using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-78 P2 (D78-SPEC 4.6, D-81): the laboratory checks seal number, label and cap of every container jar; a rejected jar is
    // replaced by a new jar from the same container (the container's seal is broken again, visibly); in composite mode the lab
    // mixes the received jars into one composite whose approved report sets the lot's CCP 1 values, and a single jar's report
    // does not replace them; in per-container mode every jar is reported on its own.
    public partial class CustodyIntakeTests
    {
        private record Lab(UserClient Analyst, UserClient Manager);

        private async Task<Lab> LabUsersAsync(Plant p)
        {
            await _app.AddMemberAsync(p.LabId, await _app.CreateUserAsync($"zz_test_{p.Tag}_analyst"), OrgRoles.Analyst);
            await _app.AddMemberAsync(p.LabId, await _app.CreateUserAsync($"zz_test_{p.Tag}_labmgr"), OrgRoles.LabManager);
            return new Lab(await UserClient.LoginAsync(_app, $"zz_test_{p.Tag}_analyst"), await UserClient.LoginAsync(_app, $"zz_test_{p.Tag}_labmgr"));
        }

        // CCP 0 (primary audit) approved, so that lab results may be entered
        private Task<int> Ccp0DoneAsync(int lot) => _app.WithContextAsync(async db =>
        {
            var ccp0 = await db.CriticalControlPoints.Where(c => c.Code == "CCP 0").Select(c => c.Id).FirstAsync();
            var a = await db.RawHoneyCCPAssignments.FirstOrDefaultAsync(x => x.RawHoneyProductionId == lot && x.RawHoneyCCPAssignmentCCP == ccp0);
            if (a == null)
                db.RawHoneyCCPAssignments.Add(a = new RawHoneyCCPAssignment { RawHoneyProductionId = lot, RawHoneyCCPAssignmentCCP = ccp0, RawHoneyCCPAssignmentStartDate = DateTime.Now });
            a.RawHoneyCCPAssignmentStatus = AssignmentStatusIds.Completed;
            a.ApprovedAt = DateTime.UtcNow;
            a.ApprovedByUserId = "seed";
            return await db.SaveChangesAsync();
        });

        private static Dictionary<string, string> Receive(int sampleId, bool? seal, bool? label, bool? cap, string? note = null)
        {
            var f = new Dictionary<string, string> { ["id"] = sampleId.ToString(CultureInfo.InvariantCulture) };
            if (seal != null) f["sealMatches"] = seal.Value ? "true" : "false";
            if (label != null) f["labelIntact"] = label.Value ? "true" : "false";
            if (cap != null) f["capIntact"] = cap.Value ? "true" : "false";
            if (note != null) f["note"] = note;
            return f;
        }

        private async Task<string> ReportAsync(Lab lab, int sampleId)
        {
            var ccp1 = await _app.WithContextAsync(db => db.CriticalControlPoints.Where(c => c.Code == "CCP 1").Select(c => c.Id).FirstAsync());
            var parameters = await _app.WithContextAsync(db => db.CriticalControlPointParameters.Where(p => p.CCPId == ccp1 && p.DataType != CcpDataType.File).ToListAsync());
            var results = new Dictionary<string, string> { ["reportNumber"] = $"ZZ-{sampleId}", ["issuedOn"] = DateTime.Today.ToString("yyyy-MM-dd") };
            foreach (var p in parameters)
                results[$"values[{p.ParameterId}]"] = p.DataType == CcpDataType.Text ? "zz" : p.DataType == CcpDataType.YesNo ? "Yes" : "1";
            Assert.Contains("Lab results saved.", await lab.Analyst.FlashAfterAsync(await lab.Analyst.PostMultipartAsync($"/LabReports/Save/{sampleId}", results, Pdf.File("reportFile"), $"/LabReports/Edit/{sampleId}")));
            Assert.Contains("Report submitted", await lab.Analyst.PostFlashAsync($"/LabReports/Submit/{sampleId}", new(), $"/LabReports/Edit/{sampleId}"));
            return await lab.Manager.PostFlashAsync($"/LabReports/Decide/{sampleId}", new() { ["approve"] = "true", ["confirmOutOfLimit"] = "true" }, $"/LabReports/Edit/{sampleId}");
        }

        private Task<List<Sample>> JarsAsync(int lot) => _app.WithContextAsync(db => db.Samples.Include(s => s.Container).Where(s => s.RawHoneyProductionId == lot).OrderBy(s => s.SampleId).ToListAsync());

        private Task<List<int?>> Ccp1ReportIdsAsync(int lot) => _app.WithContextAsync(async db =>
        {
            var ccp1 = await db.CriticalControlPoints.Where(c => c.Code == "CCP 1").Select(c => c.Id).FirstAsync();
            return await db.CriticalControlPointMonitoringLogs.Where(l => l.RawHoneyProductionId == lot && l.CCPId == ccp1).Select(l => l.LabReportId).Distinct().ToListAsync();
        });

        [Fact]
        public async Task The_lab_checks_every_jar_a_rejected_jar_is_replaced_and_the_composite_sets_the_lot_values() => await WithCustodyAsync(async () =>
        {
            var p = await PlantAsync("ci6");
            var lab = await LabUsersAsync(p);
            var (intakeId, lot) = await ConfirmBySmsAsync(p, Form(p, 2));
            await Ccp0DoneAsync(lot);
            var jars = await JarsAsync(lot);
            int jar1 = jars[0].SampleId, jar2 = jars[1].SampleId;

            // reception: three answers required; a "no" needs a reason and rejects the jar
            Assert.Contains("Answer all three reception checks", await lab.Analyst.PostFlashAsync("/Samples/Receive", Receive(jar1, true, true, null), "/Samples/Index"));
            Assert.Contains("A failed reception check rejects the jar", await lab.Analyst.PostFlashAsync("/Samples/Receive", Receive(jar1, true, true, false), "/Samples/Index"));
            Assert.Contains("Jar rejected", await lab.Analyst.PostFlashAsync("/Samples/Receive", Receive(jar1, true, true, false, "zz_test cap popped"), "/Samples/Index"));
            Assert.Contains("Jar received", await lab.Analyst.PostFlashAsync("/Samples/Receive", Receive(jar2, true, true, true), "/Samples/Index"));
            var checkedJars = await JarsAsync(lot);
            Assert.Equal((SampleStatus.Rejected, (bool?)false, (bool?)true, (bool?)true), (checkedJars[0].Status, checkedJars[0].JarCapIntactAtReception, checkedJars[0].LabelIntactAtReception, checkedJars[0].SealNumberMatches));
            Assert.Equal(SampleStatus.Received, checkedJars[1].Status);

            // no composite while a container has no received jar; no single-jar results before the composite
            Assert.DoesNotContain("id=\"composite-ready\"", await lab.Analyst.HtmlAsync("/Samples/Index"));
            Assert.Contains("Every container's jar must be received", await lab.Analyst.PostFlashAsync("/Samples/Composite", new() { ["lotId"] = lot.ToString() }, "/Samples/Index"));
            Assert.Contains("analyses the composite of the lot's jars first", System.Net.WebUtility.HtmlDecode(await lab.Analyst.HtmlAsync($"/LabReports/Edit/{jar2}")));

            // the plant takes a new jar from container 1: only the holder, only after a rejection, with fresh seals
            var c1 = checkedJars[0].ContainerId!.Value;
            var c2 = checkedJars[1].ContainerId!.Value;
            Dictionary<string, string> Resample(int container, string jar, string reseal) => new()
            {
                ["containerId"] = container.ToString(), ["jarSeal"] = jar, ["counterSeal"] = jar + "K", ["capIntact"] = "true", ["reseal"] = reseal
            };
            Assert.Contains("Only the organization that holds the lot", await lab.Analyst.PostFlashAsync("/Samples/Resample", Resample(c1, "ci6-N1", "ci6-N2"), "/Samples/Index"));
            Assert.Contains("only when the container's jar was rejected", await p.Sao.PostFlashAsync("/Samples/Resample", Resample(c2, "ci6-N1", "ci6-N2"), $"/Intake/Details/{intakeId}"));
            Assert.Contains("Every seal number of the delivery must be different.", await p.Sao.PostFlashAsync("/Samples/Resample", Resample(c1, "ci6-J0", "ci6-N2"), $"/Intake/Details/{intakeId}"));
            Assert.Contains("class=\"border rounded p-2 mt-1 resample-form\"", await p.Sao.HtmlAsync($"/Intake/Details/{intakeId}"));
            Assert.Contains($"New jar S-{lot}-C1-2 sent", await p.Sao.PostFlashAsync("/Samples/Resample", Resample(c1, "ci6-N1", "ci6-N2"), $"/Intake/Details/{intakeId}"));
            var events = await _app.WithContextAsync(db => db.ContainerSealEvents.Where(e => e.ContainerId == c1).OrderBy(e => e.EventId).Select(e => e.Event + ":" + e.SealNumber).ToListAsync());
            Assert.Equal(new[] { "Checked:ci6-O0", "Broken:ci6-O0", "Applied:ci6-R0", "Broken:ci6-R0", "Applied:ci6-N2" }, events);
            using (var scope = _app.Services.CreateScope())
                Assert.True(await scope.ServiceProvider.GetRequiredService<CustodyIntakeService>().ContentMatchesAsync(intakeId));   // the confirmed content stays the proof
            var newJar = (await JarsAsync(lot)).Single(s => s.SampleCode == $"S-{lot}-C1-2");
            Assert.Equal((SampleKind.Container, (int?)c1, SampleStatus.Sent, p.LabId), (newJar.Kind, newJar.ContainerId, newJar.Status, newJar.LabOrgId));
            Assert.Contains($"S-{lot}-C1-2", await p.Sao.HtmlAsync($"/Intake/Labels/{intakeId}?size=a4"));

            // the composite of the received jars (the new jar of container 1 and the jar of container 2)
            Assert.Contains("Jar received", await lab.Analyst.PostFlashAsync("/Samples/Receive", Receive(newJar.SampleId, true, true, true), "/Samples/Index"));
            Assert.Contains("id=\"composite-ready\"", await lab.Analyst.HtmlAsync("/Samples/Index"));
            var made = await lab.Analyst.PostAsync("/Samples/Composite", new() { ["lotId"] = lot.ToString() }, "/Samples/Index");
            var composite = (await JarsAsync(lot)).Single(s => s.Kind == SampleKind.Composite);
            Assert.Equal($"/LabReports/Edit/{composite.SampleId}", made.Headers.Location?.ToString());
            Assert.Equal((SampleStatus.Received, p.OrgId, p.LabId, $"S-{lot}-COMP"), (composite.Status, composite.TakingOrgId, composite.LabOrgId, composite.SampleCode));
            var sources = await _app.WithContextAsync(db => db.SampleSources.Where(s => s.CompositeSampleId == composite.SampleId).Select(s => s.SourceSampleId).OrderBy(x => x).ToListAsync());
            Assert.Equal(new[] { jar2, newJar.SampleId }, sources);
            Assert.Contains("already made", await lab.Analyst.PostFlashAsync("/Samples/Composite", new() { ["lotId"] = lot.ToString() }, "/Samples/Index"));

            // the composite's report sets CCP 1; a single jar's report afterwards does not replace it
            Assert.Contains("the results are now the lot's CCP 1 values", await ReportAsync(lab, composite.SampleId));
            var compositeReport = await _app.WithContextAsync(db => db.LabReports.Where(r => r.SampleId == composite.SampleId).Select(r => r.ReportId).FirstAsync());
            Assert.Equal(new int?[] { compositeReport }, await Ccp1ReportIdsAsync(lot));
            Assert.Contains("id=\"jar-report-only\"", await lab.Analyst.HtmlAsync($"/LabReports/Edit/{jar2}"));
            Assert.Contains("stay those of the composite", await ReportAsync(lab, jar2));
            Assert.Equal(new int?[] { compositeReport }, await Ccp1ReportIdsAsync(lot));
            Assert.Equal(LabReportStatus.Approved, await _app.WithContextAsync(db => db.LabReports.Where(r => r.SampleId == jar2).Select(r => r.Status).FirstAsync()));
            return 0;
        });

        [Fact]
        public async Task In_per_container_mode_every_jar_is_reported_on_its_own() => await WithCustodyAsync(async () =>
        {
            var config = _app.Services.GetRequiredService<IConfiguration>();
            config["Lab:AnalysisMode"] = "PerContainer";
            try
            {
                var p = await PlantAsync("ci7");
                var lab = await LabUsersAsync(p);
                var (_, lot) = await ConfirmBySmsAsync(p, Form(p, 1));
                await Ccp0DoneAsync(lot);
                var jar = (await JarsAsync(lot)).Single();
                Assert.Contains("Jar received", await lab.Analyst.PostFlashAsync("/Samples/Receive", Receive(jar.SampleId, true, true, true), "/Samples/Index"));
                Assert.DoesNotContain("id=\"composite-ready\"", await lab.Analyst.HtmlAsync("/Samples/Index"));
                Assert.Contains("PerContainer", await lab.Analyst.PostFlashAsync("/Samples/Composite", new() { ["lotId"] = lot.ToString() }, "/Samples/Index"));
                Assert.Contains("the results are now the lot's CCP 1 values", await ReportAsync(lab, jar.SampleId));
                Assert.Single(await Ccp1ReportIdsAsync(lot));
                return 0;
            }
            finally
            {
                config["Lab:AnalysisMode"] = "Composite";
            }
        });

        [Fact]
        public async Task A_legacy_sample_is_still_received_with_the_single_seal_check()
        {
            var p = await PlantAsync("ci8");
            var lab = await LabUsersAsync(p);
            var sampleId = await _app.WithContextAsync(async db =>
            {
                var lot = new RawHoneyProduction
                {
                    RawHoneyProductionApiaryId = p.ApiaryId, RawHoneyProductionAmount = 30, RawHoneyProductionDate = new DateOnly(2026, 6, 1),
                    RawHoneyProductionHoneyType = p.HoneyTypeId, RawHoneyProductionPeriodId = p.PeriodId, OwnerOrgId = p.OrgId, OriginCountry = "TR"
                };
                db.RawHoneyProductions.Add(lot);
                await db.SaveChangesAsync();
                var s = new Sample
                {
                    SampleCode = $"S-{lot.RawHoneyProductionId}-1", RawHoneyProductionId = lot.RawHoneyProductionId, TakingOrgId = p.OrgId, LabOrgId = p.LabId, SealNumber = "ci8-S",
                    CounterSealNumber = "ci8-C", CounterLocation = "zz", RetainUntil = DateOnly.FromDateTime(DateTime.Today.AddYears(1)), TakenByUserId = "seed", TakenAt = DateTime.UtcNow
                };
                db.Samples.Add(s);
                await db.SaveChangesAsync();
                return s.SampleId;
            });
            Assert.Contains("class=\"d-flex gap-1 receive-form\"", await lab.Analyst.HtmlAsync("/Samples/Index"));
            Assert.Contains("seal is intact", await lab.Analyst.PostFlashAsync("/Samples/Receive", new() { ["id"] = sampleId.ToString(), ["sealIntact"] = "true" }, "/Samples/Index"));
            var s = await _app.WithContextAsync(db => db.Samples.FirstAsync(x => x.SampleId == sampleId));
            Assert.Equal((SampleStatus.Received, (string?)null, (bool?)null), (s.Status, s.Kind, s.SealNumberMatches));
        }
    }
}
