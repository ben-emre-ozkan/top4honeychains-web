using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Services.OrgModel;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-78 P2 (D78-SPEC 4.4-4.5, 3.10-3.11): the delivery with sealed containers behind Features:CustodyIntake. One row per
    // container, the lot amount is their sum, a jar from every container with an intact cap, the beekeeper confirms with the SMS
    // code (M3) bound to the whole content, a signed photo waits for the Compliance Officer and blocks finalizing until accepted,
    // a confirmed delivery cannot change, legacy deliveries stay valid.
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public partial class CustodyIntakeTests
    {
        private readonly E2EApp _app;
        public CustodyIntakeTests(E2EApp app) => _app = app;

        private record Plant(int OrgId, int LabId, UserClient Sao, UserClient Co, string BusinessNumber, string BeekeeperId, int ApiaryId, int HoneyTypeId,
            int PeriodId, string District, string Tag);

        private async Task<Plant> PlantAsync(string tag, bool hasPhone = true)
        {
            // a number per beekeeper: the per-phone SMS limits (5 an hour, 10 a day) must not couple the tests
            var phone = hasPhone ? "+90532" + Random.Shared.Next(1000000, 9999999).ToString(System.Globalization.CultureInfo.InvariantCulture) : null;
            var org = await _app.CreateOrgAsync($"zz_test {tag} plant", null, (Capabilities.Processing, null));
            var lab = await _app.CreateOrgAsync($"zz_test {tag} lab", null, (Capabilities.Laboratory, "zz-acc"));
            await _app.AddMemberAsync(org, await _app.CreateUserAsync($"zz_test_{tag}_sao"), OrgRoles.SampleAcceptanceOfficer);
            await _app.AddMemberAsync(org, await _app.CreateUserAsync($"zz_test_{tag}_co"), OrgRoles.ComplianceOfficer);
            // D-97: the plant's seal pool holds the codes the forms of these tests use (also the repeat suffixes and the new jars)
            await SealPoolAsync(_app, org, SealKind.Label, PoolCodes(tag));
            var (bkId, apiaryId, honey, period, district) = await _app.WithContextAsync(async db =>
            {
                var bk = new Beekeeper
                {
                    BeekeeperId = Guid.NewGuid().ToString(), BeekeeperFirstName = "Zz", BeekeeperLastName = "Custody " + tag, BeekeeperBirthDate = new DateOnly(1970, 1, 1),
                    BeekeeperBusinessNumber = $"TR-BEE-2026-zz{tag}", BeekeeperPhoneNumber = phone, PreferredLanguage = "en", IsActive = true
                };
                db.Beekeepers.Add(bk);
                var a = new Apiary { ApiaryBeekeeperId = bk.BeekeeperId, ApiaryTitle = $"zz_test {tag} apiary", ApiaryBeehivesCount = 20, ApiaryLocationLatitude = 40.7m, ApiaryLocationLongitude = 38.4m, ApiaryQRCode = Guid.NewGuid() };
                db.Apiaries.Add(a);
                await db.SaveChangesAsync();
                return (bk.BeekeeperId, a.ApiaryId, await db.HoneyTypes.OrderBy(h => h.HoneyTypeId).Select(h => h.HoneyTypeId).FirstAsync(),
                    await db.RawHoneyProductionPeriods.OrderBy(p => p.RawHoneyProductionPeriodId).Select(p => p.RawHoneyProductionPeriodId).FirstAsync(),
                    await db.Districts.Where(d => d.ProvinceCode == "28").OrderBy(d => d.DistrictId).Select(d => d.Name).FirstAsync());
            });
            return new Plant(org, lab, await UserClient.LoginAsync(_app, $"zz_test_{tag}_sao"), await UserClient.LoginAsync(_app, $"zz_test_{tag}_co"),
                $"TR-BEE-2026-zz{tag}", bkId!, apiaryId, honey, period, district, tag);
        }

        internal static IEnumerable<string> PoolCodes(string tag) =>
            from kind in new[] { "O", "J", "K", "R", "N" } from i in Enumerable.Range(0, 4) from suffix in new[] { "", "B", "C", "K" }
            select $"{tag}-{kind}{i}{suffix}".ToUpperInvariant();

        // D-97: codes put into an organization's seal pool directly (the pool page is tested in D97SealPoolTests)
        internal static Task SealPoolAsync(E2EApp app, int orgId, string kind, IEnumerable<string> codes) => app.WithContextAsync(async db =>
        {
            var list = codes.ToList();
            var batch = new SealBatch { OrgId = orgId, Kind = kind, FirstCode = list[0], LastCode = list[^1], CodeCount = list.Count, IssuedByUserId = "seed", IssuedAt = DateTime.UtcNow };
            foreach (var c in list)
                batch.Codes.Add(new SealCode { OrgId = orgId, Code = c, Kind = kind, Status = SealStatus.Issued });
            db.SealBatches.Add(batch);
            return await db.SaveChangesAsync();
        });

        private Dictionary<string, string> Form(Plant p, int containers = 2, Action<int, Dictionary<string, string>>? change = null)
        {
            var f = new Dictionary<string, string>
            {
                ["Number"] = p.BusinessNumber, ["ApiaryId"] = p.ApiaryId.ToString(), ["HoneyTypeId"] = p.HoneyTypeId.ToString(), ["PeriodId"] = p.PeriodId.ToString(),
                ["DeliveryDate"] = DateTime.Today.ToString("yyyy-MM-dd"), ["Province"] = "Giresun", ["District"] = p.District, ["OriginCountry"] = "TR",
                ["LabOrgId"] = p.LabId.ToString(), ["CounterLocation"] = "zz_test cold room", ["RetainUntil"] = DateTime.Today.AddYears(2).ToString("yyyy-MM-dd"),
                ["CapType"] = JarCap.SafetyButton
            };
            for (var i = 0; i < containers; i++)
            {
                var c = $"Containers[{i}].";
                f[c + "Closure"] = ClosureKind.SealLabel;
                f[c + "Kind"] = "Tin";
                f[c + "NetKg"] = "25";
                f[c + "GrossKg"] = "26.5";
                if (i == 0)
                {
                    f[c + "ArrivedSealed"] = "true";
                    f[c + "OriginSeal"] = $"{p.Tag}-O{i}";
                    f[c + "OriginSealIntact"] = "true";
                }
                else
                    f[c + "ArrivedSealed"] = "false";
                f[c + "JarSeal"] = $"{p.Tag}-J{i}";
                f[c + "CounterSeal"] = $"{p.Tag}-K{i}";
                f[c + "CapIntact"] = "true";
                f[c + "ClosingSeal"] = $"{p.Tag}-R{i}";
                change?.Invoke(i, f);
            }
            return f;
        }

        private static Dictionary<string, string> Op(Dictionary<string, string> f, string op) => new(f) { ["op"] = op };
        private static string DraftOf(string html) => Regex.Match(html, "name=\"DraftKey\" value=\"([0-9a-f]+)\"").Groups[1].Value;
        private static async Task<string> BodyAsync(HttpResponseMessage r) => WebUtility.HtmlDecode(await r.Content.ReadAsStringAsync());

        private async Task<T> WithCustodyAsync<T>(Func<Task<T>> work)
        {
            var config = _app.Services.GetRequiredService<IConfiguration>();
            config["Features:CustodyIntake"] = "true";
            try { return await work(); }
            finally { config["Features:CustodyIntake"] = "false"; }
        }

        private async Task<(int IntakeId, int LotId)> ConfirmBySmsAsync(Plant p, Dictionary<string, string> form)
        {
            var html = await BodyAsync(await p.Sao.PostAsync("/Intake/Record", Op(form, "send"), $"/Intake/Create?number={p.BusinessNumber}"));
            Assert.Contains("id=\"intake-confirm\"", html);
            var draft = DraftOf(html);
            var sms = await _app.WithContextAsync(db => db.SmsMessages.Where(m => m.SubjectKey == draft).OrderByDescending(m => m.SmsId).FirstAsync());
            var code = Regex.Match(sms.BodyStored, @"\b\d{6}\b").Value;
            var confirmed = await p.Sao.PostAsync("/Intake/Record", Op(new Dictionary<string, string>(form) { ["DraftKey"] = draft, ["Code"] = code }, "confirm"),
                $"/Intake/Create?number={p.BusinessNumber}");
            Assert.Equal(HttpStatusCode.Redirect, confirmed.StatusCode);
            var intakeId = int.Parse(confirmed.Headers.Location!.ToString().Split('/').Last());
            return (intakeId, await _app.WithContextAsync(db => db.LotIntakes.Where(i => i.IntakeId == intakeId).Select(i => i.RawHoneyProductionId).FirstAsync()));
        }

        [Fact]
        public async Task With_the_switch_off_the_intake_works_as_before()
        {
            var p = await PlantAsync("ci0");
            Assert.Contains("id=\"intake-form\"", await p.Sao.HtmlAsync($"/Intake/Create?number={p.BusinessNumber}"));
            Assert.Equal(HttpStatusCode.NotFound, (await p.Sao.PostAsync("/Intake/Record", Op(Form(p), "send"), "/Intake/Index")).StatusCode);
        }

        [Fact]
        public async Task A_delivery_is_confirmed_by_sms_with_its_containers_seals_and_a_jar_per_container() => await WithCustodyAsync(async () =>
        {
            var p = await PlantAsync("ci1");
            Assert.Contains("id=\"custody-form\"", await p.Sao.HtmlAsync($"/Intake/Create?number={p.BusinessNumber}"));
            var form = Form(p);

            var html = await BodyAsync(await p.Sao.PostAsync("/Intake/Record", Op(form, "send"), $"/Intake/Create?number={p.BusinessNumber}"));
            var draft = DraftOf(html);
            var sms = await _app.WithContextAsync(db => db.SmsMessages.Where(m => m.SubjectKey == draft).FirstAsync());
            Assert.Equal(SmsPurpose.IntakeConfirm, sms.Purpose);
            Assert.Contains("zz_test ci1 plant", sms.BodyStored);
            Assert.Contains("2 tins", sms.BodyStored);
            Assert.Contains("50 kg", sms.BodyStored);
            Assert.Contains("2 samples sealed", sms.BodyStored);
            Assert.Contains(DateTime.Today.ToString("dd.MM.yyyy"), sms.BodyStored);

            // any change after sending voids the code
            var code = Regex.Match(sms.BodyStored, @"\b\d{6}\b").Value;
            var changed = new Dictionary<string, string>(form) { ["DraftKey"] = draft, ["Code"] = code, ["Containers[1].ClosingSeal"] = "ci1-R3" };
            Assert.Contains("The details changed after the code was sent", await BodyAsync(await p.Sao.PostAsync("/Intake/Record", Op(changed, "confirm"), "/Intake/Index")));

            HttpResponseMessage confirmed;
            try
            {
                _app.Clock.Advance(TimeSpan.FromSeconds(61));
                var resent = await BodyAsync(await p.Sao.PostAsync("/Intake/Record", Op(new Dictionary<string, string>(form) { ["DraftKey"] = draft }, "send"), "/Intake/Index"));
                Assert.Contains("id=\"intake-confirm\"", resent);
                code = Regex.Match((await _app.WithContextAsync(db => db.SmsMessages.Where(m => m.SubjectKey == draft).OrderByDescending(m => m.SmsId).FirstAsync())).BodyStored, @"\b\d{6}\b").Value;
                confirmed = await p.Sao.PostAsync("/Intake/Record", Op(new Dictionary<string, string>(form) { ["DraftKey"] = draft, ["Code"] = code }, "confirm"), "/Intake/Index");
            }
            finally
            {
                _app.Clock.Reset();
            }
            Assert.Equal(HttpStatusCode.Redirect, confirmed.StatusCode);
            var intakeId = int.Parse(confirmed.Headers.Location!.ToString().Split('/').Last());

            var intake = await _app.WithContextAsync(db => db.LotIntakes.Include(i => i.Containers).ThenInclude(c => c.SealEvents)
                .Include(i => i.Containers).ThenInclude(c => c.Samples).FirstAsync(i => i.IntakeId == intakeId));
            Assert.True(intake.ContainersRecorded);
            Assert.Equal(ConfirmationMethod.SmsCode, intake.ConfirmationMethod);
            Assert.Equal(EvidenceStatus.Accepted, intake.EvidenceStatus);
            Assert.NotNull(intake.CodeId);
            Assert.Equal(50m, intake.Quantity);
            Assert.Equal(50m, await _app.WithContextAsync(db => db.RawHoneyProductions.Where(r => r.RawHoneyProductionId == intake.RawHoneyProductionId).Select(r => r.RawHoneyProductionAmount).FirstAsync()));
            var lot = intake.RawHoneyProductionId;
            Assert.Equal(new[] { $"L{lot}-C1", $"L{lot}-C2" }, intake.Containers.OrderBy(c => c.Seq).Select(c => c.ContainerCode));

            var sealed0 = intake.Containers.Single(c => c.Seq == 1);
            Assert.Equal("CI1-O0", sealed0.OriginSealNumber);
            Assert.Equal(ClosureKind.SealLabel, sealed0.Closure);
            Assert.Equal(new[] { SealEvent.Checked, SealEvent.Broken, SealEvent.Applied }, sealed0.SealEvents.OrderBy(e => e.EventId).Select(e => e.Event));
            // D-97: an unsealed container gets one seal, after sampling
            var unsealed = intake.Containers.Single(c => c.Seq == 2);
            Assert.Null(unsealed.OriginSealNumber);
            Assert.Equal(new[] { "CI1-R1" }, unsealed.SealEvents.OrderBy(e => e.EventId).Select(e => e.SealNumber));
            Assert.Equal("CI1-R1", CustodyIntakeService.CurrentSeal(unsealed.SealEvents));
            Assert.Equal(JarCap.SafetyButton, intake.JarCapType);
            // the pool: origin seal opened, closing seals and jar labels used, each on its container or jar
            var pool = await _app.WithContextAsync(db => db.SealCodes.Where(s => s.OrgId == p.OrgId && s.Status != SealStatus.Issued).ToListAsync());
            Assert.Equal((SealStatus.Broken, SealUse.Origin), pool.Where(s => s.Code == "CI1-O0").Select(s => (s.Status, s.UsedAs)).Single());
            Assert.Equal((SealStatus.Used, SealUse.Closing, (int?)unsealed.ContainerId), pool.Where(s => s.Code == "CI1-R1").Select(s => (s.Status, s.UsedAs, s.ContainerId)).Single());
            Assert.Equal(SealUse.CounterJar, pool.Single(s => s.Code == "CI1-K0").UsedAs);
            Assert.Equal(7, pool.Count);

            // a jar from every container: jar -> container -> intake -> content hash
            Assert.All(intake.Containers, c =>
            {
                var jar = Assert.Single(c.Samples);
                Assert.Equal(SampleKind.Container, jar.Kind);
                Assert.True(jar.JarCapIntactAtSealing);
                Assert.Equal(JarCap.SafetyButton, jar.CapType);
                Assert.Equal(p.LabId, jar.LabOrgId);
                Assert.Equal(SampleStatus.Sent, jar.Status);
                Assert.Equal($"S-{lot}-C{c.Seq}", jar.SampleCode);
            });
            using (var scope = _app.Services.CreateScope())
            {
                var custody = scope.ServiceProvider.GetRequiredService<CustodyIntakeService>();
                Assert.True(await custody.ContentMatchesAsync(intakeId));
                var stored = await custody.StoredContentAsync(intakeId);
                Assert.StartsWith("intake2|", stored!.Canonical());
                Assert.Contains("CI1-J0", stored.Canonical());
                Assert.Contains("CI1-R1", stored.Canonical());
                Assert.Equal(intake.ContentSha256, stored.Hash());
            }

            var details = await p.Sao.HtmlAsync($"/Intake/Details/{intakeId}");
            Assert.Contains("id=\"content-verified\"", details);
            var labels = await p.Sao.HtmlAsync($"/Intake/Labels/{intakeId}?size=label");
            Assert.Equal(2, Regex.Matches(labels, "class=\"label container-label\"").Count);
            Assert.Equal(4, Regex.Matches(labels, "class=\"label jar-label\"").Count);
            Assert.Contains("<svg", labels);
            Assert.Contains("@page { size: 100mm 50mm", labels);
            Assert.Contains("size: A4", await p.Sao.HtmlAsync($"/Intake/Labels/{intakeId}?size=a4"));
            // another organization sees neither the delivery nor its labels
            var other = await PlantAsync("ci1x");
            Assert.Equal(404, await other.Sao.StatusAsync($"/Intake/Details/{intakeId}"));
            Assert.Equal(404, await other.Sao.StatusAsync($"/Intake/Labels/{intakeId}?size=a4"));

            // the legacy sample form is not used for such a lot
            Assert.Equal($"/Intake/Details/{intakeId}", (await p.Sao.GetAsync($"/Samples/Create?lotId={lot}")).Headers.Location?.ToString());
            return 0;
        });

        [Fact]
        public async Task A_code_sent_for_another_delivery_does_not_confirm_this_one() => await WithCustodyAsync(async () =>
        {
            var p = await PlantAsync("ci11");
            var first = Form(p, 1);
            var firstHtml = await BodyAsync(await p.Sao.PostAsync("/Intake/Record", Op(first, "send"), $"/Intake/Create?number={p.BusinessNumber}"));
            var firstCode = System.Text.RegularExpressions.Regex.Match((await _app.WithContextAsync(db => db.SmsMessages.Where(m => m.SubjectKey == DraftOf(firstHtml)).FirstAsync())).BodyStored, @"\b\d{6}\b").Value;
            var second = Form(p, 1, (i, f) => f[$"Containers[{i}].NetKg"] = "30");
            var secondHtml = await BodyAsync(await p.Sao.PostAsync("/Intake/Record", Op(second, "send"), $"/Intake/Create?number={p.BusinessNumber}"));
            Assert.NotEqual(DraftOf(firstHtml), DraftOf(secondHtml));
            var wrong = await p.Sao.PostAsync("/Intake/Record", Op(new Dictionary<string, string>(second) { ["DraftKey"] = DraftOf(secondHtml), ["Code"] = firstCode }, "confirm"), "/Intake/Index");
            Assert.Equal(HttpStatusCode.OK, wrong.StatusCode);
            // the first delivery's code with the second delivery's content under the first draft fails too: the content hash differs
            var swapped = await p.Sao.PostAsync("/Intake/Record", Op(new Dictionary<string, string>(second) { ["DraftKey"] = DraftOf(firstHtml), ["Code"] = firstCode }, "confirm"), "/Intake/Index");
            Assert.Equal(HttpStatusCode.OK, swapped.StatusCode);
            Assert.False(await _app.WithContextAsync(db => db.LotIntakes.AnyAsync(i => i.RawHoneyProduction!.RawHoneyProductionApiaryId == p.ApiaryId)));
            return 0;
        });

        [Fact]
        public async Task The_container_rows_are_checked() => await WithCustodyAsync(async () =>
        {
            var p = await PlantAsync("ci2");
            async Task<string> SendAsync(Dictionary<string, string> f) =>
                await BodyAsync(await p.Sao.PostAsync("/Intake/Record", Op(f, "send"), $"/Intake/Create?number={p.BusinessNumber}"));

            Assert.Contains("Container 1: the button of the jar&#x27;s cap must be down.".Replace("&#x27;", "'"),
                await SendAsync(Form(p, 1, (i, f) => f.Remove($"Containers[{i}].CapIntact"))));
            Assert.Contains("Every seal code of the delivery must be different.",
                await SendAsync(Form(p, 2, (i, f) => f[$"Containers[{i}].JarSeal"] = "ci2-J3")));
            Assert.Contains("Container 2: enter the closing seal, applied after sampling in front of the beekeeper.",
                await SendAsync(Form(p, 2, (i, f) => { if (i == 1) f.Remove($"Containers[{i}].ClosingSeal"); })));
            Assert.Contains("Container 1: enter the net weight (more than 0 kg).",
                await SendAsync(Form(p, 1, (i, f) => f[$"Containers[{i}].NetKg"] = "0")));
            Assert.Contains("Record at least one container.", await SendAsync(Form(p, 0)));
            var future = Form(p, 1);
            future["DeliveryDate"] = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            Assert.Contains("Enter the delivery date (not in the future).", await SendAsync(future));
            Assert.False(await _app.WithContextAsync(db => db.SmsMessages.AnyAsync(m => m.Purpose == SmsPurpose.IntakeConfirm && m.BodyStored.Contains("zz_test ci2 plant"))));
            return 0;
        });

        [Fact]
        public async Task A_signed_photo_needs_a_reason_and_blocks_finalizing_until_the_compliance_officer_accepts() => await WithCustodyAsync(async () =>
        {
            var p = await PlantAsync("ci3", hasPhone: false);
            var form = Form(p, 1);
            Assert.Contains("The beekeeper has no mobile phone", await BodyAsync(await p.Sao.PostAsync("/Intake/Record", Op(form, "send"), $"/Intake/Create?number={p.BusinessNumber}")));
            Assert.Contains("Give the reason why the SMS code cannot be used.",
                await BodyAsync(await p.Sao.PostMultipartAsync("/Intake/Record", Op(form, "photo"), Pdf.File("deliveryForm"), $"/Intake/Create?number={p.BusinessNumber}")));
            form["FallbackReason"] = "zz_test no phone";
            var saved = await p.Sao.PostMultipartAsync("/Intake/Record", Op(form, "photo"), Pdf.File("deliveryForm"), $"/Intake/Create?number={p.BusinessNumber}");
            Assert.Equal(HttpStatusCode.Redirect, saved.StatusCode);
            var intakeId = int.Parse(saved.Headers.Location!.ToString().Split('/').Last());
            var intake = await _app.WithContextAsync(db => db.LotIntakes.FirstAsync(i => i.IntakeId == intakeId));
            Assert.Equal(ConfirmationMethod.SignedPhoto, intake.ConfirmationMethod);
            Assert.Equal(EvidenceStatus.PendingReview, intake.EvidenceStatus);
            Assert.Null(intake.ConfirmedAt);

            // a batch with this lot cannot be finalized while the photo waits
            var batch = await BatchWithAsync(p.OrgId, intake.RawHoneyProductionId);
            Assert.Contains(await BlockersAsync(batch), b => b.Contains("the signed photo of the delivery waits for the Compliance Officer"));

            // the recorder cannot review; the Compliance Officer can (My work)
            Assert.Equal("You recorded this delivery; another person must review its signed photo.",
                await p.Sao.PostFlashAsync($"/Intake/Review/{intakeId}", new() { ["accept"] = "true" }, $"/Intake/Details/{intakeId}"));
            Assert.Contains($"/Intake/Details/{intakeId}", await p.Co.HtmlAsync("/MyWork/Index"));
            Assert.Equal("Signed photo accepted; the lot can be finalized when its CCPs are done.",
                await p.Co.PostFlashAsync($"/Intake/Review/{intakeId}", new() { ["accept"] = "true" }, $"/Intake/Details/{intakeId}"));
            Assert.DoesNotContain(await BlockersAsync(batch), b => b.Contains("signed photo"));
            return 0;
        });

        private Task<int> WholesalerAsync() => _app.WithContextAsync(async db =>
        {
            var w = new Wholesaler { WholesalerTitle = "zz_test custody wholesaler", WholesalerIsActive = true, CreatedAt = DateTime.Now };
            db.Wholesalers.Add(w);
            await db.SaveChangesAsync();
            return w.WholesalerId;
        });

        private async Task<int> BatchWithAsync(int orgId, int lotId)
        {
            var wholesaler = await WholesalerAsync();
            return await _app.WithContextAsync(async db =>
        {
            var b = new FinalHoneyProduction
            {
                FinalHoneyProductionTitle = "zz_test custody batch", FinalHoneyProductionDate = DateTime.Today, FinalHoneyProductionTotalAmount = 25,
                FinalHoneyProductionQR = Guid.NewGuid(), OwnerOrgId = orgId, FinalHoneyProductionWholesalerId = wholesaler
            };
            db.FinalHoneyProductions.Add(b);
            await db.SaveChangesAsync();
            db.FinalHoneyProductionDetails.Add(new FinalHoneyProductionDetail { FinalHoneyProductionId = b.FinalHoneyProductionId, RawHoneyProductionId = lotId, UsedAmount = 25, CreatedAt = DateTime.Now });
            await db.SaveChangesAsync();
            return b.FinalHoneyProductionId;
        });
        }

        private async Task<List<string>> BlockersAsync(int batchId)
        {
            using var scope = _app.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<CcpIntegrityService>().GetFinalizeBlockersAsync(batchId);
        }

        [Fact]
        public async Task Legacy_deliveries_and_lots_without_an_intake_stay_valid_and_are_not_blocked()
        {
            var p = await PlantAsync("ci4");
            // legacy-shaped live data: an old intake with its signed form only, and an administrator's lot without an intake
            var (legacyLot, adminLot) = await _app.WithContextAsync(async db =>
            {
                RawHoneyProduction Lot() => new()
                {
                    RawHoneyProductionApiaryId = p.ApiaryId, RawHoneyProductionAmount = 30, RawHoneyProductionDate = new DateOnly(2026, 6, 1),
                    RawHoneyProductionHoneyType = p.HoneyTypeId, RawHoneyProductionPeriodId = p.PeriodId, OwnerOrgId = p.OrgId, OriginCountry = "TR"
                };
                var a = Lot();
                var b = Lot();
                db.RawHoneyProductions.AddRange(a, b);
                await db.SaveChangesAsync();
                db.LotIntakes.Add(new LotIntake
                {
                    RawHoneyProductionId = a.RawHoneyProductionId, ReceivingOrgId = p.OrgId, ReceivedByUserId = "seed", ReceivedAt = DateTime.UtcNow,
                    DeliveryDate = new DateOnly(2026, 6, 1), Quantity = 30, Province = "Giresun", District = p.District,
                    FormStoredFileName = "zz_test.pdf", FormOriginalFileName = "zz_test.pdf", FormSha256 = new string('a', 64)
                });
                await db.SaveChangesAsync();
                return (a.RawHoneyProductionId, b.RawHoneyProductionId);
            });
            var batch = await BatchWithAsync(p.OrgId, legacyLot);
            await _app.WithContextAsync(async db =>
            {
                db.FinalHoneyProductionDetails.Add(new FinalHoneyProductionDetail { FinalHoneyProductionId = batch, RawHoneyProductionId = adminLot, UsedAmount = 30, CreatedAt = DateTime.Now });
                return await db.SaveChangesAsync();
            });
            var blockers = await BlockersAsync(batch);
            Assert.DoesNotContain(blockers, b => b.Contains("signed photo") || b.Contains("container", StringComparison.OrdinalIgnoreCase));

            var list = await p.Sao.HtmlAsync("/Intake/Index");
            Assert.Contains("legacy (containers not recorded)", WebUtility.HtmlDecode(list));
            using (var scope = _app.Services.CreateScope())
                Assert.Null(await scope.ServiceProvider.GetRequiredService<CustodyIntakeService>().ContentMatchesAsync(
                    await _app.WithContextAsync(db => db.LotIntakes.Where(i => i.RawHoneyProductionId == legacyLot).Select(i => i.IntakeId).FirstAsync())));
            // the legacy sample form still works for a legacy lot, also with the switch on
            await WithCustodyAsync(async () =>
            {
                Assert.Equal(HttpStatusCode.OK, (await p.Sao.GetAsync($"/Samples/Create?lotId={legacyLot}")).StatusCode);
                return 0;
            });
        }

        [Fact]
        public async Task A_confirmed_delivery_cannot_be_changed_by_an_administrator_or_in_the_database() => await WithCustodyAsync(async () =>
        {
            var p = await PlantAsync("ci5");
            var (intakeId, lot) = await ConfirmBySmsAsync(p, Form(p, 1));
            await _app.CreateUserAsync("zz_test_ci5_admin", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "zz_test_ci5_admin");
            Assert.Contains("id=\"delivery-locked\"", await admin.HtmlAsync($"/RawHoneyProductions/Edit/{lot}"));
            var post = await admin.PostAsync($"/RawHoneyProductions/Edit/{lot}", new()
            {
                ["RawHoneyProductionId"] = lot.ToString(), ["RawHoneyProductionApiaryId"] = p.ApiaryId.ToString(), ["RawHoneyProductionHoneyType"] = p.HoneyTypeId.ToString(),
                ["RawHoneyProductionPeriodId"] = p.PeriodId.ToString(), ["RawHoneyProductionAmount"] = "999", ["RawHoneyProductionDate"] = "2026-01-01",
                ["OwnerOrgId"] = p.OrgId.ToString(), ["OriginCountry"] = "AR", ["RawHoneyProductionWholesalerId"] = (await WholesalerAsync()).ToString()
            }, $"/RawHoneyProductions/Edit/{lot}");
            Assert.Equal(HttpStatusCode.Redirect, post.StatusCode);
            var stored = await _app.WithContextAsync(db => db.RawHoneyProductions.FirstAsync(r => r.RawHoneyProductionId == lot));
            Assert.Equal(25m, stored.RawHoneyProductionAmount);
            Assert.Equal("TR", stored.OriginCountry);
            Assert.Equal(DateOnly.FromDateTime(DateTime.Today), stored.RawHoneyProductionDate);

            // the database refuses changes too (the triggers of migration 023, installed on the test database here)
            await using var conn = new SqlConnection(_app.ConnectionString);
            await conn.OpenAsync();
            await InstallTriggersAsync(conn);
            async Task<bool> RefusedAsync(string sql)
            {
                try { await new SqlCommand(sql, conn).ExecuteNonQueryAsync(); return false; }
                catch (SqlException) { return true; }
            }
            Assert.True(await RefusedAsync($"UPDATE dbo.RawHoneyProductions SET RawHoneyProductionAmount = 1 WHERE RawHoneyProductionId = {lot}"));
            Assert.True(await RefusedAsync($"UPDATE dbo.LotIntakes SET Quantity = 1 WHERE IntakeId = {intakeId}"));
            Assert.True(await RefusedAsync($"UPDATE dbo.Containers SET NetWeightKg = 1 WHERE IntakeId = {intakeId}"));
            Assert.True(await RefusedAsync($"DELETE FROM dbo.ContainerSealEvents WHERE ContainerId IN (SELECT ContainerId FROM dbo.Containers WHERE IntakeId = {intakeId})"));
            Assert.False(await RefusedAsync($"UPDATE dbo.RawHoneyProductions SET OwnerOrgId = OwnerOrgId WHERE RawHoneyProductionId = {lot}"));
            return 0;
        });

        // the triggers of migrations 023-025 (024 replaces the delivery locks of 023), installed on the test database
        internal static async Task InstallTriggersAsync(SqlConnection conn)
        {
            foreach (var file in new[] { "023_d78_containers_samples.sql", "024_d78_lot_prices.sql", "025_d78_compliance.sql" })
            {
                var script = await File.ReadAllTextAsync(Path.Combine(RepoRoot(), "db", "migrations", file));
                foreach (var batch in Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline).Where(b => b.Contains("CREATE OR ALTER TRIGGER")))
                    await new SqlCommand(batch.Trim(), conn).ExecuteNonQueryAsync();
            }
        }

        private static string RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "db", "migrations")))
                dir = dir.Parent;
            return dir!.FullName;
        }
    }
}
