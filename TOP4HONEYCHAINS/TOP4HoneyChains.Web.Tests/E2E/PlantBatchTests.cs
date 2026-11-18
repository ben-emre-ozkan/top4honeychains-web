using System.Net;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-88: "New batch" of the processing or packing organization, without the platform administrator: only own lots whose
    // CCPs are all done and approved and that have honey left; the batch belongs to the organization, gets its legacy
    // wholesaler record automatically and every batch CCP of the responsibility matrix; other organizations see nothing.
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class PlantBatchTests
    {
        private readonly E2EApp _app;

        public PlantBatchTests(E2EApp app) => _app = app;

        // a delivered lot owned by the organization; every lot CCP completed, approved unless told otherwise
        private Task<int> LotAsync(int ownerOrgId, bool allApproved = true, decimal kg = 100) => _app.WithContextAsync(async db =>
        {
            var honey = await db.HoneyTypes.Select(h => h.HoneyTypeId).FirstAsync();
            var period = await db.RawHoneyProductionPeriods.Select(p => p.RawHoneyProductionPeriodId).FirstAsync();
            var beekeeper = new Beekeeper
            {
                BeekeeperId = Guid.NewGuid().ToString(), BeekeeperFirstName = "zz_test", BeekeeperLastName = "Beekeeper", BeekeeperBirthDate = new DateOnly(1980, 1, 1),
                BeekeeperBusinessNumber = "ZZ-PB-" + Guid.NewGuid().ToString("N")[..8], IsActive = true
            };
            db.Beekeepers.Add(beekeeper);
            await db.SaveChangesAsync();
            var apiary = new Apiary { ApiaryBeekeeperId = beekeeper.BeekeeperId, ApiaryTitle = "zz_test apiary", ApiaryBeehivesCount = 10, ApiaryQRCode = Guid.NewGuid() };
            db.Apiaries.Add(apiary);
            await db.SaveChangesAsync();
            var lot = new RawHoneyProduction
            {
                RawHoneyProductionApiaryId = apiary.ApiaryId,
                RawHoneyProductionAmount = kg, RawHoneyProductionDate = DateOnly.FromDateTime(DateTime.Today), OwnerOrgId = ownerOrgId,
                RawHoneyProductionHoneyType = honey, RawHoneyProductionPeriodId = period, OriginCountry = "TR"
            };
            db.RawHoneyProductions.Add(lot);
            await db.SaveChangesAsync();
            var ccps = await db.CriticalControlPoints.Where(c => c.AppliesTo == CcpAppliesTo.RawLot).OrderBy(c => c.Id).Select(c => c.Id).ToListAsync();
            for (var i = 0; i < ccps.Count; i++)
                db.RawHoneyCCPAssignments.Add(new RawHoneyCCPAssignment
                {
                    RawHoneyProductionId = lot.RawHoneyProductionId, RawHoneyCCPAssignmentCCP = ccps[i], RawHoneyCCPAssignmentStatus = AssignmentStatusIds.Completed,
                    RawHoneyCCPAssignmentStartDate = DateTime.Now, ApprovedAt = allApproved || i < ccps.Count - 1 ? DateTime.Now : null,
                    ApprovedByUserId = allApproved || i < ccps.Count - 1 ? "zz_test_seed" : null
                });
            await db.SaveChangesAsync();
            return lot.RawHoneyProductionId;
        });

        private static async Task<HttpResponseMessage> PostNewBatchAsync(UserClient user, string title, params (int Lot, string Kg)[] lots)
        {
            var pairs = new List<KeyValuePair<string, string>> { new("title", title), new("date", DateTime.Today.ToString("yyyy-MM-dd")) };
            foreach (var (lot, kg) in lots)
            {
                pairs.Add(new("lots", lot.ToString()));
                pairs.Add(new($"kg[{lot}]", kg));
            }
            return await user.PostPairsAsync("/OrgWorkspace/NewBatch", pairs, "/OrgWorkspace/NewBatch");
        }

        [Fact]
        public async Task The_plant_creates_its_own_batch_from_eligible_lots()
        {
            var plant = await _app.CreateOrgAsync("zz_test PB Plant", null, (Capabilities.Processing, null), (Capabilities.Packing, null));
            var other = await _app.CreateOrgAsync("zz_test PB Other plant", null, (Capabilities.Processing, null), (Capabilities.Packing, null));
            var shop = await _app.CreateOrgAsync("zz_test PB Shop", null, (Capabilities.Retail, null));
            foreach (var (org, role, user) in new[]
            {
                (plant, OrgRoles.Staff, "zz_test_pb_staff"), (plant, OrgRoles.QualityApprover, "zz_test_pb_qa"),
                (other, OrgRoles.Staff, "zz_test_pb_other"), (shop, OrgRoles.Staff, "zz_test_pb_shop")
            })
                await _app.AddMemberAsync(org, await _app.CreateUserAsync(user), role);

            var ready = await LotAsync(plant);
            var notApproved = await LotAsync(plant, allApproved: false);
            var foreign = await LotAsync(other);
            Assert.Null(await _app.WithContextAsync(db => db.Organizations.Where(o => o.OrgId == plant).Select(o => o.LegacyWholesalerId).FirstAsync()));

            var staff = await UserClient.LoginAsync(_app, "zz_test_pb_staff");
            // eligibility: only the plant's own lot whose CCPs are all approved
            var page = await staff.HtmlAsync("/OrgWorkspace/NewBatch");
            Assert.Contains($"data-lot-id=\"{ready}\"", page);
            Assert.DoesNotContain($"data-lot-id=\"{notApproved}\"", page);
            Assert.DoesNotContain($"data-lot-id=\"{foreign}\"", page);
            Assert.Contains("id=\"link-new-batch\"", await staff.HtmlAsync("/OrgWorkspace/Index"));
            // the lots ready for a batch are in My work
            Assert.Contains("/OrgWorkspace/NewBatch", await staff.HtmlAsync("/MyWork/Index"));

            // a posted foreign or unapproved lot, or more than is left, creates nothing
            var before = await _app.WithContextAsync(db => db.FinalHoneyProductions.CountAsync());
            foreach (var (lot, kg) in new[] { (foreign, "10"), (notApproved, "10"), (ready, "100.5") })
            {
                var response = await PostNewBatchAsync(staff, "zz_test refused batch", (lot, kg));
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains("id=\"new-batch-problems\"", await response.Content.ReadAsStringAsync());
            }
            Assert.Equal(before, await _app.WithContextAsync(db => db.FinalHoneyProductions.CountAsync()));

            // the batch: owned by the plant, legacy wholesaler record created and linked, every batch CCP of the matrix assigned
            var created = await PostNewBatchAsync(staff, "zz_test Chestnut 2026/1", (ready, "60"));
            Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);
            var batchId = int.Parse(Regex.Match(created.Headers.Location?.ToString() ?? "", @"/OrgWorkspace/Batch/(\d+)").Groups[1].Value);
            var batch = await _app.WithContextAsync(db => db.FinalHoneyProductions.AsNoTracking().Include(f => f.FinalHoneyProductionDetails).Include(f => f.FinalHoneyCCPAssignments)
                .FirstAsync(f => f.FinalHoneyProductionId == batchId));
            var legacy = await _app.WithContextAsync(db => db.Organizations.Where(o => o.OrgId == plant).Select(o => o.LegacyWholesalerId).FirstAsync());
            Assert.Equal(plant, batch.OwnerOrgId);
            Assert.NotNull(legacy);
            Assert.Equal(legacy, batch.FinalHoneyProductionWholesalerId);
            Assert.Equal(60m, batch.FinalHoneyProductionTotalAmount);
            Assert.Equal(60m, Assert.Single(batch.FinalHoneyProductionDetails).UsedAmount);
            var expected = await _app.WithContextAsync(db => db.CriticalControlPoints
                .Where(c => c.AppliesTo == CcpAppliesTo.Batch && db.CcpResponsibilities.Any(r => r.CcpId == c.Id)).Select(c => c.Id).OrderBy(i => i).ToListAsync());
            Assert.NotEmpty(expected);
            Assert.Equal(expected, batch.FinalHoneyCCPAssignments.Select(a => a.FinalHoneyCCPAssignmentCCP).OrderBy(i => i).ToList());
            Assert.All(batch.FinalHoneyCCPAssignments, a => Assert.Equal(AssignmentStatusIds.Assigned, a.FinalHoneyCCPAssignmentStatus));
            Assert.False(batch.FinalHoneyProductionIsFinalized);
            // D-88: the batch's CCPs are one My work item that opens the control sheet
            Assert.Contains($"/CCPMonitoring/Sheet?kind=batch&amp;id={batchId}", await staff.HtmlAsync("/MyWork/Index"));

            // 40 kg are left of the lot, so it is offered again with that amount
            Assert.Contains("value=\"40\"", await staff.HtmlAsync("/OrgWorkspace/NewBatch"));

            // other organizations: no access to the batch, its CCPs or the plant's lots
            var otherStaff = await UserClient.LoginAsync(_app, "zz_test_pb_other");
            Assert.Equal(404, await otherStaff.StatusAsync($"/OrgWorkspace/Batch/{batchId}"));
            Assert.Equal(403, await otherStaff.StatusAsync($"/CCPMonitoring/FinalHoneyProductionManageCCP?finalHoneyProductionId={batchId}&ccpId={expected[0]}"));
            Assert.DoesNotContain($"data-lot-id=\"{ready}\"", await otherStaff.HtmlAsync("/OrgWorkspace/NewBatch"));
            var stolen = await PostNewBatchAsync(otherStaff, "zz_test stolen", (ready, "10"));
            Assert.Contains("id=\"new-batch-problems\"", await stolen.Content.ReadAsStringAsync());

            // roles and organizations without the right
            var qa = await UserClient.LoginAsync(_app, "zz_test_pb_qa");
            Assert.Equal(HttpStatusCode.Redirect, (await qa.GetAsync("/OrgWorkspace/NewBatch")).StatusCode);
            Assert.DoesNotContain("id=\"link-new-batch\"", await qa.HtmlAsync("/OrgWorkspace/Index"));
            var shopStaff = await UserClient.LoginAsync(_app, "zz_test_pb_shop");
            Assert.Equal(HttpStatusCode.Redirect, (await shopStaff.GetAsync("/OrgWorkspace/NewBatch")).StatusCode);
            var shopPost = await shopStaff.PostPairsAsync("/OrgWorkspace/NewBatch",
                new[] { new KeyValuePair<string, string>("title", "zz_test shop"), new("date", DateTime.Today.ToString("yyyy-MM-dd")), new("lots", ready.ToString()), new($"kg[{ready}]", "5") },
                "/OrgWorkspace/Index");
            Assert.Equal("Only a processing or packing organization creates batches.", await shopStaff.FlashAfterAsync(shopPost));
            Assert.Equal(1, await _app.WithContextAsync(db => db.FinalHoneyProductions.CountAsync(f => f.OwnerOrgId == plant)));
        }

        [Fact]
        public async Task A_new_processing_organization_gets_its_legacy_wholesaler_record()
        {
            await _app.CreateUserAsync("zz_test_pb_admin", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "zz_test_pb_admin");
            var response = await admin.PostPairsAsync("/Organizations/Create", new[]
            {
                new KeyValuePair<string, string>("DisplayName", "zz_test PB New packer"), new("capabilities", Capabilities.Packing)
            }, "/Organizations/Create");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            var (legacy, title) = await _app.WithContextAsync(async db =>
            {
                var org = await db.Organizations.FirstAsync(o => o.DisplayName == "zz_test PB New packer");
                return (org.LegacyWholesalerId, org.LegacyWholesalerId == null ? null : await db.Wholesalers.Where(w => w.WholesalerId == org.LegacyWholesalerId).Select(w => w.WholesalerTitle).FirstAsync());
            });
            Assert.NotNull(legacy);
            Assert.Equal("zz_test PB New packer", title);

            // a laboratory gets none
            await admin.PostPairsAsync("/Organizations/Create", new[]
            {
                new KeyValuePair<string, string>("DisplayName", "zz_test PB New lab"), new("capabilities", Capabilities.Laboratory)
            }, "/Organizations/Create");
            Assert.Null(await _app.WithContextAsync(db => db.Organizations.Where(o => o.DisplayName == "zz_test PB New lab").Select(o => o.LegacyWholesalerId).FirstAsync()));
        }
    }
}
