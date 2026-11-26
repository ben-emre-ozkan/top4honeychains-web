using System.Net;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-98 (migration 035): "Other" in the delivery's honey type list. A name that stands for an existing type (title or
    // translation, ignoring case, Turkish letters, spaces and the word "honey") is refused; a new one is created with its class in
    // the delivery's transaction, bound by the SMS code, and the delivery still verifies after the type is merged away.
    public partial class CustodyIntakeTests
    {
        private static Dictionary<string, string> Other(Dictionary<string, string> f, string name, string? honeyClass)
        {
            var o = new Dictionary<string, string>(f) { ["HoneyTypeId"] = HoneyTypeCatalog.Other.ToString(), ["NewHoneyTypeName"] = name };
            if (honeyClass != null)
                o["NewHoneyTypeClass"] = honeyClass;
            return o;
        }

        [Fact]
        public async Task A_new_honey_type_is_created_with_the_delivery_and_a_known_name_is_refused() => await WithCustodyAsync(async () =>
        {
            var p = await PlantAsync("ht1");
            var page = await p.Sao.HtmlAsync($"/Intake/Create?number={p.BusinessNumber}");
            Assert.Contains("id=\"HoneyTypeId-other\"", page);
            Assert.Contains("name=\"NewHoneyTypeName\"", page);

            // a translation of an existing type counts as its name
            var known = await _app.WithContextAsync(async db =>
            {
                var t = new HoneyType { HoneyTypeTitle = "Zz Linden Honey", HoneyClass = "Floral" };
                db.HoneyTypes.Add(t);
                await db.SaveChangesAsync();
                db.ReferenceTranslations.Add(new ReferenceTranslation { Entity = RefEntity.HoneyType, EntityKey = t.HoneyTypeId.ToString(), Field = "Title", Culture = "tr", Value = "Zz Ihlamur Balı", UpdatedAt = DateTime.UtcNow });
                await db.SaveChangesAsync();
                return t;
            });
            async Task<string> SendAsync(Dictionary<string, string> f) =>
                await BodyAsync(await p.Sao.PostAsync("/Intake/Record", Op(f, "send"), $"/Intake/Create?number={p.BusinessNumber}"));
            Assert.Contains("This honey type already exists as Zz Linden Honey: choose it from the list.", await SendAsync(Other(Form(p, 1), "zz ıhlamur  BALI", "Floral")));
            Assert.Contains("This honey type already exists as Zz Linden Honey: choose it from the list.", await SendAsync(Other(Form(p, 1), "ZZ-LINDEN", "Pine")));
            Assert.Contains("Choose the honey class of the new honey type (floral, honeydew or pine).", await SendAsync(Other(Form(p, 1), "Zz Lavanta Balı", null)));
            Assert.Contains("Enter the name of the new honey type (2 to 100 characters).", await SendAsync(Other(Form(p, 1), " ", "Floral")));
            Assert.Equal(0, await _app.WithContextAsync(db => db.HoneyTypes.CountAsync(h => h.HoneyTypeTitle.StartsWith("Zz Lavanta"))));

            // a new name: created with the confirmed delivery, with its class and who added it
            var (intakeId, lot) = await ConfirmBySmsAsync(p, Other(Form(p, 1), "Zz Lavanta  Balı", "Floral"));
            var created = await _app.WithContextAsync(db => db.HoneyTypes.SingleAsync(h => h.HoneyTypeTitle == "Zz Lavanta Balı"));
            Assert.Equal(("Floral", p.OrgId), (created.HoneyClass, created.CreatedByOrgId));
            Assert.NotNull(created.CreatedByUserId);
            Assert.NotNull(created.CreatedAt);
            Assert.Equal(created.HoneyTypeId, await _app.WithContextAsync(db => db.RawHoneyProductions.Where(r => r.RawHoneyProductionId == lot).Select(r => r.RawHoneyProductionHoneyType).FirstAsync()));
            Assert.Contains("id=\"content-verified\"", await p.Sao.HtmlAsync($"/Intake/Details/{intakeId}"));
            // the next delivery must pick it
            Assert.Contains("This honey type already exists as Zz Lavanta Balı: choose it from the list.", await SendAsync(Other(Form(p, 1), "zz lavanta", "Floral")));

            // the platform administrator merges it into another type: the lot moves and the confirmed delivery still verifies
            await _app.CreateUserAsync("zz_test_ht1_admin", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "zz_test_ht1_admin");
            var merged = await admin.PostAsync($"/HoneyTypes/Merge/{created.HoneyTypeId}", new() { ["targetId"] = known.HoneyTypeId.ToString() }, $"/HoneyTypes/Merge/{created.HoneyTypeId}");
            Assert.True(merged.StatusCode == HttpStatusCode.Redirect, await merged.Content.ReadAsStringAsync());
            Assert.Equal(known.HoneyTypeId, await _app.WithContextAsync(db => db.RawHoneyProductions.Where(r => r.RawHoneyProductionId == lot).Select(r => r.RawHoneyProductionHoneyType).FirstAsync()));
            Assert.Contains("id=\"content-verified\"", await p.Sao.HtmlAsync($"/Intake/Details/{intakeId}"));
            // the merged-away name still leads to the target
            Assert.Contains("This honey type already exists as Zz Linden Honey: choose it from the list.", await SendAsync(Other(Form(p, 1), "Zz Lavanta", "Floral")));
            return 0;
        });
    }

    // D-98: the platform administrator merges honey types; the beekeeper's honey types at registration; the official number
    // ("İşletme tanımlama numarası") shown first and the TR-BEE number labelled "Platform number".
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class HoneyTypeTests
    {
        private readonly E2EApp _app;
        public HoneyTypeTests(E2EApp app) => _app = app;

        private Task<HoneyType> TypeAsync(string title, string honeyClass = "Floral") => _app.WithContextAsync(async db =>
        {
            var t = new HoneyType { HoneyTypeTitle = title, HoneyClass = honeyClass };
            db.HoneyTypes.Add(t);
            await db.SaveChangesAsync();
            return t;
        });

        [Fact]
        public async Task A_merge_moves_every_reference_and_removes_the_source()
        {
            var source = await TypeAsync("Zz Merge Source", "Honeydew");
            var target = await TypeAsync("Zz Merge Target");
            var (lot, limit, both, only) = await _app.WithContextAsync(async db =>
            {
                var parameter = await db.CriticalControlPointParameters.OrderBy(x => x.ParameterId).Select(x => x.ParameterId).FirstAsync();
                var l = new CriticalControlPointParameterLimit { ParameterId = parameter, HoneyTypeId = source.HoneyTypeId, MinValue = 1 };
                db.CriticalControlPointParameterLimits.Add(l);
                var b1 = new Beekeeper { BeekeeperId = Guid.NewGuid().ToString(), BeekeeperFirstName = "Zz", BeekeeperLastName = "Merge both", BeekeeperBusinessNumber = "TR-BEE-2026-zzm1", BeekeeperBirthDate = new DateOnly(1970, 1, 1), IsActive = true };
                var b2 = new Beekeeper { BeekeeperId = Guid.NewGuid().ToString(), BeekeeperFirstName = "Zz", BeekeeperLastName = "Merge only", BeekeeperBusinessNumber = "TR-BEE-2026-zzm2", BeekeeperBirthDate = new DateOnly(1970, 1, 1), IsActive = true };
                db.Beekeepers.AddRange(b1, b2);
                var a = new Apiary { ApiaryBeekeeperId = b1.BeekeeperId, ApiaryTitle = "zz_test merge apiary", ApiaryBeehivesCount = 5, ApiaryLocationLatitude = 40.7m, ApiaryLocationLongitude = 38.4m, ApiaryQRCode = Guid.NewGuid() };
                db.Apiaries.Add(a);
                await db.SaveChangesAsync();
                var r = new RawHoneyProduction
                {
                    RawHoneyProductionApiaryId = a.ApiaryId, RawHoneyProductionHoneyType = source.HoneyTypeId, RawHoneyProductionAmount = 1,
                    RawHoneyProductionPeriodId = await db.RawHoneyProductionPeriods.OrderBy(x => x.RawHoneyProductionPeriodId).Select(x => x.RawHoneyProductionPeriodId).FirstAsync()
                };
                db.RawHoneyProductions.Add(r);
                await db.SaveChangesAsync();
                db.BeekeeperHoneyTypes.AddRange(
                    new BeekeeperHoneyType { BeekeeperId = b1.BeekeeperId!, HoneyTypeId = source.HoneyTypeId },
                    new BeekeeperHoneyType { BeekeeperId = b1.BeekeeperId!, HoneyTypeId = target.HoneyTypeId },
                    new BeekeeperHoneyType { BeekeeperId = b2.BeekeeperId!, HoneyTypeId = source.HoneyTypeId });
                db.ReferenceTranslations.AddRange(
                    new ReferenceTranslation { Entity = RefEntity.HoneyType, EntityKey = source.HoneyTypeId.ToString(), Field = "Title", Culture = "tr", Value = "Zz Birleşen", UpdatedAt = DateTime.UtcNow },
                    new ReferenceTranslation { Entity = RefEntity.HoneyType, EntityKey = source.HoneyTypeId.ToString(), Field = "Title", Culture = "pl", Value = "Zz Scalany", UpdatedAt = DateTime.UtcNow },
                    new ReferenceTranslation { Entity = RefEntity.HoneyType, EntityKey = target.HoneyTypeId.ToString(), Field = "Title", Culture = "tr", Value = "Zz Hedef", UpdatedAt = DateTime.UtcNow });
                await db.SaveChangesAsync();
                return (r.RawHoneyProductionId, l.LimitId, b1.BeekeeperId!, b2.BeekeeperId!);
            });

            await _app.CreateUserAsync("zz_test_merge_admin", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "zz_test_merge_admin");
            var page = await admin.HtmlAsync($"/HoneyTypes/Merge/{source.HoneyTypeId}");
            Assert.Contains("id=\"merge-references\"", page);
            Assert.DoesNotContain($"<option value=\"{source.HoneyTypeId}\"", page);

            // refused: into itself
            var self = WebUtility.HtmlDecode(await (await admin.PostAsync($"/HoneyTypes/Merge/{source.HoneyTypeId}", new() { ["targetId"] = source.HoneyTypeId.ToString() },
                $"/HoneyTypes/Merge/{source.HoneyTypeId}")).Content.ReadAsStringAsync());
            Assert.Contains("A honey type cannot be merged into itself.", self);

            Assert.Equal("Zz Merge Source was merged into Zz Merge Target.", await admin.PostFlashAsync($"/HoneyTypes/Merge/{source.HoneyTypeId}",
                new() { ["targetId"] = target.HoneyTypeId.ToString() }, $"/HoneyTypes/Merge/{source.HoneyTypeId}"));
            await _app.WithContextAsync(async db =>
            {
                Assert.False(await db.HoneyTypes.AnyAsync(h => h.HoneyTypeId == source.HoneyTypeId));
                Assert.Equal(target.HoneyTypeId, await db.RawHoneyProductions.Where(r => r.RawHoneyProductionId == lot).Select(r => r.RawHoneyProductionHoneyType).FirstAsync());
                Assert.Equal(target.HoneyTypeId, await db.CriticalControlPointParameterLimits.Where(l => l.LimitId == limit).Select(l => l.HoneyTypeId).FirstAsync());
                Assert.Equal(new[] { target.HoneyTypeId }, await db.BeekeeperHoneyTypes.Where(b => b.BeekeeperId == both).Select(b => b.HoneyTypeId).ToListAsync());
                Assert.Equal(new[] { target.HoneyTypeId }, await db.BeekeeperHoneyTypes.Where(b => b.BeekeeperId == only).Select(b => b.HoneyTypeId).ToListAsync());
                // the target keeps its own Turkish title; the Polish one it lacked moves over; nothing stays on the source
                var titles = await db.ReferenceTranslations.Where(t => t.Entity == RefEntity.HoneyType && t.EntityKey == target.HoneyTypeId.ToString()).ToDictionaryAsync(t => t.Culture, t => t.Value);
                Assert.Equal("Zz Hedef", titles["tr"]);
                Assert.Equal("Zz Scalany", titles["pl"]);
                Assert.False(await db.ReferenceTranslations.AnyAsync(t => t.Entity == RefEntity.HoneyType && t.EntityKey == source.HoneyTypeId.ToString()));
                var merge = await db.HoneyTypeMerges.SingleAsync(m => m.SourceHoneyTypeId == source.HoneyTypeId);
                Assert.Equal(("Zz Merge Source", "Honeydew", target.HoneyTypeId), (merge.SourceTitle, merge.SourceClass, merge.TargetHoneyTypeId));
                Assert.True(await db.AuditEntries.AnyAsync(a => a.Action == "Merge" && a.Entity == "HoneyTypes" && a.EntityKey == source.HoneyTypeId.ToString()));
                return 0;
            });

            // two limits of their own for the same parameter: refused
            var c = await TypeAsync("Zz Merge Clash C");
            var d = await TypeAsync("Zz Merge Clash D");
            await _app.WithContextAsync(async db =>
            {
                var parameter = await db.CriticalControlPointParameters.OrderBy(x => x.ParameterId).Select(x => x.ParameterId).FirstAsync();
                db.CriticalControlPointParameterLimits.AddRange(new CriticalControlPointParameterLimit { ParameterId = parameter, HoneyTypeId = c.HoneyTypeId, MaxValue = 5 },
                    new CriticalControlPointParameterLimit { ParameterId = parameter, HoneyTypeId = d.HoneyTypeId, MaxValue = 6 });
                return await db.SaveChangesAsync();
            });
            var clash = WebUtility.HtmlDecode(await (await admin.PostAsync($"/HoneyTypes/Merge/{c.HoneyTypeId}", new() { ["targetId"] = d.HoneyTypeId.ToString() },
                $"/HoneyTypes/Merge/{c.HoneyTypeId}")).Content.ReadAsStringAsync());
            Assert.Contains("Both honey types have their own limit for", clash);
            Assert.True(await _app.WithContextAsync(db => db.HoneyTypes.AnyAsync(h => h.HoneyTypeId == c.HoneyTypeId)));

            // only the platform administrator
            var org = await _app.CreateOrgAsync("zz_test merge plant", null, (Capabilities.Processing, null));
            await _app.AddMemberAsync(org, await _app.CreateUserAsync("zz_test_merge_sao"), OrgRoles.SampleAcceptanceOfficer);
            var sao = await UserClient.LoginAsync(_app, "zz_test_merge_sao");
            Assert.Contains("/Account/AccessDenied", (await sao.GetAsync($"/HoneyTypes/Merge/{c.HoneyTypeId}")).Headers.Location?.ToString());
        }

        [Fact]
        public async Task The_beekeepers_honey_types_are_saved_with_the_registration_and_changed_later()
        {
            var org = await _app.CreateOrgAsync("zz_test honey İlçe Tarım Müdürlüğü", null, (Capabilities.BeekeeperRegistration, null));
            await _app.AddMemberAsync(org, await _app.CreateUserAsync("zz_test_honey_ro"), OrgRoles.RegistrationOfficer);
            var ro = await UserClient.LoginAsync(_app, "zz_test_honey_ro");
            var page = await ro.HtmlAsync("/Registration/New?registryNumber=TR281234500094");
            Assert.Contains("id=\"registration-honey-types\"", page);
            Assert.Contains("id=\"honey-type-other\"", page);
            var known = await _app.WithContextAsync(db => db.HoneyTypes.OrderBy(h => h.HoneyTypeId).Select(h => h.HoneyTypeId).Take(2).ToListAsync());
            var district = await _app.WithContextAsync(db => db.Districts.Where(d => d.ProvinceCode == "28").OrderBy(d => d.DistrictId).Select(d => d.DistrictId).FirstAsync());
            var form = new List<KeyValuePair<string, string>>
            {
                new("DraftKey", ""), new("Code", ""), new("RegistryNumber", "TR281234500094"), new("FirstName", "Zz Honey"), new("LastName", "Zz Types"), new("BirthDate", "1975-03-02"),
                new("Phone", "+905320000494"), new("Language", "tr"), new("ApiaryLatitude", "40.7"), new("ApiaryLongitude", "38.4"), new("ApiaryProvince", "28"),
                new("ApiaryDistrictId", district.ToString()), new("ApiaryLocality", "Zz Yayla"), new("ApiaryHives", "12"), new("PublicName", "false"),
                new("PortalNotifications", "false"), new("Attest", "true"),
                new("HoneyTypeIds", known[0].ToString()), new("HoneyTypeIds", known[1].ToString()),
                new("OtherHoneyType", "true"), new("NewHoneyTypeName", "Zz Geven Balı"), new("NewHoneyTypeClass", "Pine")
            };
            List<KeyValuePair<string, string>> With(string op, params (string Key, string Value)[] set)
            {
                var list = form.Where(kv => set.All(s => s.Key != kv.Key)).ToList();
                list.AddRange(set.Select(s => new KeyValuePair<string, string>(s.Key, s.Value)));
                list.Add(new("op", op));
                return list;
            }

            // an existing name is refused before the code is sent
            var refused = WebUtility.HtmlDecode(await (await ro.PostPairsAsync("/Registration/Submit", With("send", ("NewHoneyTypeName", "PINE honey")), "/Registration/New")).Content.ReadAsStringAsync());
            Assert.Contains("This honey type already exists as Pine Honey: choose it from the list.", refused);

            var html = WebUtility.HtmlDecode(await (await ro.PostPairsAsync("/Registration/Submit", With("send"), "/Registration/New")).Content.ReadAsStringAsync());
            var draft = Regex.Match(html, "name=\"DraftKey\" value=\"([0-9a-f]*)\"").Groups[1].Value;
            Assert.Equal(32, draft.Length);
            var sms = await _app.WithContextAsync(db => db.SmsMessages.Where(m => m.SubjectKey == draft).OrderByDescending(m => m.SmsId).FirstAsync());
            var code = Regex.Match(sms.BodyStored, @"\b\d{6}\b").Value;
            // the code covers the honey types: another choice after sending voids it
            var changed = WebUtility.HtmlDecode(await (await ro.PostPairsAsync("/Registration/Submit", With("confirm", ("DraftKey", draft), ("Code", code), ("NewHoneyTypeClass", "Floral")), "/Registration/New")).Content.ReadAsStringAsync());
            Assert.Contains("The details changed after the code was sent. Send a new code.", changed);
            Assert.Equal(0, await _app.WithContextAsync(db => db.HoneyTypes.CountAsync(h => h.HoneyTypeTitle == "Zz Geven Balı")));

            // a new code (after the resend pause)
            _app.Clock.Advance(TimeSpan.FromSeconds(61));
            try
            {
                await ro.PostPairsAsync("/Registration/Submit", With("send", ("DraftKey", draft)), "/Registration/New");
                sms = await _app.WithContextAsync(db => db.SmsMessages.Where(m => m.SubjectKey == draft).OrderByDescending(m => m.SmsId).FirstAsync());
                code = Regex.Match(sms.BodyStored, @"\b\d{6}\b").Value;
                Assert.Equal("The beekeeper is registered and verified.", await ro.FlashAfterAsync(await ro.PostPairsAsync("/Registration/Submit", With("confirm", ("DraftKey", draft), ("Code", code)), "/Registration/New")));
            }
            finally
            {
                _app.Clock.Reset();
            }

            var (beekeeperId, registrationId, geven) = await _app.WithContextAsync(async db =>
            {
                var bk = await db.Beekeepers.Where(b => b.BeekeeperFirstName == "Zz Honey").Select(b => b.BeekeeperId).SingleAsync();
                var reg = await db.BeekeeperRegistrations.Where(r => r.BeekeeperId == bk).Select(r => r.RegistrationId).SingleAsync();
                var type = await db.HoneyTypes.SingleAsync(h => h.HoneyTypeTitle == "Zz Geven Balı");
                return (bk!, reg, type);
            });
            Assert.Equal(("Pine", (int?)org), (geven.HoneyClass, geven.CreatedByOrgId));
            Assert.Equal(new[] { known[0], known[1], geven.HoneyTypeId }.OrderBy(i => i),
                await _app.WithContextAsync(db => db.BeekeeperHoneyTypes.Where(b => b.BeekeeperId == beekeeperId).Select(b => b.HoneyTypeId).OrderBy(i => i).ToListAsync()));

            // the details show them; the officer changes them later
            var details = WebUtility.HtmlDecode(await ro.HtmlAsync($"/Registration/Details/{registrationId}"));
            Assert.Contains("Zz Geven Balı", details);
            Assert.Contains("id=\"edit-honey-types\"", details);
            Assert.Equal("The beekeeper's honey types are saved.", await ro.PostFlashAsync($"/Registration/SetHoneyTypes/{registrationId}",
                new() { ["HoneyTypeIds"] = known[1].ToString() }, $"/Registration/Details/{registrationId}"));
            Assert.Equal(new[] { known[1] }, await _app.WithContextAsync(db => db.BeekeeperHoneyTypes.Where(b => b.BeekeeperId == beekeeperId).Select(b => b.HoneyTypeId).ToListAsync()));
            Assert.Equal("This honey type already exists as Zz Geven Balı: choose it from the list.", await ro.PostFlashAsync($"/Registration/SetHoneyTypes/{registrationId}",
                new() { ["OtherHoneyType"] = "true", ["NewHoneyTypeName"] = "zz GEVEN", ["NewHoneyTypeClass"] = "Pine" }, $"/Registration/Details/{registrationId}"));

            // D-98 (15): the official number first, the platform number (the user name) labelled as such
            Assert.Contains("Holding identification number", details);
            Assert.Matches("id=\"platform-number\">Platform number: TR-BEE-", details);
        }

        [Fact]
        public async Task The_numbers_are_labelled_in_turkish_and_the_sign_in_page_names_the_platform_number()
        {
            var login = await UserClient.Anonymous(_app).HtmlAsync("/Account/Login");
            Assert.Contains("id=\"login-beekeeper-hint\"", login);
            Assert.Contains("platform number", WebUtility.HtmlDecode(login));

            await _app.CreateUserAsync("zz_test_numbers_admin", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "zz_test_numbers_admin");
            var list = WebUtility.HtmlDecode(await admin.HtmlAsync("/Beekeepers/Index"));
            Assert.Contains("<th>Holding identification number</th>", list);
            Assert.Contains("<th>Holding identification number</th>", WebUtility.HtmlDecode(await admin.HtmlAsync("/Apiaries/Index")));
            var registry = WebUtility.HtmlDecode(await admin.HtmlAsync("/Registry/Index"));
            Assert.Contains("<th>Holding identification number</th><th>", registry);
            Assert.Contains("<th>Platform number</th>", registry);

            using var turkish = _app.CreateClient();
            turkish.DefaultRequestHeaders.AcceptLanguage.ParseAdd("tr");
            var tr = WebUtility.HtmlDecode(await turkish.GetStringAsync("/Account/Login"));
            Assert.Contains("platform numaranızdır", tr);
        }
    }
}
