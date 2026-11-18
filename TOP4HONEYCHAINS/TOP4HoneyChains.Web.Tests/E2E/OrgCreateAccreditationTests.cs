using System.Net;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-88 follow-up: the administrator enters a laboratory's accreditation in the form that creates the organization
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class OrgCreateAccreditationTests
    {
        private readonly E2EApp _app;
        public OrgCreateAccreditationTests(E2EApp app) => _app = app;

        private static KeyValuePair<string, string> P(string key, string value) => new(key, value);

        [Fact]
        public async Task An_accredited_laboratory_is_created_with_its_accreditation_in_one_step()
        {
            await _app.CreateUserAsync("zz_test_acc_admin", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "zz_test_acc_admin");
            Assert.Contains("id=\"lab-accreditation\"", await admin.HtmlAsync("/Organizations/Create"));

            var created = await admin.PostPairsAsync("/Organizations/Create", new[]
            {
                P("DisplayName", "zz_test ACC lab"), P("capabilities", Capabilities.Laboratory), P("capabilities", Capabilities.Retail),
                P("isAccredited", "true"), P("accreditationNumber", "  zz-AB-0001-T "), P("accreditationValidUntil", "2030-01-31")
            }, "/Organizations/Create");
            Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);
            var caps = await _app.WithContextAsync(db => db.OrgCapabilities.Where(c => db.Organizations.Any(o => o.OrgId == c.OrgId && o.DisplayName == "zz_test ACC lab")).ToListAsync());
            var lab = Assert.Single(caps, c => c.Capability == Capabilities.Laboratory);
            Assert.True(lab.IsAccredited);
            Assert.Equal("zz-AB-0001-T", lab.AccreditationNumber);
            Assert.Equal(new DateOnly(2030, 1, 31), lab.AccreditationValidUntil);
            // the accreditation belongs to the Laboratory capability only
            var retail = Assert.Single(caps, c => c.Capability == Capabilities.Retail);
            Assert.False(retail.IsAccredited);
            Assert.Null(retail.AccreditationNumber);
        }

        [Fact]
        public async Task Accreditation_needs_the_laboratory_capability_and_a_number()
        {
            await _app.CreateUserAsync("zz_test_acc_admin2", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "zz_test_acc_admin2");

            var noNumber = await admin.PostPairsAsync("/Organizations/Create", new[]
            {
                P("DisplayName", "zz_test ACC no number"), P("capabilities", Capabilities.Laboratory), P("isAccredited", "true")
            }, "/Organizations/Create");
            Assert.Equal(HttpStatusCode.OK, noNumber.StatusCode);
            Assert.Contains("An accredited laboratory needs an accreditation number.", WebUtility.HtmlDecode(await noNumber.Content.ReadAsStringAsync()));

            var notLab = await admin.PostPairsAsync("/Organizations/Create", new[]
            {
                P("DisplayName", "zz_test ACC packer"), P("capabilities", Capabilities.Packing), P("isAccredited", "true"), P("accreditationNumber", "zz-AB-0002-T")
            }, "/Organizations/Create");
            Assert.Equal(HttpStatusCode.OK, notLab.StatusCode);
            var html = WebUtility.HtmlDecode(await notLab.Content.ReadAsStringAsync());
            Assert.Contains("Accreditation applies only to an organization with the Laboratory capability.", html);
            Assert.Contains("value=\"zz-AB-0002-T\"", html);   // what was typed stays in the form

            Assert.False(await _app.WithContextAsync(db => db.Organizations.AnyAsync(o => o.DisplayName == "zz_test ACC no number" || o.DisplayName == "zz_test ACC packer")));
        }
    }
}
