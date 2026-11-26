using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.Identity;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-78 P1 (D78-SPEC 4.1, 4.2, F2-F5): registration by a district directorate's registration officer, within the own
    // directorate only; registry number format and province check, SMS code to the beekeeper's phone, the officer's
    // attestation; signed photo as fallback, accepted by the Compliance Officer (never by the person who uploaded it);
    // consent and notice captured at registration; generated apiary titles; legacy beekeepers stay locked until re-verified.
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class RegistrationTests
    {
        private readonly E2EApp _app;
        public RegistrationTests(E2EApp app) => _app = app;

        private record Directorate(int OrgId, UserClient Officer, string OfficerId, UserClient Co, string CoId);

        // a verified district directorate in Giresun (plate 28) with a registration officer and a Compliance Officer
        private async Task<Directorate> DirectorateAsync(string tag)
        {
            var org = await _app.CreateOrgAsync($"zz_test {tag} İlçe Tarım Müdürlüğü", null, (Capabilities.BeekeeperRegistration, null));
            await _app.WithContextAsync(async db =>
            {
                var o = await db.Organizations.FirstAsync(x => x.OrgId == org);
                o.Province = "Giresun";
                return await db.SaveChangesAsync();
            });
            var ro = await _app.CreateUserAsync($"zz_test_{tag}_ro");
            var co = await _app.CreateUserAsync($"zz_test_{tag}_co");
            await _app.AddMemberAsync(org, ro, OrgRoles.RegistrationOfficer);
            await _app.AddMemberAsync(org, co, OrgRoles.ComplianceOfficer);
            return new Directorate(org, await UserClient.LoginAsync(_app, $"zz_test_{tag}_ro"), ro, await UserClient.LoginAsync(_app, $"zz_test_{tag}_co"), co);
        }

        private Task<int> DistrictAsync(string province) =>
            _app.WithContextAsync(db => db.Districts.Where(d => d.ProvinceCode == province).OrderBy(d => d.DistrictId).Select(d => d.DistrictId).FirstAsync());

        private async Task<Dictionary<string, string>> FormAsync(string registry, string phone, string locality, bool publicName = true)
        {
            return new()
            {
                ["RegistryNumber"] = registry, ["FirstName"] = "Zz Ayşe", ["LastName"] = "Zz Test", ["BirthDate"] = "1970-05-04", ["Phone"] = phone,
                ["Language"] = "tr", ["ApiaryLatitude"] = "40.7", ["ApiaryLongitude"] = "38.4", ["ApiaryProvince"] = "28",
                ["ApiaryDistrictId"] = (await DistrictAsync("28")).ToString(), ["ApiaryLocality"] = locality, ["ApiaryHives"] = "40",
                ["PublicName"] = publicName ? "true" : "false", ["PortalNotifications"] = "false", ["Attest"] = "true"
            };
        }

        private static string DraftKeyOf(string html) => Regex.Match(html, "name=\"DraftKey\" value=\"([0-9a-f]+)\"").Groups[1].Value;

        private Task<SmsMessage> LastSmsAsync(string subjectKey) =>
            _app.WithContextAsync(db => db.SmsMessages.Where(m => m.SubjectKey == subjectKey).OrderByDescending(m => m.SmsId).FirstAsync());

        private static async Task<string> BodyAsync(HttpResponseMessage r) => WebUtility.HtmlDecode(await r.Content.ReadAsStringAsync());

        [Fact]
        public async Task An_officer_registers_a_beekeeper_confirmed_by_an_sms_code()
        {
            var d = await DirectorateAsync("reg1");
            Assert.Contains("id=\"registration-find\"", await d.Officer.HtmlAsync("/Registration/Index"));
            var found = await d.Officer.PostAsync("/Registration/Find", new() { ["number"] = "TR 281234500001" }, "/Registration/Index");
            Assert.Equal("/Registration/New?registryNumber=TR281234500001", found.Headers.Location?.ToString());

            var form = await FormAsync("TR281234500001", "0532 000 04 01", "Kümbet Yaylası");
            var sent = await d.Officer.PostAsync("/Registration/Submit?op=send", Op(form, "send"), "/Registration/New");
            var html = await BodyAsync(sent);
            Assert.Contains("id=\"registration-confirm\"", html);
            var draft = DraftKeyOf(html);
            Assert.Equal(32, draft.Length);

            var sms = await LastSmsAsync(draft);
            Assert.Equal(SmsPurpose.Registration, sms.Purpose);
            Assert.Equal("tr", sms.Language);
            Assert.Contains("zz_test reg1 İlçe Tarım Müdürlüğü", sms.BodyStored);
            Assert.Contains($"/privacy?org={d.OrgId}", sms.BodyStored);
            Assert.Contains("EVET", sms.BodyStored);   // the public-name choice is in the message
            var code = Regex.Match(sms.BodyStored, @"\b\d{6}\b").Value;

            // a changed field after sending voids the code
            form["DraftKey"] = draft;
            var changed = new Dictionary<string, string>(form) { ["LastName"] = "Zz Other", ["Code"] = code };
            Assert.Contains("The details changed after the code was sent", await BodyAsync(await d.Officer.PostAsync("/Registration/Submit", Op(changed, "confirm"), "/Registration/New")));

            // a new code (not within the same minute), then the confirmation
            Assert.Contains("Wait a minute before sending a new code.", await BodyAsync(await d.Officer.PostAsync("/Registration/Submit", Op(form, "send"), "/Registration/New")));
            HttpResponseMessage confirmed;
            try
            {
                _app.Clock.Advance(TimeSpan.FromSeconds(61));
                var resent = await d.Officer.PostAsync("/Registration/Submit", Op(form, "send"), "/Registration/New");
                Assert.Contains("id=\"registration-confirm\"", await BodyAsync(resent));
                code = Regex.Match((await LastSmsAsync(draft)).BodyStored, @"\b\d{6}\b").Value;
                confirmed = await d.Officer.PostAsync("/Registration/Submit", Op(new Dictionary<string, string>(form) { ["Code"] = code }, "confirm"), "/Registration/New");
            }
            finally
            {
                _app.Clock.Reset();
            }
            Assert.Equal(HttpStatusCode.Redirect, confirmed.StatusCode);
            Assert.Equal("The beekeeper is registered and verified.", await d.Officer.FlashAfterAsync(confirmed));

            var regId = int.Parse(confirmed.Headers.Location!.ToString().Split('/').Last());
            var r = await _app.WithContextAsync(db => db.BeekeeperRegistrations.Include(x => x.Beekeeper).ThenInclude(b => b!.Apiaries).FirstAsync(x => x.RegistrationId == regId));
            Assert.Equal(RegistrationKind.New, r.Kind);
            Assert.Equal(AcceptanceMethod.SmsCode, r.Method);
            Assert.Equal(EvidenceStatus.Accepted, r.EvidenceStatus);
            Assert.True(r.OfficerAttestation);
            Assert.True(r.RegistryProvinceMatches);
            Assert.NotNull(r.AttestationNoticeId);
            Assert.Equal("+905320000401", r.Beekeeper!.BeekeeperPhoneNumber);
            Assert.Equal("tr", r.Beekeeper.PreferredLanguage);
            Assert.StartsWith("TR-BEE-", r.Beekeeper.BeekeeperBusinessNumber);
            var apiary = Assert.Single(r.Beekeeper.Apiaries);
            Assert.StartsWith("Giresun – ", apiary.ApiaryTitle);
            Assert.EndsWith(" – Kümbet Yaylası", apiary.ApiaryTitle);
            Assert.Equal("Generated", apiary.TitleSource);
            Assert.Equal(RegistryStatus.Verified, await _app.WithContextAsync(db => db.BeekeeperRegistry.Where(x => x.BeekeeperId == r.BeekeeperId).Select(x => x.Status).FirstAsync()));

            // the notice and the consent are part of the registration (F4)
            var acceptance = Assert.Single(await _app.WithContextAsync(db => db.NoticeAcceptances.Where(a => a.BeekeeperId == r.BeekeeperId).ToListAsync()));
            Assert.Equal(AcceptanceMethod.SmsCode, acceptance.Method);
            Assert.Equal(r.CodeId, acceptance.CodeId);
            var consents = await _app.WithContextAsync(db => db.Consents.Where(c => c.BeekeeperId == r.BeekeeperId).ToListAsync());
            var consent = Assert.Single(consents);
            Assert.Equal(ConsentPurpose.PublicName, consent.Purpose);
            Assert.Equal(AcceptanceMethod.SmsCode, consent.Method);
            Assert.Equal(d.OrgId, consent.RegisteringOrgId);
            Assert.Null(consent.AssociationOrgId);
            Assert.Equal("platform-2026-11-draft1", consent.TextVersion);

            // no account: beekeeper accounts are off (O-15)
            Assert.False(await _app.WithContextAsync(db => db.BeekeeperAccounts.AnyAsync(a => a.BeekeeperId == r.BeekeeperId)));
            Assert.Contains("Kümbet Yaylası", WebUtility.HtmlDecode(await d.Officer.HtmlAsync($"/Registration/Details/{regId}")));
        }

        private static Dictionary<string, string> Op(Dictionary<string, string> form, string op) => new(form) { ["op"] = op };

        // D-88: the registration officer links the beekeeper's association; the SMS code covers the choice; no administrator step
        [Fact]
        public async Task The_officer_links_the_beekeepers_association_at_registration()
        {
            var d = await DirectorateAsync("reg88");
            var association = await _app.CreateOrgAsync("zz_test reg88 Arıcılar Birliği", null, (Capabilities.Association, null));
            var lab = await _app.CreateOrgAsync("zz_test reg88 Lab", null, (Capabilities.Laboratory, "ZZ-88"));
            Assert.Contains($"value=\"{association}\"", await d.Officer.HtmlAsync("/Registration/New?registryNumber=TR281234508801"));

            var form = await FormAsync("TR281234508801", "0532 000 88 01", "Zz Yayla");
            Assert.Contains("Choose an organization with the Association capability.",
                await BodyAsync(await d.Officer.PostAsync("/Registration/Submit", Op(new Dictionary<string, string>(form) { ["AssociationOrgId"] = lab.ToString() }, "send"), "/Registration/New")));

            form["AssociationOrgId"] = association.ToString();
            var sent = await BodyAsync(await d.Officer.PostAsync("/Registration/Submit", Op(form, "send"), "/Registration/New"));
            form["DraftKey"] = DraftKeyOf(sent);
            var code = Regex.Match((await LastSmsAsync(form["DraftKey"])).BodyStored, @"\b\d{6}\b").Value;
            // another association after the code was sent voids the code; a new code confirms the registration
            Assert.Contains("The details changed after the code was sent", await BodyAsync(await d.Officer.PostAsync("/Registration/Submit",
                Op(new Dictionary<string, string>(form) { ["AssociationOrgId"] = "", ["Code"] = code }, "confirm"), "/Registration/New")));
            HttpResponseMessage confirmed;
            try
            {
                _app.Clock.Advance(TimeSpan.FromSeconds(61));
                await d.Officer.PostAsync("/Registration/Submit", Op(form, "send"), "/Registration/New");
                code = Regex.Match((await LastSmsAsync(form["DraftKey"])).BodyStored, @"\b\d{6}\b").Value;
                confirmed = await d.Officer.PostAsync("/Registration/Submit", Op(new Dictionary<string, string>(form) { ["Code"] = code }, "confirm"), "/Registration/New");
            }
            finally
            {
                _app.Clock.Reset();
            }
            Assert.Equal("The beekeeper is registered and verified.", await d.Officer.FlashAfterAsync(confirmed));

            var regId = int.Parse(confirmed.Headers.Location!.ToString().Split('/').Last());
            var beekeeperId = await _app.WithContextAsync(db => db.BeekeeperRegistrations.Where(r => r.RegistrationId == regId).Select(r => r.BeekeeperId).FirstAsync());
            var link = Assert.Single(await _app.WithContextAsync(db => db.BeekeeperAssociations.Where(a => a.BeekeeperId == beekeeperId).ToListAsync()));
            Assert.Equal(association, link.AssociationOrgId);
            Assert.Null(link.ValidTo);
            Assert.Equal(d.OfficerId, link.CreatedByUserId);
        }

        [Fact]
        public async Task The_registry_number_format_and_province_are_checked()
        {
            var d = await DirectorateAsync("reg2");
            Assert.Equal("The number must be TR, a province code 01-81 and 10 digits (e.g. TR061234567890).",
                await d.Officer.PostFlashAsync("/Registration/Find", new() { ["number"] = "TR99123" }, "/Registration/Index"));

            // an Ankara number (06) at a Giresun directorate needs a reason
            var form = await FormAsync("TR061234500002", "+905320000402", "Kale Mevkii");
            Assert.Contains("The holding identification number belongs to another province than the directorate.", await BodyAsync(await d.Officer.PostAsync("/Registration/Submit", Op(form, "send"), "/Registration/New")));
            form["ProvinceMismatchReason"] = "zz_test apiary across the border";
            Assert.Contains("id=\"registration-confirm\"", await BodyAsync(await d.Officer.PostAsync("/Registration/Submit", Op(form, "send"), "/Registration/New")));

            // the attestation is required
            var unattested = await FormAsync("TR281234500003", "+905320000403", "Kale Mevkii");
            unattested["Attest"] = "false";
            Assert.Contains("Confirm the attestation", await BodyAsync(await d.Officer.PostAsync("/Registration/Submit", Op(unattested, "send"), "/Registration/New")));
        }

        [Fact]
        public async Task A_signed_photo_waits_for_the_compliance_officer_who_is_not_the_uploader()
        {
            var d = await DirectorateAsync("reg3");
            var form = await FormAsync("TR281234500004", "", "Yayla Yolu");
            form["NoPhone"] = "true";
            var saved = await d.Officer.PostMultipartAsync("/Registration/Submit", Op(form, "photo"), Pdf.File("evidence"), "/Registration/New");
            Assert.Equal(HttpStatusCode.Redirect, saved.StatusCode);
            var regId = int.Parse(saved.Headers.Location!.ToString().Split('/').Last());
            var r = await _app.WithContextAsync(db => db.BeekeeperRegistrations.Include(x => x.Beekeeper).FirstAsync(x => x.RegistrationId == regId));
            Assert.Equal(EvidenceStatus.PendingReview, r.EvidenceStatus);
            Assert.Equal(AcceptanceMethod.SignedPhoto, r.Method);
            Assert.Null(r.Beekeeper!.BeekeeperPhoneNumber);
            Assert.Equal(RegistryStatus.Pending, await _app.WithContextAsync(db => db.BeekeeperRegistry.Where(x => x.BeekeeperId == r.BeekeeperId).Select(x => x.Status).FirstAsync()));

            // the uploader cannot review
            Assert.Contains("id=\"evidence-waiting\"", await d.Officer.HtmlAsync($"/Registration/Details/{regId}"));
            Assert.Equal("You recorded this registration; another person must review its evidence.",
                await d.Officer.PostFlashAsync($"/Registration/Review/{regId}", new() { ["accept"] = "true" }, $"/Registration/Details/{regId}"));

            // the beekeeper cannot deliver while the photo waits
            var plant = await _app.CreateOrgAsync("zz_test reg3 plant", null, (Capabilities.Processing, null));
            await _app.AddMemberAsync(plant, await _app.CreateUserAsync("zz_test_reg3_sao"), OrgRoles.SampleAcceptanceOfficer);
            var sao = await UserClient.LoginAsync(_app, "zz_test_reg3_sao");
            Assert.Contains("No active beekeeper with this number.", await sao.HtmlAsync($"/Intake/Create?number={r.Beekeeper.BeekeeperBusinessNumber}"));

            // the Compliance Officer sees it in My work, opens the photo and accepts
            Assert.Contains($"/Registration/Details/{regId}", await d.Co.HtmlAsync("/MyWork/Index"));
            Assert.Equal(HttpStatusCode.OK, (await d.Co.GetAsync($"/Registration/Evidence/{regId}")).StatusCode);
            // C2 (D-84): the recording role, the Compliance Officer and the admin; another role 403, another organization 404; audited
            Assert.Equal(HttpStatusCode.OK, (await d.Officer.GetAsync($"/Registration/Evidence/{regId}")).StatusCode);
            Assert.True(await _app.WithContextAsync(db => db.AuditEntries.AnyAsync(a => a.Action == "Download" && a.Entity == "BeekeeperRegistrations" && a.EntityKey == regId.ToString())));
            await _app.AddMemberAsync(d.OrgId, await _app.CreateUserAsync("zz_test_reg3_staff"), OrgRoles.Staff);
            Assert.Equal(403, await (await UserClient.LoginAsync(_app, "zz_test_reg3_staff")).StatusAsync($"/Registration/Evidence/{regId}"));
            Assert.Equal(404, await sao.StatusAsync($"/Registration/Evidence/{regId}"));
            Assert.Equal("Evidence accepted; the holding identification number is verified.",
                await d.Co.PostFlashAsync($"/Registration/Review/{regId}", new() { ["accept"] = "true" }, $"/Registration/Details/{regId}"));
            Assert.Equal(RegistryStatus.Verified, await _app.WithContextAsync(db => db.BeekeeperRegistry.Where(x => x.BeekeeperId == r.BeekeeperId).Select(x => x.Status).FirstAsync()));
            Assert.Equal(AcceptanceMethod.SignedPhoto, await _app.WithContextAsync(db => db.NoticeAcceptances.Where(a => a.BeekeeperId == r.BeekeeperId).Select(a => a.Method).FirstAsync()));
            Assert.DoesNotContain("No active beekeeper with this number.", await sao.HtmlAsync($"/Intake/Create?number={r.Beekeeper.BeekeeperBusinessNumber}"));

            // O-17: no sign-in without a phone confirmed by SMS
            Assert.DoesNotContain("id=\"issue-sign-in\"", await d.Officer.HtmlAsync($"/Registration/Details/{regId}"));
        }

        [Fact]
        public async Task A_rejected_photo_withdraws_its_consents_and_needs_a_reason()
        {
            var d = await DirectorateAsync("reg4");
            var form = await FormAsync("TR281234500005", "+905320000405", "Dere Boyu");
            form["FallbackReason"] = "zz_test no signal";
            var saved = await d.Officer.PostMultipartAsync("/Registration/Submit", Op(form, "photo"), Pdf.File("evidence"), "/Registration/New");
            var regId = int.Parse(saved.Headers.Location!.ToString().Split('/').Last());
            Assert.Equal("Give the reason for the rejection.",
                await d.Co.PostFlashAsync($"/Registration/Review/{regId}", new() { ["accept"] = "false" }, $"/Registration/Details/{regId}"));
            Assert.Equal("Evidence rejected; the beekeeper cannot deliver until registered again.",
                await d.Co.PostFlashAsync($"/Registration/Review/{regId}", new() { ["accept"] = "false", ["reason"] = "zz_test unreadable" }, $"/Registration/Details/{regId}"));
            var bkId = await _app.WithContextAsync(db => db.BeekeeperRegistrations.Where(x => x.RegistrationId == regId).Select(x => x.BeekeeperId).FirstAsync());
            Assert.All(await _app.WithContextAsync(db => db.Consents.Where(c => c.BeekeeperId == bkId).ToListAsync()), c => Assert.NotNull(c.WithdrawnAt));
            Assert.Equal(RegistryStatus.Rejected, await _app.WithContextAsync(db => db.BeekeeperRegistry.Where(x => x.BeekeeperId == bkId).Select(x => x.Status).FirstAsync()));
        }

        [Fact]
        public async Task An_officer_works_only_within_the_own_directorate()
        {
            var a = await DirectorateAsync("reg5a");
            var b = await DirectorateAsync("reg5b");
            var form = await FormAsync("TR281234500006", "", "Ova");
            form["NoPhone"] = "true";
            var saved = await a.Officer.PostMultipartAsync("/Registration/Submit", Op(form, "photo"), Pdf.File("evidence"), "/Registration/New");
            var regId = int.Parse(saved.Headers.Location!.ToString().Split('/').Last());

            Assert.Equal(404, await b.Officer.StatusAsync($"/Registration/Details/{regId}"));
            Assert.Equal(404, await b.Officer.StatusAsync($"/Registration/Evidence/{regId}"));
            Assert.DoesNotContain("TR281234500006", await b.Officer.HtmlAsync("/Registration/Index"));
            Assert.Equal("This beekeeper is registered by another directorate.",
                await b.Officer.PostFlashAsync("/Registration/Find", new() { ["number"] = "TR281234500006" }, "/Registration/Index"));
            // the other directorate's Compliance Officer cannot review it either
            var refused = await b.Co.PostAsync($"/Registration/Review/{regId}", new() { ["accept"] = "true" }, "/MyWork/Index");
            Assert.Equal(HttpStatusCode.NotFound, refused.StatusCode);

            // a member without the registration role (Staff) has no registration pages
            await _app.AddMemberAsync(a.OrgId, await _app.CreateUserAsync("zz_test_reg5_staff"), OrgRoles.Staff);
            var staff = await UserClient.LoginAsync(_app, "zz_test_reg5_staff");
            Assert.Equal(404, await staff.StatusAsync("/Registration/Index"));
            Assert.Equal(404, await staff.StatusAsync("/Registration/New"));
        }

        [Fact]
        public async Task A_legacy_beekeeper_stays_locked_until_re_verified_and_gets_no_account_while_accounts_are_off()
        {
            var d = await DirectorateAsync("reg6");
            // a beekeeper from before D-78 with its locked legacy login (BeekeeperId = AspNetUsers.Id)
            string legacyId;
            using (var scope = _app.Services.CreateScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var login = new ApplicationUser { UserName = "TR-BEE-2025-zz0006" };
                Assert.True((await users.CreateAsync(login, E2EApp.Password)).Succeeded);
                await users.SetLockoutEndDateAsync(login, DateTimeOffset.MaxValue);
                legacyId = login.Id;
            }
            await _app.WithContextAsync(async db =>
            {
                db.Beekeepers.Add(new Beekeeper { BeekeeperId = legacyId, BeekeeperFirstName = "Zz Legacy", BeekeeperLastName = "Zz Test", BeekeeperBirthDate = new DateOnly(1960, 1, 1), BeekeeperBusinessNumber = "TR-BEE-2025-zz0006", IsActive = true });
                return await db.SaveChangesAsync();
            });
            Assert.Equal(HttpStatusCode.OK, (await UserClient.SignInAsync(_app, "TR-BEE-2025-zz0006", E2EApp.Password)).Response.StatusCode);   // locked

            var found = await d.Officer.PostAsync("/Registration/Find", new() { ["number"] = "TR-BEE-2025-zz0006" }, "/Registration/Index");
            Assert.Equal($"/Registration/Reverify/{legacyId}", found.Headers.Location?.ToString());
            Assert.Contains("Zz Legacy", await d.Officer.HtmlAsync($"/Registration/Reverify/{legacyId}"));

            var form = await FormAsync("TR281234500007", "+905320000407", "x");
            foreach (var k in new[] { "ApiaryLatitude", "ApiaryLongitude", "ApiaryProvince", "ApiaryDistrictId", "ApiaryLocality", "ApiaryHives" })
                form.Remove(k);   // re-verification without a new apiary
            form["BeekeeperId"] = legacyId;
            form["FirstName"] = "Zz Legacy";
            Assert.Contains("id=\"registration-confirm\"", await BodyAsync(await d.Officer.PostAsync("/Registration/Submit", Op(form, "send"), $"/Registration/Reverify/{legacyId}")));
            var code = Regex.Match((await LastSmsAsync(legacyId)).BodyStored, @"\b\d{6}\b").Value;
            var confirmed = await d.Officer.PostAsync("/Registration/Submit", Op(new Dictionary<string, string>(form) { ["Code"] = code }, "confirm"), $"/Registration/Reverify/{legacyId}");
            Assert.Equal(HttpStatusCode.Redirect, confirmed.StatusCode);
            Assert.Equal(RegistrationKind.Reverification, await _app.WithContextAsync(db => db.BeekeeperRegistrations.Where(r => r.BeekeeperId == legacyId).Select(r => r.Kind).FirstAsync()));

            // still locked and not linked: accounts come with the portal (Features:BeekeeperAccounts, P3)
            Assert.False(await _app.WithContextAsync(db => db.BeekeeperAccounts.AnyAsync(a => a.BeekeeperId == legacyId)));
            Assert.Equal(HttpStatusCode.OK, (await UserClient.SignInAsync(_app, "TR-BEE-2025-zz0006", E2EApp.Password)).Response.StatusCode);
        }

        [Fact]
        public async Task An_administrator_creates_an_apiary_with_a_generated_title_and_legacy_titles_stay()
        {
            await _app.CreateUserAsync("zz_test_reg7_admin", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "zz_test_reg7_admin");
            var bkId = Guid.NewGuid().ToString();
            var legacyApiary = await _app.WithContextAsync(async db =>
            {
                db.Beekeepers.Add(new Beekeeper { BeekeeperId = bkId, BeekeeperFirstName = "Zz", BeekeeperLastName = "Apiary", BeekeeperBirthDate = new DateOnly(1980, 1, 1), BeekeeperBusinessNumber = "TR-BEE-2026-zz0007", IsActive = true });
                var a = new Apiary { ApiaryBeekeeperId = bkId, ApiaryTitle = "zz_test typed title", ApiaryBeehivesCount = 5, ApiaryLocationLatitude = 40.9m, ApiaryLocationLongitude = 38.3m, ApiaryQRCode = Guid.NewGuid() };
                db.Apiaries.Add(a);
                await db.SaveChangesAsync();
                return a.ApiaryId;
            });
            var district = await DistrictAsync("28");
            Dictionary<string, string> Apiary(string locality) => new()
            {
                ["ApiaryBeekeeperId"] = bkId, ["ApiaryBeehivesCount"] = "12", ["ApiaryLocationLatitude"] = "40.8", ["ApiaryLocationLongitude"] = "38.5",
                ["Province"] = "28", ["DistrictId"] = district.ToString(), ["Locality"] = locality
            };
            Assert.Equal(HttpStatusCode.Redirect, (await admin.PostAsync("/Apiaries/Create", Apiary("zz_test Çam Mevkii"), $"/Apiaries/Create?beekeeperId={bkId}")).StatusCode);
            Assert.Equal(HttpStatusCode.Redirect, (await admin.PostAsync("/Apiaries/Create", Apiary("zz_test Çam Mevkii"), $"/Apiaries/Create?beekeeperId={bkId}")).StatusCode);
            var titles = await _app.WithContextAsync(db => db.Apiaries.Where(a => a.ApiaryBeekeeperId == bkId && a.TitleSource == "Generated").OrderBy(a => a.ApiaryId).Select(a => a.ApiaryTitle).ToListAsync());
            Assert.Equal(2, titles.Count);
            Assert.EndsWith(" – zz_test Çam Mevkii", titles[0]);
            Assert.Equal(titles[0] + " 2", titles[1]);   // the same parts: a number is added
            Assert.All(titles, t => Assert.True(t.Length <= 50));

            // missing parts are refused
            var noPlace = await admin.PostAsync("/Apiaries/Create", new Dictionary<string, string>(Apiary("")) { ["Locality"] = "" }, $"/Apiaries/Create?beekeeperId={bkId}");
            Assert.Equal(HttpStatusCode.OK, noPlace.StatusCode);

            // a legacy apiary keeps its typed title on a save without the parts
            Assert.Contains("id=\"apiary-legacy-title\"", await admin.HtmlAsync($"/Apiaries/Edit/{legacyApiary}"));
            var saved = await admin.PostAsync($"/Apiaries/Edit/{legacyApiary}", new()
            {
                ["ApiaryId"] = legacyApiary.ToString(), ["ApiaryBeekeeperId"] = bkId, ["ApiaryBeehivesCount"] = "6", ["ApiaryLocationLatitude"] = "40.9", ["ApiaryLocationLongitude"] = "38.3"
            }, $"/Apiaries/Edit/{legacyApiary}");
            Assert.Equal(HttpStatusCode.Redirect, saved.StatusCode);
            Assert.Equal("zz_test typed title", await _app.WithContextAsync(db => db.Apiaries.Where(a => a.ApiaryId == legacyApiary).Select(a => a.ApiaryTitle).FirstAsync()));
        }
    }
}
