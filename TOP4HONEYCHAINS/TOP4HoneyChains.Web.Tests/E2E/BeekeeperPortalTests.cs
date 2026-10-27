using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-78 P3 (D78-SPEC 2.4, 3.8, 4.7, 6.4 C5, O-5 to O-8, D-82): the beekeeper portal. Two beekeepers, each sees only their own data on
    // every screen; the price is their only business write (until the lot is in a finalized batch); the lot owner's OA and CO read it,
    // Staff and later holders do not, the association only with consent; the public-name consent switches the Honey Story; every
    // other page and POST answers 403, an id that is not theirs 404.
    public partial class CustodyIntakeTests
    {
        private record PortalBee(Plant Plant, UserClient Client, int Lot, int IntakeId);

        private async Task<PortalBee> PortalBeekeeperAsync(string tag)
        {
            var p = await PlantAsync(tag);
            var (intakeId, lot) = await ConfirmBySmsAsync(p, Form(p, 1));
            var userId = await _app.CreateUserAsync($"zz_test_{tag}_bee");
            await _app.WithContextAsync(async db =>
            {
                db.BeekeeperAccounts.Add(new BeekeeperAccount { BeekeeperId = p.BeekeeperId, UserId = userId, LinkedAt = DateTime.UtcNow, LinkedByUserId = "seed", Status = BeekeeperAccountStatus.Active });
                return await db.SaveChangesAsync();
            });
            return new PortalBee(p, await UserClient.LoginAsync(_app, $"zz_test_{tag}_bee"), lot, intakeId);
        }

        private async Task<T> WithPortalAsync<T>(Func<Task<T>> work) => await WithCustodyAsync(async () =>
        {
            var config = _app.Services.GetRequiredService<IConfiguration>();
            config["Features:BeekeeperAccounts"] = "true";
            try { return await work(); }
            finally { config["Features:BeekeeperAccounts"] = "false"; }
        });

        private Task<int> FinalizeAsync(int batchId) => _app.WithContextAsync(async db =>
        {
            var b = await db.FinalHoneyProductions.FirstAsync(x => x.FinalHoneyProductionId == batchId);
            b.FinalHoneyProductionIsFinalized = true;
            b.FinalHoneyProductionFinalizedDate = DateTime.Today;
            return await db.SaveChangesAsync();
        });

        private static Dictionary<string, string> PriceForm(int lot, string price, string currency = "TRY") =>
            new() { ["lotId"] = lot.ToString(), ["pricePerKg"] = price, ["currency"] = currency, ["note"] = "zz_test" };

        [Fact]
        public async Task Every_portal_screen_shows_only_the_signed_in_beekeepers_data() => await WithPortalAsync(async () =>
        {
            var one = await PortalBeekeeperAsync("pt1");
            var two = await PortalBeekeeperAsync("pt2");
            var batch = await BatchWithAsync(one.Plant.OrgId, one.Lot);
            await FinalizeAsync(batch);
            foreach (var (me, other) in new[] { (one, two), (two, one) })
            {
                foreach (var screen in new[] { "/Me", "/Me/Deliveries", "/Me/Samples", "/Me/Batches", "/Me/Stories", "/Me/Apiaries", "/Me/Privacy", "/Me/Account" })
                {
                    var html = WebUtility.HtmlDecode(await me.Client.HtmlAsync(screen));
                    Assert.DoesNotContain($"L{other.Lot}-C1", html);
                    Assert.DoesNotContain($"S-{other.Lot}-C1", html);
                    Assert.DoesNotContain($"zz_test {other.Plant.Tag} apiary", html);
                    Assert.DoesNotContain(other.Plant.BusinessNumber, html);
                }
                Assert.Contains($"L{me.Lot}-C1", WebUtility.HtmlDecode(await me.Client.HtmlAsync("/Me/Deliveries")));
                Assert.Contains($"S-{me.Lot}-C1", await me.Client.HtmlAsync("/Me/Samples"));
                Assert.Contains($"zz_test {me.Plant.Tag} apiary", WebUtility.HtmlDecode(await me.Client.HtmlAsync("/Me/Apiaries")));
                Assert.Contains(me.Plant.BusinessNumber, await me.Client.HtmlAsync("/Me"));
                Assert.Contains($"zz_test {me.Plant.Tag} plant", WebUtility.HtmlDecode(await me.Client.HtmlAsync("/Me/Privacy")));   // the data controller holding their data
            }
            Assert.Contains("class=\"me-card me-batch\"", await one.Client.HtmlAsync("/Me/Batches"));
            Assert.DoesNotContain("class=\"me-card me-batch\"", await two.Client.HtmlAsync("/Me/Batches"));
            var qr = await _app.WithContextAsync(db => db.FinalHoneyProductions.Where(b => b.FinalHoneyProductionId == batch).Select(b => b.FinalHoneyProductionQR).FirstAsync());
            Assert.Contains($"/honeystory/{qr}", await one.Client.HtmlAsync("/Me/Stories"));
            Assert.DoesNotContain($"/honeystory/{qr}", await two.Client.HtmlAsync("/Me/Stories"));
            return 0;
        });

        [Fact]
        public async Task The_price_is_the_beekeepers_only_write_and_only_the_lot_owner_reads_it() => await WithPortalAsync(async () =>
        {
            var one = await PortalBeekeeperAsync("pt3");
            var two = await PortalBeekeeperAsync("pt4");
            Assert.Contains("Enter the price per kg", await one.Client.PostFlashAsync("/Me/Price", PriceForm(one.Lot, "0"), "/Me/Deliveries"));
            Assert.Contains("Choose a currency.", await one.Client.PostFlashAsync("/Me/Price", PriceForm(one.Lot, "12.5", "XXX"), "/Me/Deliveries"));
            Assert.Contains("Price saved.", await one.Client.PostFlashAsync("/Me/Price", PriceForm(one.Lot, "12.5"), "/Me/Deliveries"));
            Assert.Contains("Price saved.", await one.Client.PostFlashAsync("/Me/Price", PriceForm(one.Lot, "13,25"), "/Me/Deliveries"));
            Assert.Equal(new[] { 13.25m, 12.5m }, await _app.WithContextAsync(db => db.LotPrices.Where(x => x.RawHoneyProductionId == one.Lot).OrderByDescending(x => x.PriceId).Select(x => x.PricePerKg).ToArrayAsync()));
            var deliveries = WebUtility.HtmlDecode(await one.Client.HtmlAsync("/Me/Deliveries"));
            Assert.Contains("Price history (2)", deliveries);
            // another beekeeper's lot is not found
            Assert.Equal(HttpStatusCode.NotFound, (await two.Client.PostAsync("/Me/Price", PriceForm(one.Lot, "1"), "/Me/Deliveries")).StatusCode);
            Assert.DoesNotContain("13.25", WebUtility.HtmlDecode(await two.Client.HtmlAsync("/Me/Deliveries")));

            // the plant's OA and CO read it; Staff does not; a later holder does not
            await _app.AddMemberAsync(one.Plant.OrgId, await _app.CreateUserAsync("zz_test_pt3_oa"), OrgRoles.OrgAdmin);
            await _app.AddMemberAsync(one.Plant.OrgId, await _app.CreateUserAsync("zz_test_pt3_staff"), OrgRoles.Staff);
            var oa = await UserClient.LoginAsync(_app, "zz_test_pt3_oa");
            var staff = await UserClient.LoginAsync(_app, "zz_test_pt3_staff");
            Assert.Contains("13.25 TRY / kg", WebUtility.HtmlDecode(await oa.HtmlAsync("/Intake/Index")));
            Assert.Contains("id=\"lot-prices\"", await oa.HtmlAsync($"/Intake/Details/{one.IntakeId}"));
            Assert.Contains("id=\"lot-prices\"", await one.Plant.Co.HtmlAsync($"/Intake/Details/{one.IntakeId}"));
            Assert.DoesNotContain("id=\"lot-prices\"", await staff.HtmlAsync($"/Intake/Details/{one.IntakeId}"));
            Assert.DoesNotContain("13.25", await one.Plant.Sao.HtmlAsync("/Intake/Index"));
            var holder = await _app.CreateOrgAsync("zz_test pt3 holder", null, (Capabilities.Processing, null));
            await _app.AddMemberAsync(holder, await _app.CreateUserAsync("zz_test_pt3_holder_oa"), OrgRoles.OrgAdmin);
            var holderOa = await UserClient.LoginAsync(_app, "zz_test_pt3_holder_oa");
            var holderBatch = await BatchWithAsync(holder, one.Lot);
            var holderPage = await holderOa.HtmlAsync($"/Intake/Details/{one.IntakeId}");
            Assert.DoesNotContain("id=\"lot-prices\"", holderPage);
            Assert.DoesNotContain("13.25", holderPage);
            Assert.DoesNotContain("13.25", WebUtility.HtmlDecode(await oa.HtmlAsync($"/Recall/Lot/{one.Lot}")));   // commercial data stays out of the recall report

            // locked once the lot is in a finalized batch
            await FinalizeAsync(holderBatch);
            Assert.Contains("its price is locked", await one.Client.PostFlashAsync("/Me/Price", PriceForm(one.Lot, "14"), "/Me/Deliveries"));
            Assert.Contains("me-price-locked", await one.Client.HtmlAsync("/Me/Deliveries"));
            var qr = await _app.WithContextAsync(db => db.FinalHoneyProductions.Where(b => b.FinalHoneyProductionId == holderBatch).Select(b => b.FinalHoneyProductionQR).FirstAsync());
            Assert.DoesNotContain("13.25", await UserClient.Anonymous(_app).HtmlAsync($"/honeystory/{qr}"));   // O-7
            return 0;
        });

        [Fact]
        public async Task The_association_reads_prices_only_with_the_beekeepers_consent() => await WithPortalAsync(async () =>
        {
            var one = await PortalBeekeeperAsync("pt5");
            Assert.Contains("Price saved.", await one.Client.PostFlashAsync("/Me/Price", PriceForm(one.Lot, "11"), "/Me/Deliveries"));
            var association = await _app.CreateOrgAsync("zz_test pt5 association", null, (Capabilities.Association, null));
            await _app.AddMemberAsync(association, await _app.CreateUserAsync("zz_test_pt5_assoc"), OrgRoles.OrgAdmin);
            await _app.WithContextAsync(async db =>
            {
                db.BeekeeperAssociations.Add(new BeekeeperAssociation { BeekeeperId = one.Plant.BeekeeperId, AssociationOrgId = association, ValidFrom = new DateOnly(2026, 1, 1), CreatedAt = DateTime.UtcNow });
                return await db.SaveChangesAsync();
            });
            var assoc = await UserClient.LoginAsync(_app, "zz_test_pt5_assoc");
            Assert.Equal(404, await assoc.StatusAsync($"/Storefronts/Prices/{one.Plant.BeekeeperId}"));
            Assert.Contains("data-purpose=\"AssociationPrice\"", await one.Client.HtmlAsync("/Me/Privacy"));
            Assert.Contains("Consent given.", await one.Client.PostFlashAsync("/Me/Consent", new() { ["purpose"] = ConsentPurpose.AssociationPrice, ["give"] = "true" }, "/Me/Privacy"));
            Assert.Contains("11.00 TRY / kg", WebUtility.HtmlDecode(await assoc.HtmlAsync($"/Storefronts/Prices/{one.Plant.BeekeeperId}")));
            Assert.Contains("prices-link", await assoc.HtmlAsync("/Storefronts/Index"));
            Assert.Contains("Consent withdrawn", await one.Client.PostFlashAsync("/Me/Consent", new() { ["purpose"] = ConsentPurpose.AssociationPrice, ["give"] = "false" }, "/Me/Privacy"));
            Assert.Equal(404, await assoc.StatusAsync($"/Storefronts/Prices/{one.Plant.BeekeeperId}"));
            return 0;
        });

        [Fact]
        public async Task Withdrawing_the_public_name_consent_switches_the_honey_story_to_the_neutral_code() => await WithPortalAsync(async () =>
        {
            var one = await PortalBeekeeperAsync("pt6");
            var batch = await BatchWithAsync(one.Plant.OrgId, one.Lot);
            await FinalizeAsync(batch);
            var qr = await _app.WithContextAsync(db => db.FinalHoneyProductions.Where(b => b.FinalHoneyProductionId == batch).Select(b => b.FinalHoneyProductionQR).FirstAsync());
            var name = "Zz Custody pt6";
            var anonymous = UserClient.Anonymous(_app);
            async Task<string> StoryAsync() => WebUtility.HtmlDecode(await anonymous.HtmlAsync($"/honeystory/{qr}"));

            // D-85: a title typed before D-78 often contains the beekeeper's name; it is public only where the name is
            await _app.WithContextAsync(async db =>
            {
                var apiary = await db.Apiaries.SingleAsync(a => a.ApiaryId == one.Plant.ApiaryId);
                apiary.ApiaryTitle = name + " Arılığı";
                apiary.TitleSource = null;
                return await db.SaveChangesAsync();
            });

            // a legacy beekeeper without consent: the neutral code (HideUntilConsent, the default), and no typed apiary title
            var story = await StoryAsync();
            Assert.DoesNotContain(name, story);
            var code = await _app.WithContextAsync(db => db.BeekeeperStorefronts.Where(s => s.BeekeeperId == one.Plant.BeekeeperId).Select(s => s.Code).FirstAsync());
            Assert.Contains(code, story);
            Assert.Contains(code, await one.Client.HtmlAsync("/Me/Stories"));

            Assert.DoesNotContain("Arılığı", story);
            Assert.Contains("Apiary in", WebUtility.HtmlDecode(await anonymous.HtmlAsync($"/honeystory/{qr}?culture=en")));
            Assert.Contains("Consent given.", await one.Client.PostFlashAsync("/Me/Consent", new() { ["purpose"] = ConsentPurpose.PublicName, ["give"] = "true" }, "/Me/Privacy"));
            Assert.Contains(name, await StoryAsync());
            Assert.Contains(name + " Arılığı", await StoryAsync());
            Assert.Contains("Consent withdrawn", await one.Client.PostFlashAsync("/Me/Consent", new() { ["purpose"] = ConsentPurpose.PublicName, ["give"] = "false" }, "/Me/Privacy"));
            story = await StoryAsync();
            Assert.DoesNotContain(name, story);
            Assert.Contains(code, story);

            // ShowUntilReverified: a legacy beekeeper keeps the name until registered again
            var config = _app.Services.GetRequiredService<IConfiguration>();
            config["PublicNames:LegacyMode"] = "ShowUntilReverified";
            try
            {
                Assert.Contains(name, await StoryAsync());
                await _app.WithContextAsync(async db =>
                {
                    db.BeekeeperRegistrations.Add(new BeekeeperRegistration
                    {
                        BeekeeperId = one.Plant.BeekeeperId, RegisteringOrgId = one.Plant.OrgId, OfficerUserId = "seed", At = DateTime.UtcNow, Kind = RegistrationKind.Reverification,
                        Method = "SmsCode", RegistryFormatOk = true, OfficerAttestation = true
                    });
                    return await db.SaveChangesAsync();
                });
                Assert.DoesNotContain(name, await StoryAsync());
            }
            finally
            {
                config["PublicNames:LegacyMode"] = "HideUntilConsent";
            }
            return 0;
        });

        [Fact]
        public async Task A_beekeeper_account_gets_403_outside_the_portal_and_404_for_ids_that_are_not_theirs() => await WithPortalAsync(async () =>
        {
            var one = await PortalBeekeeperAsync("pt7");
            var two = await PortalBeekeeperAsync("pt8");
            foreach (var page in new[] { "/Intake/Index", $"/Intake/Details/{one.IntakeId}", "/OrgWorkspace", "/MyWork/Index", "/Samples/Index", "/Account/Profile",
                         $"/Recall/Lot/{one.Lot}", "/Registration", "/UserManagement", "/Beekeepers", $"/Intake/Labels/{one.IntakeId}" })
                Assert.Equal(403, await one.Client.StatusAsync(page));
            foreach (var (url, form) in new (string, Dictionary<string, string>)[]
                     {
                         ("/Intake/Record", new() { ["op"] = "send" }), ("/Account/UpdatePhone", new() { ["PhoneNumber"] = "+905320000999" }),
                         ("/Samples/Receive", new() { ["id"] = "1", ["sealIntact"] = "true" }), ($"/Intake/Review/{one.IntakeId}", new() { ["accept"] = "true" }),
                         ("/Account/ChangePassword", new() { ["CurrentPassword"] = "x", ["NewPassword"] = "y" })
                     })
                Assert.Equal(HttpStatusCode.Forbidden, (await one.Client.PostAsync(url, form, "/Me/Account")).StatusCode);
            // the public pages stay open; another beekeeper's lot is not found
            Assert.Equal(200, await one.Client.StatusAsync("/"));
            Assert.Equal(HttpStatusCode.NotFound, (await one.Client.PostAsync("/Me/Price", PriceForm(two.Lot, "5"), "/Me/Deliveries")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await one.Client.PostAsync("/Me/Price", PriceForm(999999, "5"), "/Me/Deliveries")).StatusCode);
            // a member is not let into the portal
            Assert.Equal(404, await one.Plant.Sao.StatusAsync("/Me"));
            // with the switch off the portal is closed
            _app.Services.GetRequiredService<IConfiguration>()["Features:BeekeeperAccounts"] = "false";
            Assert.Equal(404, await one.Client.StatusAsync("/Me"));
            return 0;
        });
    }
}
