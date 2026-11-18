using System.Net;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Tests.Demo;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-88: control sheets. All batch CCPs (4–10) on one page, saved together or not at all with the checks of a single CCP; the
    // approver approves the sheet in one action, never an own entry (four-eyes per record), and each record gets its own audit entry.
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class ControlSheetTests
    {
        private readonly E2EApp _app;

        public ControlSheetTests(E2EApp app) => _app = app;

        private record Setup(int Plant, int Batch, List<int> Ccps, Dictionary<int, List<CriticalControlPointParameter>> Parameters);

        // a plant with an approved lot and a batch made from it through "New batch"
        private async Task<Setup> SeedAsync(string suffix)
        {
            var plant = await _app.CreateOrgAsync("zz_test CS Plant " + suffix, null, (Capabilities.Processing, null), (Capabilities.Packing, null));
            var other = await _app.CreateOrgAsync("zz_test CS Other " + suffix, null, (Capabilities.Processing, null), (Capabilities.Packing, null));
            await _app.AddMemberAsync(plant, await _app.CreateUserAsync("zz_test_cs_staff_" + suffix), OrgRoles.Staff);
            await _app.AddMemberAsync(plant, await _app.CreateUserAsync("zz_test_cs_qa_" + suffix), OrgRoles.QualityApprover);
            var dual = await _app.CreateUserAsync("zz_test_cs_dual_" + suffix);
            await _app.AddMemberAsync(plant, dual, OrgRoles.Staff);
            await _app.AddMemberAsync(plant, dual, OrgRoles.QualityApprover);
            await _app.AddMemberAsync(other, await _app.CreateUserAsync("zz_test_cs_other_" + suffix), OrgRoles.QualityApprover);

            var lot = await _app.WithContextAsync(async db =>
            {
                var bk = new Beekeeper
                {
                    BeekeeperId = Guid.NewGuid().ToString(), BeekeeperFirstName = "zz_test", BeekeeperLastName = "Beekeeper", BeekeeperBirthDate = new DateOnly(1980, 1, 1),
                    BeekeeperBusinessNumber = "ZZ-CS-" + suffix, IsActive = true
                };
                db.Beekeepers.Add(bk);
                await db.SaveChangesAsync();
                var ap = new Apiary { ApiaryBeekeeperId = bk.BeekeeperId, ApiaryTitle = "zz_test apiary", ApiaryBeehivesCount = 5, ApiaryQRCode = Guid.NewGuid() };
                db.Apiaries.Add(ap);
                await db.SaveChangesAsync();
                var r = new RawHoneyProduction
                {
                    RawHoneyProductionApiaryId = ap.ApiaryId, RawHoneyProductionAmount = 100, RawHoneyProductionDate = DateOnly.FromDateTime(DateTime.Today),
                    OwnerOrgId = plant, RawHoneyProductionHoneyType = await db.HoneyTypes.Select(h => h.HoneyTypeId).FirstAsync(),
                    RawHoneyProductionPeriodId = await db.RawHoneyProductionPeriods.Select(p => p.RawHoneyProductionPeriodId).FirstAsync(), OriginCountry = "TR"
                };
                db.RawHoneyProductions.Add(r);
                await db.SaveChangesAsync();
                foreach (var id in await db.CriticalControlPoints.Where(c => c.AppliesTo == CcpAppliesTo.RawLot).Select(c => c.Id).ToListAsync())
                    db.RawHoneyCCPAssignments.Add(new RawHoneyCCPAssignment
                    {
                        RawHoneyProductionId = r.RawHoneyProductionId, RawHoneyCCPAssignmentCCP = id, RawHoneyCCPAssignmentStatus = AssignmentStatusIds.Completed,
                        RawHoneyCCPAssignmentStartDate = DateTime.Now, ApprovedAt = DateTime.Now, ApprovedByUserId = "zz_test_seed"
                    });
                await db.SaveChangesAsync();
                return r.RawHoneyProductionId;
            });

            var staff = await UserClient.LoginAsync(_app, "zz_test_cs_staff_" + suffix);
            var created = await staff.PostPairsAsync("/OrgWorkspace/NewBatch", new[]
            {
                new KeyValuePair<string, string>("title", "zz_test CS batch " + suffix), new("date", DateTime.Today.ToString("yyyy-MM-dd")),
                new("lots", lot.ToString()), new($"kg[{lot}]", "50")
            }, "/OrgWorkspace/NewBatch");
            var batch = int.Parse(Regex.Match(created.Headers.Location?.ToString() ?? "", @"/OrgWorkspace/Batch/(\d+)").Groups[1].Value);
            var ccps = await _app.WithContextAsync(db => db.FinalHoneyCCPAssignments.Where(a => a.FinalHoneyProductionId == batch)
                .OrderBy(a => a.FinalHoneyCCPAssignmentCCP).Select(a => a.FinalHoneyCCPAssignmentCCP).ToListAsync());
            var parameters = await _app.WithContextAsync(db => db.CriticalControlPointParameters.AsNoTracking().Where(p => p.CCPId != null && ccps.Contains(p.CCPId.Value))
                .OrderBy(p => p.ParameterId).ToListAsync());
            return new Setup(plant, batch, ccps, parameters.GroupBy(p => p.CCPId!.Value).ToDictionary(g => g.Key, g => g.ToList()));
        }

        private static string Code(Setup s, int ccpId) => "CCP " + (s.Ccps.IndexOf(ccpId) + 4);

        // the sheet form for the given CCPs; value(p) gives the posted text ("" leaves the field empty); the rest of the CCPs are not posted
        private static List<KeyValuePair<string, string>> SheetForm(Setup s, IEnumerable<int> ccps, Func<int, CriticalControlPointParameter, string> value)
        {
            var pairs = new List<KeyValuePair<string, string>> { new("Kind", "batch"), new("Id", s.Batch.ToString()) };
            foreach (var ccp in ccps)
            {
                pairs.Add(new("Ccps.Index", ccp.ToString()));
                pairs.Add(new($"Ccps[{ccp}].CCPId", ccp.ToString()));
                var list = s.Parameters.TryGetValue(ccp, out var l) ? l : new();
                for (var i = 0; i < list.Count; i++)
                {
                    var p = list[i];
                    pairs.Add(new($"Ccps[{ccp}].Groups[0].Parameters[{i}].ParameterId", p.ParameterId.ToString()));
                    var v = value(ccp, p);
                    if (v.Length == 0)
                        continue;
                    pairs.Add(new(p.DataType == CcpDataType.Number ? $"Ccps[{ccp}].Groups[0].Parameters[{i}].MeasuredNumericValue" : $"Ccps[{ccp}].Groups[0].Parameters[{i}].MeasuredTextValue", v));
                }
            }
            return pairs;
        }

        // values within the limits (the partner test values)
        private static string Good(Setup s, int ccp, CriticalControlPointParameter p) => DemoValues.For(Code(s, ccp), p, DemoCountries.All.First(c => c.Code == "TR"));

        private string SheetUrl(Setup s) => $"/CCPMonitoring/Sheet?kind=batch&id={s.Batch}";

        [Fact]
        public async Task The_sheet_saves_everything_or_nothing_and_is_approved_in_one_action_with_four_eyes_per_record()
        {
            var suffix = Guid.NewGuid().ToString("N")[..6];
            var s = await SeedAsync(suffix);
            var staff = await UserClient.LoginAsync(_app, "zz_test_cs_staff_" + suffix);
            var qa = await UserClient.LoginAsync(_app, "zz_test_cs_qa_" + suffix);
            var dual = await UserClient.LoginAsync(_app, "zz_test_cs_dual_" + suffix);
            var url = SheetUrl(s);

            // one page with every batch CCP; the batch is a single item in My work, opening the sheet
            var page = await staff.HtmlAsync(url);
            foreach (var ccp in s.Ccps)
                Assert.Contains($"id=\"ccp-{ccp}\"", page);
            Assert.Contains("id=\"sheet-save\"", page);
            var work = await staff.HtmlAsync("/MyWork/Index");
            Assert.Contains($"kind=batch&amp;id={s.Batch}", work);
            Assert.DoesNotContain($"finalHoneyProductionId={s.Batch}&amp;ccpId=", work);

            // a wrong value in one CCP: nothing is saved, every problem is listed with its CCP
            var first = s.Ccps[0];
            var numberParam = s.Parameters[first].First(p => p.DataType == CcpDataType.Number);
            var bad = SheetForm(s, s.Ccps, (ccp, p) => Good(s, ccp, p));
            bad.Add(new($"Ccps[{first}].Groups[0].Parameters[{s.Parameters[first].IndexOf(numberParam)}].MeasuredTextValue", "a lot"));
            var refused = await staff.PostPairsAsync("/CCPMonitoring/Sheet", bad, url);
            Assert.Equal(HttpStatusCode.OK, refused.StatusCode);
            var refusedHtml = WebUtility.HtmlDecode(await refused.Content.ReadAsStringAsync());
            Assert.Contains("id=\"sheet-errors\"", refusedHtml);
            Assert.Contains($"CCP 4: {numberParam.ParameterName}: enter a number.", refusedHtml);
            Assert.Equal(0, await _app.WithContextAsync(db => db.CriticalControlPointMonitoringLogs.CountAsync(l => l.FinalHoneyProductionId == s.Batch)));

            // "Not applicable" follows the same rules: a reason is required
            var noReason = SheetForm(s, new[] { s.Ccps[^1] }, (_, _) => "");
            noReason.Add(new($"Ccps[{s.Ccps[^1]}].NotApplicable", "true"));
            Assert.Contains("A reason is required to mark a CCP as &quot;Not applicable&quot;.", await (await staff.PostPairsAsync("/CCPMonitoring/Sheet", noReason, url)).Content.ReadAsStringAsync());

            // the dual-role user enters CCP 4 as Staff; the staff member enters the others at once
            var dualStaff = await _app.WithContextAsync(db => db.Memberships.Where(m => m.OrgId == s.Plant && m.OrgRole == OrgRoles.Staff
                && db.Memberships.Count(x => x.UserId == m.UserId && x.OrgId == s.Plant) > 1).Select(m => m.MembershipId).FirstAsync());
            Assert.Equal(HttpStatusCode.Redirect, (await dual.PostAsync("/OrgWorkspace/Switch", new() { ["membershipId"] = dualStaff.ToString() }, "/OrgWorkspace/Index")).StatusCode);
            var dualCcp4 = SheetForm(s, new[] { first }, (ccp, p) => Good(s, ccp, p));
            Assert.Equal("Control sheet saved.", await dual.FlashAfterAsync(await dual.PostPairsAsync("/CCPMonitoring/Sheet", dualCcp4, url)));
            var rest = s.Ccps.Skip(1).ToList();
            Assert.Equal("Control sheet saved.", await staff.FlashAfterAsync(await staff.PostPairsAsync("/CCPMonitoring/Sheet", SheetForm(s, rest, (ccp, p) => Good(s, ccp, p)), url)));
            var statuses = await _app.WithContextAsync(db => db.FinalHoneyCCPAssignments.Where(a => a.FinalHoneyProductionId == s.Batch)
                .ToDictionaryAsync(a => a.FinalHoneyCCPAssignmentCCP, a => a.FinalHoneyCCPAssignmentStatus));
            Assert.All(s.Ccps, c => Assert.Equal(AssignmentStatusIds.Submitted, statuses[c]));

            // the dual user switches to the QualityApprover membership: everything but the own CCP 4 is approved, CCP 4 is listed with the reason
            var dualUser = await _app.WithContextAsync(db => db.Memberships.Where(m => m.OrgId == s.Plant).GroupBy(m => m.UserId).Where(g => g.Count() > 1).Select(g => g.Key).FirstAsync());
            var dualQa = await _app.WithContextAsync(db => db.Memberships.Where(m => m.UserId == dualUser && m.OrgRole == OrgRoles.QualityApprover).Select(m => m.MembershipId).FirstAsync());
            Assert.Equal(HttpStatusCode.Redirect, (await dual.PostAsync("/OrgWorkspace/Switch", new() { ["membershipId"] = dualQa.ToString() }, "/OrgWorkspace/Index")).StatusCode);
            var dualPage = WebUtility.HtmlDecode(await dual.HtmlAsync(url));
            Assert.Contains("id=\"sheet-approve-button\"", dualPage);
            Assert.Contains("Four-eyes: you recorded values of this CCP, so another person must approve it.", dualPage);
            var approve = new Dictionary<string, string> { ["kind"] = "batch", ["id"] = s.Batch.ToString(), ["confirmOutOfLimit"] = "true" };
            var response = await dual.PostAsync("/CCPMonitoring/ApproveSheet", new(approve), url);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            var approved = await _app.WithContextAsync(db => db.FinalHoneyCCPAssignments.Where(a => a.FinalHoneyProductionId == s.Batch)
                .ToDictionaryAsync(a => a.FinalHoneyCCPAssignmentCCP, a => (a.ApprovedAt, a.ApprovedByUserId, a.FinalHoneyCCPAssignmentStatus)));
            Assert.Null(approved[first].ApprovedAt);
            Assert.All(rest, c => Assert.Equal(dualUser, approved[c].ApprovedByUserId));
            Assert.All(rest, c => Assert.Equal(AssignmentStatusIds.Completed, approved[c].FinalHoneyCCPAssignmentStatus));

            // one audit entry per approved record, marked as approved on the control sheet
            var entries = await _app.WithContextAsync(db => db.AuditEntries.Where(e => e.Action == "Approve" && e.Entity == "FinalHoneyCCPAssignments"
                && e.EntityKey!.StartsWith(s.Batch + "/")).Select(e => new { e.EntityKey, e.Flags }).ToListAsync());
            Assert.Equal(rest.Select(c => $"{s.Batch}/{c}").OrderBy(k => k), entries.Select(e => e.EntityKey!).OrderBy(k => k));
            Assert.All(entries, e => Assert.Equal("ControlSheet", e.Flags));

            // the other QualityApprover approves the rest (CCP 4), and nothing is left to approve
            Assert.Equal("Approved: CCP 4.", await qa.PostFlashAsync("/CCPMonitoring/ApproveSheet", new(approve), url));
            Assert.Equal("Nothing on this sheet waits for your approval.", await qa.PostFlashAsync("/CCPMonitoring/ApproveSheet", new(approve), url));
            Assert.Empty(await _app.WithContextAsync(db => db.FinalHoneyCCPAssignments.Where(a => a.FinalHoneyProductionId == s.Batch && a.ApprovedAt == null).ToListAsync()));
            Assert.Empty((await _app.WithContextAsync(db => db.FinalHoneyCCPAssignments.Where(a => a.FinalHoneyProductionId == s.Batch).Select(a => a.ApprovedByUserId).ToListAsync()))
                .Where(u => u == null));

            // staff cannot approve; another organization cannot open the sheet; a missing record answers 404
            var staffApprove = await staff.PostFlashAsync("/CCPMonitoring/ApproveSheet", new(approve), url);
            Assert.Equal("Nothing on this sheet waits for your approval.", staffApprove);
            var other = await UserClient.LoginAsync(_app, "zz_test_cs_other_" + suffix);
            Assert.Equal(403, await other.StatusAsync(url));
            Assert.Equal(403, (int)(await other.PostAsync("/CCPMonitoring/ApproveSheet", new(approve), "/MyWork/Index")).StatusCode);
            Assert.Equal(404, await staff.StatusAsync("/CCPMonitoring/Sheet?kind=batch&id=999999"));
        }

        [Fact]
        public async Task Values_outside_their_limits_are_approved_only_when_confirmed()
        {
            var suffix = Guid.NewGuid().ToString("N")[..6];
            var s = await SeedAsync(suffix);
            var staff = await UserClient.LoginAsync(_app, "zz_test_cs_staff_" + suffix);
            var qa = await UserClient.LoginAsync(_app, "zz_test_cs_qa_" + suffix);
            var url = SheetUrl(s);
            // CCP 5 diastase below its limit (3 when HMF is at most 15); everything else within the limits
            var ccp5 = s.Ccps[1];
            var form = SheetForm(s, s.Ccps, (ccp, p) => ccp == ccp5 && p.ParameterName.Trim() == "Diastase" ? "1" : Good(s, ccp, p));
            Assert.Equal("Control sheet saved.", await staff.FlashAfterAsync(await staff.PostPairsAsync("/CCPMonitoring/Sheet", form, url)));
            Assert.True(await _app.WithContextAsync(db => db.CriticalControlPointMonitoringLogs.AnyAsync(l => l.FinalHoneyProductionId == s.Batch && l.CCPId == ccp5 && l.IsInRange == false)));

            var page = await qa.HtmlAsync(url);
            Assert.Contains("id=\"oolc-sheet-approve-form\"", page);
            var approve = new Dictionary<string, string> { ["kind"] = "batch", ["id"] = s.Batch.ToString(), ["confirmOutOfLimit"] = "false" };
            Assert.Equal("The sheet has values outside their limits; confirm them to approve.", await qa.PostFlashAsync("/CCPMonitoring/ApproveSheet", new(approve), url));
            Assert.Equal(0, await _app.WithContextAsync(db => db.FinalHoneyCCPAssignments.CountAsync(a => a.FinalHoneyProductionId == s.Batch && a.ApprovedAt != null)));
            approve["confirmOutOfLimit"] = "true";
            Assert.StartsWith("Approved: CCP 4, CCP 5", await qa.PostFlashAsync("/CCPMonitoring/ApproveSheet", new(approve), url));
            Assert.Equal(s.Ccps.Count, await _app.WithContextAsync(db => db.FinalHoneyCCPAssignments.CountAsync(a => a.FinalHoneyProductionId == s.Batch && a.ApprovedAt != null)));
        }
    }
}
