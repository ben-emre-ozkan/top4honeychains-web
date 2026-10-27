using System.Net;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // P3 follow-up (D-84): the Compliance Officer's approval checks cover batches too: batch CCPs, finalizing and CCP 10 storage
    public partial class CustodyIntakeTests
    {
        [Fact]
        public async Task The_compliance_officer_sees_stale_and_self_approved_batch_records_and_finalizing()
        {
            var p = await PlantAsync("cobatch");
            var other = await PlantAsync("cobatch2");
            var old = DateTime.Now.AddDays(-60);
            var ids = await _app.WithContextAsync(async db =>
            {
                int Ccp(string code) => db.CriticalControlPoints.Where(c => c.Code == code).Select(c => c.Id).First();
                var w = new Wholesaler { WholesalerTitle = "zz_test cobatch wholesaler", WholesalerIsActive = true, CreatedAt = DateTime.Now };
                db.Wholesalers.Add(w);
                await db.SaveChangesAsync();
                FinalHoneyProduction Batch(int owner, string title) => new()
                {
                    FinalHoneyProductionTitle = title, FinalHoneyProductionDate = DateTime.Today, FinalHoneyProductionTotalAmount = 10,
                    FinalHoneyProductionQR = Guid.NewGuid(), OwnerOrgId = owner, FinalHoneyProductionWholesalerId = w.WholesalerId
                };
                var staleCcp = Batch(p.OrgId, "zz_test cobatch stale ccp");
                var selfCcp = Batch(p.OrgId, "zz_test cobatch self ccp");
                var waiting = Batch(p.OrgId, "zz_test cobatch finalize waiting");
                waiting.FinalizeRequestedAt = old;
                waiting.FinalizeRequestedByUserId = "seed";
                var selfFinal = Batch(p.OrgId, "zz_test cobatch self finalized");
                selfFinal.FinalHoneyProductionIsFinalized = true;
                selfFinal.SelfFinalized = true;
                selfFinal.FinalizeRequestedAt = DateTime.Now;
                var held = Batch(other.OrgId, "zz_test cobatch held");
                var foreign = Batch(other.OrgId, "zz_test cobatch other org");
                db.FinalHoneyProductions.AddRange(staleCcp, selfCcp, waiting, selfFinal, held, foreign);
                await db.SaveChangesAsync();
                db.FinalHoneyCCPAssignments.AddRange(
                    new FinalHoneyCCPAssignment { FinalHoneyCCPAssignmentCCP = Ccp("CCP 4"), FinalHoneyProductionId = staleCcp.FinalHoneyProductionId, FinalHoneyCCPAssignmentStartDate = old, FinalHoneyCCPAssignmentStatus = AssignmentStatusIds.Submitted },
                    new FinalHoneyCCPAssignment { FinalHoneyCCPAssignmentCCP = Ccp("CCP 7"), FinalHoneyProductionId = selfCcp.FinalHoneyProductionId, FinalHoneyCCPAssignmentStartDate = DateTime.Now,
                        FinalHoneyCCPAssignmentStatus = AssignmentStatusIds.Completed, SelfApproved = true, ApprovedByOrgId = p.OrgId, ApprovedAt = DateTime.Now },
                    // recent and waiting: not stale, not flagged
                    new FinalHoneyCCPAssignment { FinalHoneyCCPAssignmentCCP = Ccp("CCP 5"), FinalHoneyProductionId = selfCcp.FinalHoneyProductionId, FinalHoneyCCPAssignmentStartDate = DateTime.Now, FinalHoneyCCPAssignmentStatus = AssignmentStatusIds.Submitted },
                    // another organization's self approval: not this officer's
                    new FinalHoneyCCPAssignment { FinalHoneyCCPAssignmentCCP = Ccp("CCP 7"), FinalHoneyProductionId = foreign.FinalHoneyProductionId, FinalHoneyCCPAssignmentStartDate = DateTime.Now,
                        FinalHoneyCCPAssignmentStatus = AssignmentStatusIds.Completed, SelfApproved = true, ApprovedByOrgId = other.OrgId, ApprovedAt = DateTime.Now });
                // the plant holds a batch of another organization and approved its storage record itself
                db.HolderStorageRecords.Add(new HolderStorageRecord { FinalHoneyProductionId = held.FinalHoneyProductionId, HolderOrgId = p.OrgId, CcpId = Ccp("CCP 10"),
                    Status = AssignmentStatusIds.Completed, HeldQuantity = 5, RecordedByUserId = "seed", RecordedAt = DateTime.Now, ApprovedByUserId = "seed", ApprovedAt = DateTime.Now, SelfApproved = true });
                await db.SaveChangesAsync();
                return new[] { staleCcp.FinalHoneyProductionId, selfCcp.FinalHoneyProductionId, waiting.FinalHoneyProductionId, selfFinal.FinalHoneyProductionId,
                               held.FinalHoneyProductionId, foreign.FinalHoneyProductionId };
            });

            var html = WebUtility.HtmlDecode(await p.Co.HtmlAsync("/Compliance/Index"));
            var flags = System.Text.RegularExpressions.Regex.Matches(html, @"<li data-flag=""(\w+)""><a href=""([^""]+)""")
                .Select(m => (Kind: m.Groups[1].Value, Url: m.Groups[2].Value)).ToList();
            Assert.Contains(("batch", $"/Flow/Batch/{ids[0]}"), flags);       // stale batch CCP
            Assert.Contains(("batch", $"/Flow/Batch/{ids[1]}"), flags);       // self-approved batch CCP
            Assert.Single(flags, f => f.Url == $"/Flow/Batch/{ids[1]}");      // its recent waiting CCP is not flagged
            Assert.Contains(("finalize", $"/Flow/Batch/{ids[2]}"), flags);    // a finalize request waiting too long
            Assert.Contains(("finalize", $"/Flow/Batch/{ids[3]}"), flags);    // finalized by the requester
            Assert.Contains(("storage", $"/Shipments/Storage?batchId={ids[4]}"), flags);
            Assert.DoesNotContain(flags, f => f.Url.EndsWith($"/{ids[5]}"));
            // the other organization's officer sees its own self approval, not the plant's
            var otherHtml = WebUtility.HtmlDecode(await other.Co.HtmlAsync("/Compliance/Index"));
            Assert.Contains($"/Flow/Batch/{ids[5]}", otherHtml);
            Assert.DoesNotContain($"/Flow/Batch/{ids[1]}", otherHtml);
        }
    }
}
