using System.Net;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-91: a beekeeper could not sign in with the details from the registration SMS. The officer had pressed "send the sign-in
    // details" again on the registration page ten seconds later; that set a new temporary password, so the first SMS stopped working.
    // Also: the login page silently opened the signed-in officer's start page, and a login form left open while someone signed in
    // or out in another tab ended in an HTTP 400 page.
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class SignInClarityTests
    {
        private readonly E2EApp _app;
        public SignInClarityTests(E2EApp app) => _app = app;

        private async Task<T> WithAccountsAsync<T>(Func<Task<T>> work)
        {
            var config = _app.Services.GetRequiredService<IConfiguration>();
            config["Features:BeekeeperAccounts"] = "true";
            try { return await work(); }
            finally { config["Features:BeekeeperAccounts"] = "false"; }
        }

        private Task<string> TemporaryPasswordAsync(string userId) => _app.WithContextAsync(async db =>
            Regex.Match(await db.SmsMessages.Where(m => m.Purpose == SmsPurpose.Credentials && m.SubjectKey == userId)
                .OrderByDescending(m => m.SmsId).Select(m => m.BodyStored).FirstAsync(), @"temporary password (\S+)\. ").Groups[1].Value);

        private Task<int> CredentialSmsCountAsync(string userId) =>
            _app.WithContextAsync(db => db.SmsMessages.CountAsync(m => m.Purpose == SmsPurpose.Credentials && m.SubjectKey == userId));

        [Fact]
        public async Task Sign_in_details_sent_at_registration_are_not_replaced_by_a_second_click() => await WithAccountsAsync(async () =>
        {
            var org = await _app.CreateOrgAsync("zz_test si91 İlçe Tarım Müdürlüğü", null, (Capabilities.BeekeeperRegistration, null));
            await _app.WithContextAsync(async db => { (await db.Organizations.FirstAsync(o => o.OrgId == org)).Province = "Giresun"; return await db.SaveChangesAsync(); });
            await _app.AddMemberAsync(org, await _app.CreateUserAsync("zz_test_si91_ro"), OrgRoles.RegistrationOfficer);
            await _app.AddMemberAsync(org, await _app.CreateUserAsync("zz_test_si91_co"), OrgRoles.ComplianceOfficer);
            var officer = await UserClient.LoginAsync(_app, "zz_test_si91_ro");
            var district = await _app.WithContextAsync(db => db.Districts.Where(d => d.ProvinceCode == "28").OrderBy(d => d.DistrictId).Select(d => d.DistrictId).FirstAsync());
            var form = new Dictionary<string, string>
            {
                ["RegistryNumber"] = "TR281234509101", ["FirstName"] = "Zz Hatice", ["LastName"] = "Zz Test", ["BirthDate"] = "1975-09-03", ["Phone"] = "0532 000 91 01",
                ["Language"] = "en", ["ApiaryLatitude"] = "40.7", ["ApiaryLongitude"] = "38.4", ["ApiaryProvince"] = "28", ["ApiaryDistrictId"] = district.ToString(),
                ["ApiaryLocality"] = "Zz Yayla", ["ApiaryHives"] = "30", ["PublicName"] = "true", ["PortalNotifications"] = "false", ["Attest"] = "true", ["IssueSignIn"] = "true"
            };
            var sent = WebUtility.HtmlDecode(await (await officer.PostAsync("/Registration/Submit", new(form) { ["op"] = "send" }, "/Registration/New")).Content.ReadAsStringAsync());
            form["DraftKey"] = Regex.Match(sent, "name=\"DraftKey\" value=\"([0-9a-f]+)\"").Groups[1].Value;
            var code = Regex.Match(await _app.WithContextAsync(db => db.SmsMessages.Where(m => m.SubjectKey == form["DraftKey"]).OrderByDescending(m => m.SmsId).Select(m => m.BodyStored).FirstAsync()), @"\b\d{6}\b").Value;
            var confirmed = await officer.PostAsync("/Registration/Submit", new(form) { ["op"] = "confirm", ["Code"] = code }, "/Registration/New");
            Assert.Equal("The beekeeper is registered and verified; the sign-in details were sent by SMS.", await officer.FlashAfterAsync(confirmed));
            var details = confirmed.Headers.Location!.ToString();
            var regId = details.Split('/').Last();

            var (userId, userName) = await _app.WithContextAsync(async db =>
            {
                var beekeeper = await db.BeekeeperRegistrations.Where(r => r.RegistrationId == int.Parse(regId)).Select(r => r.BeekeeperId).FirstAsync();
                var uid = await db.BeekeeperAccounts.Where(a => a.BeekeeperId == beekeeper).Select(a => a.UserId).FirstAsync();
                return (uid, await db.Database.SqlQueryRaw<string>("SELECT UserName AS Value FROM AspNetUsers WHERE Id = {0}", uid).FirstAsync());
            });
            var first = await TemporaryPasswordAsync(userId);

            // the page says the details went out and asks for a tick before sending new ones
            var page = WebUtility.HtmlDecode(await officer.HtmlAsync(details));
            Assert.Contains("id=\"sign-in-sent\"", page);
            Assert.Contains("name=\"again\"", page);

            // the second click without the tick sends nothing, and the first SMS still works
            Assert.Contains("already sent", await officer.PostFlashAsync($"/Registration/IssueSignIn/{regId}", new(), details));
            Assert.Equal(1, await CredentialSmsCountAsync(userId));
            var (_, signIn) = await UserClient.SignInAsync(_app, userName, first);
            Assert.Equal(HttpStatusCode.Redirect, signIn.StatusCode);
            Assert.DoesNotContain("/Account/Login", signIn.Headers.Location?.ToString() ?? "");

            // with the tick, new details go out and the earlier password stops working
            Assert.Equal("The sign-in details were sent by SMS.", await officer.PostFlashAsync($"/Registration/IssueSignIn/{regId}", new() { ["again"] = "true" }, details));
            Assert.Equal(2, await CredentialSmsCountAsync(userId));
            var second = await TemporaryPasswordAsync(userId);
            Assert.NotEqual(first, second);
            Assert.Equal(HttpStatusCode.OK, (await UserClient.SignInAsync(_app, userName, first)).Response.StatusCode);   // the form again: wrong password
            Assert.Equal(HttpStatusCode.Redirect, (await UserClient.SignInAsync(_app, userName, second)).Response.StatusCode);
            return 0;
        });

        [Fact]
        public async Task The_login_page_names_who_is_signed_in_and_an_outdated_form_gets_a_fresh_one()
        {
            await _app.CreateUserAsync("zz_test_si91_admin", "Administrator");

            // someone is signed in in this browser: the login page says who and offers to continue or to sign out
            var browser = await UserClient.LoginAsync(_app, "zz_test_si91_admin");
            var login = await browser.GetAsync("/Account/Login");
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            var html = WebUtility.HtmlDecode(await login.Content.ReadAsStringAsync());
            Assert.Contains("id=\"signed-in-as\"", html);
            Assert.Contains("zz_test_si91_admin", html);
            Assert.Contains("href=\"/Account/Logout\"", html);
            Assert.DoesNotContain("id=\"login-form\"", html);

            // a form taken while nobody was signed in, sent after someone signed in in another tab: a fresh form and a plain message
            var tab = UserClient.Anonymous(_app);
            var oldToken = await tab.FormTokenAsync("/Account/Login");
            Assert.Equal(HttpStatusCode.Redirect, (await tab.PostAsync("/Account/Login", new() { ["UserName"] = "zz_test_si91_admin", ["Password"] = E2EApp.Password }, "/Account/Login")).StatusCode);
            var stale = await tab.PostWithTokenAsync("/Account/Login", new() { ["UserName"] = "zz_test_si91_admin", ["Password"] = E2EApp.Password }, oldToken);
            Assert.Equal(HttpStatusCode.Redirect, stale.StatusCode);
            Assert.StartsWith("/Account/Login", stale.Headers.Location!.ToString());
            Assert.Contains("out of date", WebUtility.HtmlDecode(await tab.HtmlAsync(stale.Headers.Location!.ToString())));

            // once signed out again, the browser is anonymous like the old form, which then works; a fresh form works too
            await tab.GetAsync("/Account/Logout");
            var again = await tab.PostWithTokenAsync("/Account/Login", new() { ["UserName"] = "zz_test_si91_admin", ["Password"] = E2EApp.Password }, oldToken);
            Assert.Equal(HttpStatusCode.Redirect, again.StatusCode);
            Assert.DoesNotContain("/Account/Login", again.Headers.Location?.ToString() ?? "");
            await tab.GetAsync("/Account/Logout");
            var fresh = await tab.PostAsync("/Account/Login", new() { ["UserName"] = "zz_test_si91_admin", ["Password"] = E2EApp.Password }, "/Account/Login");
            Assert.Equal(HttpStatusCode.Redirect, fresh.StatusCode);
            Assert.DoesNotContain("/Account/Login", fresh.Headers.Location?.ToString() ?? "");

            // a forged sign-in without any token still signs nobody in
            var forged = await UserClient.Anonymous(_app).RawPostAsync("/Account/Login");
            Assert.Equal(HttpStatusCode.Redirect, forged.StatusCode);
            Assert.StartsWith("/Account/Login", forged.Headers.Location!.ToString());
        }
    }
}
