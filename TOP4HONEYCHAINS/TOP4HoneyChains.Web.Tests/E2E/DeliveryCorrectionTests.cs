using System.Net;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-78 P3 (D-82, P2 follow-up 1): a delivery recorded with containers is corrected only by two platform administrators; the
    // original stays readable, the new content hash is recorded, the audit chain covers both, the beekeeper gets an SMS (M6).
    public partial class CustodyIntakeTests
    {
        private static Dictionary<string, string> CorrectionForm(DeliveryCorrectionForm f, string? reason, Action<Dictionary<string, string>>? change = null)
        {
            var d = new Dictionary<string, string> { ["DeliveryDate"] = f.DeliveryDate!.Value.ToString("yyyy-MM-dd") };
            if (reason != null)
                d["Reason"] = reason;
            for (var i = 0; i < f.Containers.Count; i++)
            {
                var c = f.Containers[i];
                var p = $"Containers[{i}].";
                d[p + "Kind"] = c.Kind!;
                d[p + "NetKg"] = c.NetKg!.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                if (c.GrossKg != null) d[p + "GrossKg"] = c.GrossKg.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                d[p + "ArrivalSeal"] = c.ArrivalSeal!;
                d[p + "JarSeal"] = c.JarSeal!;
                d[p + "CounterSeal"] = c.CounterSeal!;
                d[p + "Reseal"] = c.Reseal!;
            }
            change?.Invoke(d);
            return d;
        }

        [Fact]
        public async Task Two_administrators_correct_a_delivery_the_original_stays_and_the_beekeeper_is_told() => await WithCustodyAsync(async () =>
        {
            var p = await PlantAsync("ci12");
            var (intakeId, lot) = await ConfirmBySmsAsync(p, Form(p, 2));
            await using (var conn = new SqlConnection(_app.ConnectionString))
            {
                await conn.OpenAsync();
                await InstallTriggersAsync(conn);   // the correction passes the delivery locks of migration 024
            }
            await _app.CreateUserAsync("zz_test_ci12_admin1", "Administrator");
            await _app.CreateUserAsync("zz_test_ci12_admin2", "Administrator");
            var admin1 = await UserClient.LoginAsync(_app, "zz_test_ci12_admin1");
            var admin2 = await UserClient.LoginAsync(_app, "zz_test_ci12_admin2");
            DeliveryCorrectionForm stored;
            string oldHash;
            using (var scope = _app.Services.CreateScope())
            {
                var content = (await scope.ServiceProvider.GetRequiredService<CustodyIntakeService>().StoredContentAsync(intakeId))!;
                stored = DeliveryCorrectionService.FormOf(content);
                oldHash = content.Hash();
            }
            var details = $"/Intake/Details/{intakeId}";
            Assert.Contains("id=\"request-correction\"", await admin1.HtmlAsync(details));
            Assert.DoesNotContain("id=\"request-correction\"", await p.Sao.HtmlAsync(details));

            // members cannot request; a reason is required
            var correction = CorrectionForm(stored, "zz_test wrong scale reading", d =>
            {
                d["DeliveryDate"] = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
                d["Containers[0].NetKg"] = "24";
                d["Containers[1].JarSeal"] = "ci12-J1-fixed";
            });
            Assert.Contains("/Account/AccessDenied", (await p.Sao.PostAsync($"/Intake/RequestCorrection/{intakeId}", correction, details)).Headers.Location?.ToString());
            Assert.False(await _app.WithContextAsync(db => db.DeliveryCorrections.AnyAsync(c => c.IntakeId == intakeId)));
            Assert.Contains("A reason is required for a correction.", await admin1.PostFlashAsync($"/Intake/RequestCorrection/{intakeId}", CorrectionForm(stored, null), details));
            Assert.Contains("The correction does not change anything.", await admin1.PostFlashAsync($"/Intake/RequestCorrection/{intakeId}", CorrectionForm(stored, "zz"), details));
            Assert.Contains("Every seal number of the delivery must be different.", await admin1.PostFlashAsync($"/Intake/RequestCorrection/{intakeId}",
                CorrectionForm(stored, "zz", d => d["Containers[1].JarSeal"] = "ci12-R0"), details));
            Assert.Contains("Correction requested", await admin1.PostFlashAsync($"/Intake/RequestCorrection/{intakeId}", correction, details));
            Assert.Contains("already waits", await admin1.PostFlashAsync($"/Intake/RequestCorrection/{intakeId}", correction, details));
            var row = await _app.WithContextAsync(db => db.DeliveryCorrections.SingleAsync(c => c.IntakeId == intakeId));
            Assert.Equal((CorrectionStatus.Pending, oldHash), (row.Status, row.OldContentSha256.Trim()));
            Assert.Equal(50m, await _app.WithContextAsync(db => db.LotIntakes.Where(i => i.IntakeId == intakeId).Select(i => i.Quantity).FirstAsync()));   // nothing changes yet

            // four-eyes: the requester cannot apply it; the second administrator can
            var decide = new Dictionary<string, string> { ["correctionId"] = row.CorrectionId.ToString(), ["approve"] = "true" };
            Assert.Contains("Four-eyes", await admin1.PostFlashAsync("/Intake/DecideCorrection", decide, details));
            Assert.Contains($"/Intake/Details/{intakeId}", await admin2.HtmlAsync("/MyWork/Index"));
            Assert.Contains("Correction applied", await admin2.PostFlashAsync("/Intake/DecideCorrection", decide, details));

            var after = await _app.WithContextAsync(db => db.LotIntakes.Include(i => i.Containers).ThenInclude(c => c.Samples).FirstAsync(i => i.IntakeId == intakeId));
            row = await _app.WithContextAsync(db => db.DeliveryCorrections.SingleAsync(c => c.IntakeId == intakeId));
            Assert.Equal(CorrectionStatus.Applied, row.Status);
            Assert.Equal((49m, DateOnly.FromDateTime(DateTime.Today.AddDays(-1))), (after.Quantity, after.DeliveryDate));
            Assert.Equal(row.NewContentSha256.Trim(), after.ContentSha256!.Trim());
            Assert.NotEqual(oldHash, row.NewContentSha256.Trim());
            Assert.Equal(49m, await _app.WithContextAsync(db => db.RawHoneyProductions.Where(r => r.RawHoneyProductionId == lot).Select(r => r.RawHoneyProductionAmount).FirstAsync()));
            Assert.Equal(24m, after.Containers.Single(c => c.Seq == 1).NetWeightKg);
            Assert.Equal("ci12-J1-fixed", after.Containers.Single(c => c.Seq == 2).Samples.Single().SealNumber);
            using (var scope = _app.Services.CreateScope())
                Assert.True(await scope.ServiceProvider.GetRequiredService<CustodyIntakeService>().ContentMatchesAsync(intakeId));
            // the original stays readable
            var original = DeliveryCorrectionService.FromJson(row.OldContent);
            Assert.Equal((25m, "ci12-J1"), (original.Containers[0].Net, original.Containers[1].JarSeal));
            var page = WebUtility.HtmlDecode(await p.Sao.HtmlAsync(details));
            Assert.Contains("ci12-J1-fixed", page);
            Assert.Contains("data-status=\"Applied\"", page);
            Assert.Contains("id=\"content-verified\"", page);
            // the beekeeper is told; the audit chain has the correction and the changed rows
            var sms = await _app.WithContextAsync(db => db.SmsMessages.SingleAsync(m => m.SmsId == row.InfoSmsId));
            Assert.Equal(SmsPurpose.DeliveryCorrected, sms.Purpose);
            Assert.Contains("49 kg", sms.BodyStored);
            Assert.True(await _app.WithContextAsync(db => db.AuditEntries.AnyAsync(a => a.Entity == "DeliveryCorrections" && a.EntityKey == row.CorrectionId.ToString())));
            Assert.True(await _app.WithContextAsync(db => db.AuditEntries.AnyAsync(a => a.Entity == "Containers" && a.Action == "Update")));
            Assert.Contains("already decided", await admin2.PostFlashAsync("/Intake/DecideCorrection", decide, details));
            return 0;
        });

        [Fact]
        public async Task A_rejected_correction_changes_nothing_and_needs_a_note() => await WithCustodyAsync(async () =>
        {
            var p = await PlantAsync("ci13");
            var (intakeId, _) = await ConfirmBySmsAsync(p, Form(p, 1));
            await _app.CreateUserAsync("zz_test_ci13_admin1", "Administrator");
            await _app.CreateUserAsync("zz_test_ci13_admin2", "Administrator");
            var admin1 = await UserClient.LoginAsync(_app, "zz_test_ci13_admin1");
            var admin2 = await UserClient.LoginAsync(_app, "zz_test_ci13_admin2");
            DeliveryCorrectionForm stored;
            using (var scope = _app.Services.CreateScope())
                stored = DeliveryCorrectionService.FormOf((await scope.ServiceProvider.GetRequiredService<CustodyIntakeService>().StoredContentAsync(intakeId))!);
            var details = $"/Intake/Details/{intakeId}";
            Assert.Contains("Correction requested", await admin1.PostFlashAsync($"/Intake/RequestCorrection/{intakeId}",
                CorrectionForm(stored, "zz", d => d["Containers[0].NetKg"] = "20"), details));
            var id = await _app.WithContextAsync(db => db.DeliveryCorrections.Where(c => c.IntakeId == intakeId).Select(c => c.CorrectionId).SingleAsync());
            Assert.Contains("A note is required", await admin2.PostFlashAsync("/Intake/DecideCorrection", new() { ["correctionId"] = id.ToString(), ["approve"] = "false" }, details));
            Assert.Contains("Correction rejected.", await admin2.PostFlashAsync("/Intake/DecideCorrection", new() { ["correctionId"] = id.ToString(), ["approve"] = "false", ["note"] = "zz no" }, details));
            Assert.Equal(25m, await _app.WithContextAsync(db => db.LotIntakes.Where(i => i.IntakeId == intakeId).Select(i => i.Quantity).FirstAsync()));
            Assert.Null(await _app.WithContextAsync(db => db.DeliveryCorrections.Where(c => c.CorrectionId == id).Select(c => c.InfoSmsId).FirstAsync()));
            return 0;
        });
    }
}
