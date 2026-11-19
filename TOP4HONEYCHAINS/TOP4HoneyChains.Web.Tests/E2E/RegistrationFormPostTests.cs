using System.Net;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-86 (walkthrough finding): a browser posts every field of the form, also the empty DraftKey of a new registration. The page
    // after "send" must carry the key that the code was bound to, otherwise the confirmation finds no code.
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class RegistrationFormPostTests
    {
        private readonly E2EApp _app;
        public RegistrationFormPostTests(E2EApp app) => _app = app;

        [Fact]
        public async Task A_new_registration_posted_like_a_browser_is_confirmed_by_the_code()
        {
            var org = await _app.CreateOrgAsync("zz_test regform İlçe Tarım Müdürlüğü", null, (Capabilities.BeekeeperRegistration, null));
            await _app.AddMemberAsync(org, await _app.CreateUserAsync("zz_test_regform_ro"), OrgRoles.RegistrationOfficer);
            var ro = await UserClient.LoginAsync(_app, "zz_test_regform_ro");
            var district = await _app.WithContextAsync(db => db.Districts.Where(d => d.ProvinceCode == "28").OrderBy(d => d.DistrictId).Select(d => d.DistrictId).FirstAsync());
            var form = new Dictionary<string, string>
            {
                ["DraftKey"] = "", ["Code"] = "", ["RegistryNumber"] = "TR281234500091", ["FirstName"] = "Zz Form", ["LastName"] = "Zz Post", ["BirthDate"] = "1975-03-02",
                ["Phone"] = "+905320000491", ["Language"] = "tr", ["ApiaryLatitude"] = "40.7", ["ApiaryLongitude"] = "38.4", ["ApiaryProvince"] = "28",
                ["ApiaryDistrictId"] = district.ToString(), ["ApiaryLocality"] = "Zz Yayla", ["ApiaryHives"] = "12", ["PublicName"] = "false",
                ["PortalNotifications"] = "false", ["Attest"] = "true", ["op"] = "send"
            };
            var html = WebUtility.HtmlDecode(await (await ro.PostAsync("/Registration/Submit", form, "/Registration/New")).Content.ReadAsStringAsync());
            var draft = Regex.Match(html, "name=\"DraftKey\" value=\"([0-9a-f]*)\"").Groups[1].Value;
            Assert.Equal(32, draft.Length);
            var sms = await _app.WithContextAsync(db => db.SmsMessages.Where(m => m.SubjectKey == draft).OrderByDescending(m => m.SmsId).FirstAsync());
            form["DraftKey"] = draft;
            form["Code"] = Regex.Match(sms.BodyStored, @"\b\d{6}\b").Value;
            form["op"] = "confirm";
            Assert.Equal("The beekeeper is registered and verified.", await ro.FlashAfterAsync(await ro.PostAsync("/Registration/Submit", form, "/Registration/New")));
        }

        // D-93: "Add apiary" blocks are saved with the registration; an empty block is ignored; the code covers the blocks
        [Fact]
        public async Task More_apiaries_are_saved_with_the_registration_and_covered_by_the_code()
        {
            var org = await _app.CreateOrgAsync("zz_test regform2 İlçe Tarım Müdürlüğü", null, (Capabilities.BeekeeperRegistration, null));
            await _app.AddMemberAsync(org, await _app.CreateUserAsync("zz_test_regform2_ro"), OrgRoles.RegistrationOfficer);
            var ro = await UserClient.LoginAsync(_app, "zz_test_regform2_ro");
            var district = await _app.WithContextAsync(db => db.Districts.Where(d => d.ProvinceCode == "28").OrderBy(d => d.DistrictId).Select(d => d.DistrictId).FirstAsync());
            var form = new Dictionary<string, string>
            {
                ["DraftKey"] = "", ["Code"] = "", ["RegistryNumber"] = "TR281234500092", ["FirstName"] = "Zz Many", ["LastName"] = "Zz Apiaries", ["BirthDate"] = "1975-03-02",
                ["Phone"] = "+905320000492", ["Language"] = "tr", ["ApiaryLatitude"] = "40.7", ["ApiaryLongitude"] = "38.4", ["ApiaryProvince"] = "28",
                ["ApiaryDistrictId"] = district.ToString(), ["ApiaryLocality"] = "Zz Bir", ["ApiaryHives"] = "12",
                ["MoreApiaries[0].Latitude"] = "40.71", ["MoreApiaries[0].Longitude"] = "38.41", ["MoreApiaries[0].Province"] = "28",
                ["MoreApiaries[0].DistrictId"] = district.ToString(), ["MoreApiaries[0].Locality"] = "Zz İki", ["MoreApiaries[0].Hives"] = "7",
                ["MoreApiaries[1].Latitude"] = "", ["MoreApiaries[1].Longitude"] = "", ["MoreApiaries[1].Province"] = "",
                ["MoreApiaries[1].DistrictId"] = "", ["MoreApiaries[1].Locality"] = "", ["MoreApiaries[1].Hives"] = "",
                ["MoreApiaries[2].Latitude"] = "40.72", ["MoreApiaries[2].Longitude"] = "38.42", ["MoreApiaries[2].Province"] = "28",
                ["MoreApiaries[2].DistrictId"] = district.ToString(), ["MoreApiaries[2].Locality"] = "Zz Üç", ["MoreApiaries[2].Hives"] = "30",
                ["PublicName"] = "false", ["PortalNotifications"] = "false", ["Attest"] = "true", ["op"] = "send"
            };
            var html = WebUtility.HtmlDecode(await (await ro.PostAsync("/Registration/Submit", form, "/Registration/New")).Content.ReadAsStringAsync());
            Assert.Contains("value=\"Zz Üç\"", html);
            var draft = Regex.Match(html, "name=\"DraftKey\" value=\"([0-9a-f]*)\"").Groups[1].Value;
            var sms = await _app.WithContextAsync(db => db.SmsMessages.Where(m => m.SubjectKey == draft).OrderByDescending(m => m.SmsId).FirstAsync());
            form["DraftKey"] = draft;
            form["Code"] = Regex.Match(sms.BodyStored, @"\b\d{6}\b").Value;
            form["op"] = "confirm";

            Assert.Equal("The beekeeper is registered and verified.", await ro.FlashAfterAsync(await ro.PostAsync("/Registration/Submit", form, "/Registration/New")));
            var hives = await _app.WithContextAsync(db => db.Apiaries.Where(a => a.ApiaryBeekeeper!.BeekeeperFirstName == "Zz Many").OrderBy(a => a.ApiaryBeehivesCount).Select(a => (int)a.ApiaryBeehivesCount).ToListAsync());
            Assert.Equal(new[] { 7, 12, 30 }, hives);
        }

        // D-93: the SMS code covers the extra apiary blocks, so changing one after sending voids the code
        [Fact]
        public void The_code_content_covers_the_extra_apiaries()
        {
            var c = new TOP4HoneyChains.Web.Services.OrgModel.RegistrationService.Checked("TR281234500093", "+905320000493", true, null, null);
            TOP4HoneyChains.Web.Services.OrgModel.RegistrationForm Form(short hives) => new()
            {
                DraftKey = "zz", MoreApiaries = { new() { Latitude = 40.7m, Longitude = 38.4m, Province = "28", DistrictId = 1, Locality = "Zz", Hives = hives } }
            };
            Assert.Equal(TOP4HoneyChains.Web.Services.OrgModel.RegistrationService.ContentHash(Form(7), c),
                TOP4HoneyChains.Web.Services.OrgModel.RegistrationService.ContentHash(Form(7), c));
            Assert.NotEqual(TOP4HoneyChains.Web.Services.OrgModel.RegistrationService.ContentHash(Form(7), c),
                TOP4HoneyChains.Web.Services.OrgModel.RegistrationService.ContentHash(Form(8), c));
        }
    }
}
