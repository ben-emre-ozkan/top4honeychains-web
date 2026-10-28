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
    }
}
