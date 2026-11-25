using System.Net;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-78 P2 (D78-SPEC 3.11): the recall report shows the containers of a delivery with their seal history and jars and checks the
    // stored content against the hash the beekeeper confirmed (HTML and CSV); a legacy delivery is marked as such.
    public partial class CustodyIntakeTests
    {
        [Fact]
        public async Task The_recall_report_shows_containers_seal_history_and_checks_the_content_hash() => await WithCustodyAsync(async () =>
        {
            var p = await PlantAsync("ci9");
            var (_, lot) = await ConfirmBySmsAsync(p, Form(p, 2));
            var batch = await BatchWithAsync(p.OrgId, lot);

            var html = WebUtility.HtmlDecode(await p.Sao.HtmlAsync($"/Recall/Lot/{lot}"));
            Assert.Contains("recall-content-verified", html);
            Assert.DoesNotContain("recall-content-changed", html);
            Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(html, "class=\"recall-container\"").Count);
            foreach (var s in new[] { $"L{lot}-C1", $"L{lot}-C2", "CI9-O0", "CI9-R0", "CI9-R1", $"S-{lot}-C1", "CI9-J1", "Seal checked on arrival", "Seal broken" })
                Assert.Contains(s, html);
            Assert.Contains($"L{lot}-C2", WebUtility.HtmlDecode(await p.Sao.HtmlAsync($"/Recall/Batch/{batch}")));

            var csv = await (await p.Sao.GetAsync($"/Recall/Lot/{lot}?format=csv")).Content.ReadAsStringAsync();
            Assert.Contains("unchanged since confirmation", csv);
            Assert.Contains($"Container,L{lot}-C1,Tin,CI9-R0,25", csv);
            Assert.Contains("Seal checked on arrival CI9-O0", csv);
            Assert.Contains($"Sample jar,S-{lot}-C2,L{lot}-C2,CI9-J1", csv);

            // another organization does not get the report
            var other = await PlantAsync("ci9x");
            Assert.Equal(404, await other.Sao.StatusAsync($"/Recall/Lot/{lot}"));

            // a change behind the application's back (triggers bypassed) shows as "changed after confirmation"
            await using (var conn = new SqlConnection(_app.ConnectionString))
            {
                await conn.OpenAsync();
                await new SqlCommand($@"IF OBJECT_ID(N'dbo.TR_Containers_Immutable') IS NOT NULL DISABLE TRIGGER dbo.TR_Containers_Immutable ON dbo.Containers;
                    UPDATE dbo.Containers SET NetWeightKg = 99 WHERE RawHoneyProductionId = {lot} AND Seq = 1;
                    IF OBJECT_ID(N'dbo.TR_Containers_Immutable') IS NOT NULL ENABLE TRIGGER dbo.TR_Containers_Immutable ON dbo.Containers;", conn).ExecuteNonQueryAsync();
            }
            Assert.Contains("recall-content-changed", await p.Sao.HtmlAsync($"/Recall/Lot/{lot}"));
            Assert.Contains("changed after confirmation", await (await p.Sao.GetAsync($"/Recall/Lot/{lot}?format=csv")).Content.ReadAsStringAsync());
            return 0;
        });

        [Fact]
        public async Task The_recall_report_marks_a_legacy_delivery()
        {
            var p = await PlantAsync("ci10");
            var lot = await _app.WithContextAsync(async db =>
            {
                var r = new RawHoneyProduction
                {
                    RawHoneyProductionApiaryId = p.ApiaryId, RawHoneyProductionAmount = 30, RawHoneyProductionDate = new DateOnly(2026, 6, 1),
                    RawHoneyProductionHoneyType = p.HoneyTypeId, RawHoneyProductionPeriodId = p.PeriodId, OwnerOrgId = p.OrgId, OriginCountry = "TR"
                };
                db.RawHoneyProductions.Add(r);
                await db.SaveChangesAsync();
                db.LotIntakes.Add(new LotIntake
                {
                    RawHoneyProductionId = r.RawHoneyProductionId, ReceivingOrgId = p.OrgId, ReceivedByUserId = "seed", ReceivedAt = DateTime.UtcNow,
                    DeliveryDate = new DateOnly(2026, 6, 1), Quantity = 30, Province = "Giresun", District = p.District,
                    FormStoredFileName = "zz_test.pdf", FormOriginalFileName = "zz_test.pdf", FormSha256 = new string('b', 64)
                });
                await db.SaveChangesAsync();
                return r.RawHoneyProductionId;
            });
            var html = WebUtility.HtmlDecode(await p.Sao.HtmlAsync($"/Recall/Lot/{lot}"));
            Assert.Contains("legacy (containers not recorded)", html);
            Assert.Contains(new string('b', 64), html);
            Assert.DoesNotContain("recall-container", html);
            Assert.Contains("(legacy (containers not recorded))", await (await p.Sao.GetAsync($"/Recall/Lot/{lot}?format=csv")).Content.ReadAsStringAsync());
        }
    }
}
