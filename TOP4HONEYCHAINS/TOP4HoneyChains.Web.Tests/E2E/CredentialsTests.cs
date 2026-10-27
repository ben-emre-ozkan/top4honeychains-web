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
    // D-78 P1 (D78-SPEC 4.3, 4.10): sign-in details issued by the person who registers the user and sent by SMS, the forced
    // change at the first sign-in, the 72-hour validity, re-issue, and the password reset by SMS code
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class CredentialsTests
    {
        private readonly E2EApp _app;
        public CredentialsTests(E2EApp app) => _app = app;

        private const string NewPassword = "Bal-Kovan-2026-zz";

        private async Task<UserClient> AdminAsync(string name)
        {
            await _app.CreateUserAsync(name, "Administrator");
            return await UserClient.LoginAsync(_app, name);
        }

        // the temporary password as the fake provider delivered it (English: the admin works in English)
        private Task<string> TemporaryPasswordOfAsync(string userName) =>
            _app.WithContextAsync(async db =>
            {
                var userId = await db.Database.SqlQueryRaw<string>("SELECT Id AS Value FROM AspNetUsers WHERE UserName = {0}", userName).FirstAsync();
                var body = await db.SmsMessages.Where(m => m.Purpose == SmsPurpose.Credentials && m.SubjectKey == userId)
                    .OrderByDescending(m => m.SmsId).Select(m => m.BodyStored).FirstAsync();
                return Regex.Match(body, @"temporary password (\S+)\. ").Groups[1].Value;
            });

        private async Task<ApplicationUser> UserAsync(string userName)
        {
            using var scope = _app.Services.CreateScope();
            return (await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByNameAsync(userName))!;
        }

        private Task<string> CodeSentToAsync(string userId, string purpose) =>
            _app.WithContextAsync(async db => Regex.Match(await db.SmsMessages.Where(m => m.SubjectKey == userId && m.Purpose == purpose)
                .OrderByDescending(m => m.SmsId).Select(m => m.BodyStored).FirstAsync(), @"\b\d{6}\b").Value);

        [Fact]
        public async Task An_administrator_issues_sign_in_details_by_sms_and_the_first_sign_in_forces_a_new_password()
        {
            var admin = await AdminAsync("zz_test_cred_admin");
            var created = await admin.PostAsync("/UserManagement/Create", new()
            {
                // a platform administrator, because an account without a role or membership cannot sign in (D-01)
                ["UserName"] = "zz_test_cred_user", ["PhoneNumber"] = "0532 000 03 01", ["Delivery"] = "Sms", ["Role"] = "Administrator"
            }, "/UserManagement/Create");
            Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);
            Assert.Equal("Sign-in details for zz_test_cred_user were sent by SMS.", await admin.FlashAfterAsync(created));

            var temporary = await TemporaryPasswordOfAsync("zz_test_cred_user");
            Assert.Equal(10, temporary.Length);
            Assert.DoesNotMatch("[0O1lI]", temporary);
            var sms = await _app.WithContextAsync(db => db.SmsMessages.Where(m => m.Purpose == SmsPurpose.Credentials).OrderByDescending(m => m.SmsId).FirstAsync());
            Assert.Equal("+90******0301", sms.ToPhoneMasked);

            // sign-in with the temporary password: everything leads to the password page first
            var (user, login) = await UserClient.SignInAsync(_app, "zz_test_cred_user", temporary);
            Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
            var blocked = await user.GetAsync("/Notices/Index");
            Assert.Equal("/Account/FirstPassword", blocked.Headers.Location?.ToString());
            Assert.Equal("/Account/FirstPassword", (await user.GetAsync("/Account/Profile")).Headers.Location?.ToString());

            // policy: 10 characters, not the user name, not common
            async Task<string> TryAsync(string pw)
            {
                var r = await user.PostAsync("/Account/FirstPassword", new() { ["password"] = pw, ["confirmPassword"] = pw }, "/Account/FirstPassword");
                return r.StatusCode == HttpStatusCode.OK ? await r.Content.ReadAsStringAsync() : "redirect:" + r.Headers.Location;
            }
            Assert.Contains("id=\"first-password-errors\"", await TryAsync("short1"));
            Assert.Contains("too common", await TryAsync("1234567890"));
            Assert.Contains("user name", await TryAsync("xx-zz_test_cred_user-xx"));
            Assert.Equal("redirect:/MyWork/Index", await TryAsync(NewPassword));

            var u = await UserAsync("zz_test_cred_user");
            Assert.Equal("+905320000301", u.PhoneNumber);
            Assert.True(u.PhoneNumberConfirmed);   // the temporary password arrived on that phone
            Assert.NotNull(await _app.WithContextAsync(db => db.UserCredentialIssues.Where(i => i.UserId == u.Id).Select(i => i.PasswordChangedAt).FirstAsync()));
            Assert.Equal(HttpStatusCode.Redirect, (await UserClient.SignInAsync(_app, "zz_test_cred_user", NewPassword)).Response.StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await UserClient.SignInAsync(_app, "zz_test_cred_user", temporary)).Response.StatusCode);   // old one refused
        }

        [Fact]
        public async Task A_temporary_password_expires_after_72_hours_and_a_reissue_replaces_it()
        {
            var admin = await AdminAsync("zz_test_cred_admin2");
            await admin.PostAsync("/UserManagement/Create", new() { ["UserName"] = "zz_test_cred_exp", ["PhoneNumber"] = "+905320000302", ["Delivery"] = "Sms", ["Role"] = "Administrator" }, "/UserManagement/Create");
            var first = await TemporaryPasswordOfAsync("zz_test_cred_exp");
            try
            {
                _app.Clock.Advance(TimeSpan.FromHours(72) + TimeSpan.FromMinutes(1));
                var (_, expired) = await UserClient.SignInAsync(_app, "zz_test_cred_exp", first);
                Assert.Equal(HttpStatusCode.OK, expired.StatusCode);
                Assert.Contains("The temporary password has expired", await expired.Content.ReadAsStringAsync());
            }
            finally
            {
                _app.Clock.Reset();
            }

            var u = await UserAsync("zz_test_cred_exp");
            Assert.Equal("Sign-in details for zz_test_cred_exp were sent by SMS.", await admin.PostFlashAsync($"/UserManagement/IssueCredentials/{u.Id}", new() { ["phone"] = "" }, "/UserManagement/Index"));
            var second = await TemporaryPasswordOfAsync("zz_test_cred_exp");
            Assert.NotEqual(first, second);
            Assert.Equal(HttpStatusCode.OK, (await UserClient.SignInAsync(_app, "zz_test_cred_exp", first)).Response.StatusCode);
            var (client, ok) = await UserClient.SignInAsync(_app, "zz_test_cred_exp", second);
            Assert.Equal(HttpStatusCode.Redirect, ok.StatusCode);
            Assert.Equal("/Account/FirstPassword", (await client.GetAsync("/MyWork/Index")).Headers.Location?.ToString());
        }

        [Fact]
        public async Task An_initial_password_set_by_an_administrator_must_also_be_changed()
        {
            var admin = await AdminAsync("zz_test_cred_admin3");
            var created = await admin.PostAsync("/UserManagement/Create", new()
            {
                ["UserName"] = "zz_test_cred_manual", ["Delivery"] = "Manual", ["Password"] = "Ilk-Sifre-2026-zz", ["ConfirmPassword"] = "Ilk-Sifre-2026-zz", ["Role"] = "Administrator"
            }, "/UserManagement/Create");
            Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);
            var (client, _) = await UserClient.SignInAsync(_app, "zz_test_cred_manual", "Ilk-Sifre-2026-zz");
            Assert.Equal("/Account/FirstPassword", (await client.GetAsync("/Account/Profile")).Headers.Location?.ToString());

            // the SMS path needs a phone number
            var refused = await admin.PostAsync("/UserManagement/Create", new() { ["UserName"] = "zz_test_cred_nophone", ["Delivery"] = "Sms", ["Role"] = "" }, "/UserManagement/Create");
            Assert.Equal(HttpStatusCode.OK, refused.StatusCode);
            Assert.Null(await _app.WithContextAsync(db => db.Database.SqlQueryRaw<string>("SELECT Id AS Value FROM AspNetUsers WHERE UserName = 'zz_test_cred_nophone'").FirstOrDefaultAsync()));

            // a password set by the administrator on the user list is changed at the next sign-in too
            var u = await UserAsync("zz_test_cred_manual");
            await client.PostAsync("/Account/FirstPassword", new() { ["password"] = NewPassword, ["confirmPassword"] = NewPassword }, "/Account/FirstPassword");
            await admin.PostAsync("/UserManagement/ChangePassword", new() { ["UserId"] = u.Id, ["NewPassword"] = "Yeni-Sifre-2026-zz", ["ConfirmPassword"] = "Yeni-Sifre-2026-zz" }, "/UserManagement/Index");
            var (again, _) = await UserClient.SignInAsync(_app, "zz_test_cred_manual", "Yeni-Sifre-2026-zz");
            Assert.Equal("/Account/FirstPassword", (await again.GetAsync("/MyWork/Index")).Headers.Location?.ToString());
        }

        [Fact]
        public async Task A_new_member_gets_sign_in_details_by_sms_from_the_organization_page()
        {
            var admin = await AdminAsync("zz_test_cred_admin4");
            var org = await _app.CreateOrgAsync("zz_test cred org", null, (Capabilities.Processing, null));
            Assert.Contains("added as", await admin.PostFlashAsync($"/Organizations/AddMember/{org}", new()
            {
                ["orgRole"] = OrgRoles.Staff, ["newUserName"] = "zz_test_cred_member", ["newPhone"] = "+905320000304"
            }, $"/Organizations/Details/{org}"));
            Assert.Equal(10, (await TemporaryPasswordOfAsync("zz_test_cred_member")).Length);
            Assert.Equal("Enter an existing user name, or a new user name and a mobile phone (or an initial password).", await admin.PostFlashAsync($"/Organizations/AddMember/{org}", new()
            {
                ["orgRole"] = OrgRoles.Staff, ["newUserName"] = "zz_test_cred_member2"
            }, $"/Organizations/Details/{org}"));
        }

        [Fact]
        public async Task The_password_is_reset_with_an_sms_code_to_a_verified_phone()
        {
            var userId = await _app.CreateUserAsync("zz_test_cred_reset", "Administrator");
            using (var scope = _app.Services.CreateScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var u = (await users.FindByIdAsync(userId))!;
                u.PhoneNumber = "+905320000305";
                u.PhoneNumberConfirmed = true;
                await users.UpdateAsync(u);
            }
            var anonymous = UserClient.Anonymous(_app);
            Assert.Contains("id=\"reset-send-code\"", await anonymous.HtmlAsync("/Account/ForgotPassword"));

            // the same answer for an unknown account, and no SMS
            var before = await _app.WithContextAsync(db => db.SmsMessages.CountAsync());
            Assert.Equal("If the account exists and has a verified mobile phone, a code was sent by SMS.",
                await anonymous.PostFlashAsync("/Account/SendResetCode", new() { ["userName"] = "zz_test_nobody" }, "/Account/ForgotPassword"));
            Assert.Equal(before, await _app.WithContextAsync(db => db.SmsMessages.CountAsync()));

            Assert.Equal("If the account exists and has a verified mobile phone, a code was sent by SMS.",
                await anonymous.PostFlashAsync("/Account/SendResetCode", new() { ["userName"] = "zz_test_cred_reset" }, "/Account/ForgotPassword"));
            var code = await CodeSentToAsync(userId, SmsPurpose.PasswordReset);
            Assert.Matches(@"^\d{6}$", code);

            var wrong = await anonymous.PostAsync("/Account/ResetWithCode", new()
            {
                ["userName"] = "zz_test_cred_reset", ["code"] = code == "000000" ? "111111" : "000000", ["password"] = NewPassword, ["confirmPassword"] = NewPassword
            }, "/Account/ResetWithCode?userName=zz_test_cred_reset");
            Assert.Contains("Wrong code. 2 tries left.", await wrong.Content.ReadAsStringAsync());

            var ok = await anonymous.PostAsync("/Account/ResetWithCode", new()
            {
                ["userName"] = "zz_test_cred_reset", ["code"] = code, ["password"] = NewPassword, ["confirmPassword"] = NewPassword
            }, "/Account/ResetWithCode?userName=zz_test_cred_reset");
            Assert.Equal("/Account/Login", ok.Headers.Location?.ToString());
            Assert.Equal(HttpStatusCode.Redirect, (await UserClient.SignInAsync(_app, "zz_test_cred_reset", NewPassword)).Response.StatusCode);

            // an account without a verified phone gets no code
            var plain = await _app.CreateUserAsync("zz_test_cred_reset2");
            await anonymous.PostAsync("/Account/SendResetCode", new() { ["userName"] = "zz_test_cred_reset2" }, "/Account/ForgotPassword");
            Assert.False(await _app.WithContextAsync(db => db.SmsMessages.AnyAsync(m => m.SubjectKey == plain)));
        }

        [Fact]
        public async Task A_phone_number_is_verified_with_a_code_on_the_profile()
        {
            var userId = await _app.CreateUserAsync("zz_test_cred_phone", "Administrator");
            var user = await UserClient.LoginAsync(_app, "zz_test_cred_phone");
            Assert.Equal("A code was sent by SMS. Enter it below.", await user.PostFlashAsync("/Account/UpdatePhone", new() { ["PhoneNumber"] = "0532 000 03 06" }, "/Account/Profile"));
            Assert.Contains("id=\"phone-verify\"", await user.HtmlAsync("/Account/Profile"));
            var code = await CodeSentToAsync(userId, SmsPurpose.PhoneVerify);
            Assert.Equal("Wrong code. 2 tries left.", await user.PostFlashAsync("/Account/VerifyPhone", new() { ["phone"] = "+905320000306", ["code"] = code == "999999" ? "999998" : "999999" }, "/Account/Profile"));
            Assert.Equal("Phone number verified.", await user.PostFlashAsync("/Account/VerifyPhone", new() { ["phone"] = "+905320000306", ["code"] = code }, "/Account/Profile"));
            var u = await UserAsync("zz_test_cred_phone");
            Assert.Equal("+905320000306", u.PhoneNumber);
            Assert.True(u.PhoneNumberConfirmed);
            Assert.NotNull(await _app.WithContextAsync(db => db.UserSettings.Where(s => s.UserId == userId).Select(s => s.PhoneVerifiedAt).FirstAsync()));
        }

        [Fact]
        public async Task The_beekeeper_area_is_closed_while_beekeeper_accounts_are_off()
        {
            await _app.CreateUserAsync("zz_test_cred_notbk", "Administrator");
            var user = await UserClient.LoginAsync(_app, "zz_test_cred_notbk");
            Assert.Equal(404, await user.StatusAsync("/Me/Index"));
        }
    }
}
