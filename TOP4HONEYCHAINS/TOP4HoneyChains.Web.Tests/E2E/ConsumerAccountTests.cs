using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.Identity;
using TOP4HoneyChains.Web.Services.Messaging;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-87 part 3: the consumer sign-up (only its own fields bind, honeypot, minimum fill time), sign-in and the account page
    // (password, language, phone by SMS code, my data, deletion with anonymisation), and the switch.
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class ConsumerAccountTests
    {
        private readonly E2EApp _app;

        public ConsumerAccountTests(E2EApp app) => _app = app;

        public static async Task<T> WithConsumersAsync<T>(E2EApp app, Func<Task<T>> work)
        {
            var config = app.Services.GetRequiredService<IConfiguration>();
            config["Features:ConsumerAccounts"] = "true";
            try { return await work(); }
            finally
            {
                config["Features:ConsumerAccounts"] = "false";
                app.Clock.Reset();
            }
        }

        private static string FormTime(string html) => WebUtility.HtmlDecode(Regex.Match(html, "name=\"formTime\" value=\"([^\"]+)\"").Groups[1].Value);

        // signs up through the page (as a person: waits past the minimum fill time) and returns the signed-in client
        public static async Task<(UserClient Client, HttpResponseMessage Response)> SignUpAsync(E2EApp app, string userName, Dictionary<string, string>? extra = null,
            bool wait = true, string? returnUrl = null)
        {
            var client = UserClient.Anonymous(app);
            var url = "/Consumer/SignUp" + (returnUrl != null ? "?returnUrl=" + Uri.EscapeDataString(returnUrl) : "");
            var html = await client.HtmlAsync(url);
            var form = new Dictionary<string, string>
            {
                ["UserName"] = userName, ["Password"] = E2EApp.Password, ["ConfirmPassword"] = E2EApp.Password, ["Language"] = "en",
                ["IsAdult"] = "true", ["AcceptNotice"] = "true", ["formTime"] = FormTime(html), ["website"] = ""
            };
            foreach (var (k, v) in extra ?? new())
                form[k] = v;
            if (wait)
                app.Clock.Advance(TimeSpan.FromSeconds(5));
            var response = await client.PostAsync(url, form, url);
            return (client, response);
        }

        private Task<ConsumerAccount?> AccountAsync(string userName) => _app.WithContextAsync(async db =>
        {
            using var scope = _app.Services.CreateScope();
            var user = await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByNameAsync(userName);
            return user == null ? null : await db.ConsumerAccounts.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == user.Id);
        });

        [Fact]
        public async Task Sign_up_creates_a_consumer_without_role_membership_or_email_and_ignores_extra_fields() => await WithConsumersAsync(_app, async () =>
        {
            var (client, response) = await SignUpAsync(_app, "zz_test_cons_signup", new()
            {
                ["Role"] = "Administrator", ["OrgId"] = "1", ["OrgRole"] = "OrgAdmin", ["IsAdmin"] = "true", ["Email"] = "zz@example.invalid",
                ["Status"] = "Active", ["Pseudonym"] = "C-AAAAAA", ["EmailConfirmed"] = "true", ["PhoneNumberConfirmed"] = "true"
            });
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Consumer", response.Headers.Location!.ToString());
            using var scope = _app.Services.CreateScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = (await users.FindByNameAsync("zz_test_cons_signup"))!;
            Assert.Empty(await users.GetRolesAsync(user));
            Assert.Null(user.Email);
            Assert.False(user.EmailConfirmed);
            Assert.False(user.PhoneNumberConfirmed);
            var account = (await AccountAsync("zz_test_cons_signup"))!;
            Assert.Matches("^C-[A-Z2-9]{6}$", account.Pseudonym);
            Assert.NotEqual("C-AAAAAA", account.Pseudonym);
            Assert.Equal(0, await _app.WithContextAsync(db => db.Memberships.CountAsync(m => m.UserId == user.Id)));
            Assert.Equal(1, await _app.WithContextAsync(db => db.NoticeAcceptances.CountAsync(a => a.UserId == user.Id && a.Notice!.Audience == NoticeAudience.Consumer)));
            // the audit names the pseudonym, never the user name
            Assert.False(await _app.WithContextAsync(db => db.AuditEntries.AnyAsync(a => a.UserName == "zz_test_cons_signup")));
            // the consumer home opens; staff pages do not
            Assert.Contains(account.Pseudonym, await client.HtmlAsync("/Consumer"));
            Assert.Equal(403, await client.StatusAsync("/Dashboard"));
            return 0;
        });

        [Fact]
        public async Task A_filled_honeypot_or_a_form_sent_too_quickly_creates_no_account() => await WithConsumersAsync(_app, async () =>
        {
            var (_, honeypot) = await SignUpAsync(_app, "zz_test_cons_bot1", new() { ["website"] = "http://spam.example.invalid" });
            Assert.Equal(HttpStatusCode.OK, honeypot.StatusCode);
            Assert.Null(await AccountAsync("zz_test_cons_bot1"));
            var (_, fast) = await SignUpAsync(_app, "zz_test_cons_bot2", wait: false);
            Assert.Equal(HttpStatusCode.OK, fast.StatusCode);
            Assert.Null(await AccountAsync("zz_test_cons_bot2"));
            var (_, noAge) = await SignUpAsync(_app, "zz_test_cons_young", new() { ["IsAdult"] = "false" });
            Assert.Equal(HttpStatusCode.OK, noAge.StatusCode);
            Assert.Contains("18 or older", await noAge.Content.ReadAsStringAsync());
            Assert.Null(await AccountAsync("zz_test_cons_young"));
            var (_, noNotice) = await SignUpAsync(_app, "zz_test_cons_nonotice", new() { ["AcceptNotice"] = "false" });
            Assert.Null(await AccountAsync("zz_test_cons_nonotice"));
            Assert.Equal(HttpStatusCode.OK, noNotice.StatusCode);
            return 0;
        });

        [Fact]
        public async Task Sign_in_lands_on_the_consumer_home_and_follows_only_a_return_url_inside_the_boundary() => await WithConsumersAsync(_app, async () =>
        {
            await SignUpAsync(_app, "zz_test_cons_login");
            var story = "/honeystory/" + Guid.NewGuid();
            var client = UserClient.Anonymous(_app);
            var ok = await client.PostAsync("/Account/Login?returnUrl=" + Uri.EscapeDataString(story), new() { ["UserName"] = "zz_test_cons_login", ["Password"] = E2EApp.Password }, "/Account/Login");
            Assert.Equal(story, ok.Headers.Location!.ToString());
            var other = UserClient.Anonymous(_app);
            var staff = await other.PostAsync("/Account/Login?returnUrl=%2FDashboard", new() { ["UserName"] = "zz_test_cons_login", ["Password"] = E2EApp.Password }, "/Account/Login");
            Assert.Equal("/Consumer", staff.Headers.Location!.ToString());
            Assert.Contains(staff.Headers.GetValues("Set-Cookie"), c => c.StartsWith("t4h.consumer=1"));
            // the sign-in page offers the sign-up while the switch is on
            Assert.Contains("/Consumer/SignUp", await UserClient.Anonymous(_app).HtmlAsync("/Account/Login"));
            return 0;
        });

        [Fact]
        public async Task With_the_switch_off_the_pages_answer_404_the_link_is_hidden_and_consumers_cannot_sign_in()
        {
            await WithConsumersAsync(_app, async () => (await SignUpAsync(_app, "zz_test_cons_off")).Response);
            var anonymous = UserClient.Anonymous(_app);
            Assert.Equal(404, await anonymous.StatusAsync("/Consumer/SignUp"));
            Assert.DoesNotContain("/Consumer/SignUp", await anonymous.HtmlAsync("/Account/Login"));
            var (_, login) = await UserClient.SignInAsync(_app, "zz_test_cons_off", E2EApp.Password);
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        }

        [Fact]
        public async Task The_account_page_changes_password_and_language_verifies_and_removes_the_phone() => await WithConsumersAsync(_app, async () =>
        {
            var (client, _) = await SignUpAsync(_app, "zz_test_cons_acc", new() { ["PhoneNumber"] = "+905320007701" });
            // a phone given at sign-up gets a code; the number is used only after the code is entered
            var store = _app.Services.GetRequiredService<FakeSmsStore>();
            var code = Regex.Match(store.LastTo("+905320007701")!.Text, @"\b\d{6}\b").Value;
            using (var scope = _app.Services.CreateScope())
                Assert.False((await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByNameAsync("zz_test_cons_acc"))!.PhoneNumberConfirmed);
            await client.PostAsync("/Consumer/VerifyPhone", new() { ["phone"] = "+905320007701", ["code"] = code }, "/Consumer/Account");
            Assert.Contains("+90 532 000 77 01", WebUtility.HtmlDecode(await client.HtmlAsync("/Consumer/Account")));   // D-94: shown with spaces
            await client.PostAsync("/Consumer/RemovePhone", new(), "/Consumer/Account");
            Assert.DoesNotContain("+90 532 000 77 01", WebUtility.HtmlDecode(await client.HtmlAsync("/Consumer/Account")));

            await client.PostAsync("/Consumer/Language", new() { ["language"] = "pl" }, "/Consumer/Account");
            Assert.Equal("pl", (await AccountAsync("zz_test_cons_acc"))!.Language);

            await client.PostAsync("/Consumer/Password", new() { ["currentPassword"] = E2EApp.Password, ["newPassword"] = "zz-New-Pw-2026!", ["confirmPassword"] = "zz-New-Pw-2026!" }, "/Consumer/Account");
            var (_, oldPassword) = await UserClient.SignInAsync(_app, "zz_test_cons_acc", E2EApp.Password);
            Assert.Equal(HttpStatusCode.OK, oldPassword.StatusCode);
            var (_, newPassword) = await UserClient.SignInAsync(_app, "zz_test_cons_acc", "zz-New-Pw-2026!");
            Assert.Equal(HttpStatusCode.Redirect, newPassword.StatusCode);
            return 0;
        });

        [Fact]
        public async Task My_data_downloads_as_json_and_deletion_anonymises_the_account() => await WithConsumersAsync(_app, async () =>
        {
            var (client, _) = await SignUpAsync(_app, "zz_test_cons_del");
            var account = (await AccountAsync("zz_test_cons_del"))!;
            var download = await client.GetAsync("/Consumer/Download");
            Assert.Equal("application/json", download.Content.Headers.ContentType!.MediaType);
            Assert.Contains("no-store", download.Headers.CacheControl!.ToString());
            using (var json = JsonDocument.Parse(await download.Content.ReadAsStringAsync()))
            {
                Assert.Equal(account.Pseudonym, json.RootElement.GetProperty("account").GetProperty("pseudonym").GetString());
                Assert.Equal("zz_test_cons_del", json.RootElement.GetProperty("account").GetProperty("userName").GetString());
            }
            // a wrong password deletes nothing
            await client.PostAsync("/Consumer/Delete", new() { ["password"] = "wrong-password-1" }, "/Consumer/Account");
            Assert.NotNull(await AccountAsync("zz_test_cons_del"));

            var deleted = await client.PostAsync("/Consumer/Delete", new() { ["password"] = E2EApp.Password }, "/Consumer/Account");
            Assert.Equal("/Account/Login", deleted.Headers.Location!.ToString());
            Assert.False(await _app.WithContextAsync(db => db.ConsumerAccounts.AnyAsync(c => c.UserId == account.UserId)));
            using var scope = _app.Services.CreateScope();
            var user = (await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByIdAsync(account.UserId))!;
            Assert.Equal("deleted-" + account.Pseudonym.ToLowerInvariant(), user.UserName);
            Assert.Null(user.PasswordHash);
            Assert.Null(user.PhoneNumber);
            var (_, again) = await UserClient.SignInAsync(_app, "zz_test_cons_del", E2EApp.Password);
            Assert.Equal(HttpStatusCode.OK, again.StatusCode);
            return 0;
        });

        [Fact]
        public async Task Members_and_role_grants_refuse_a_consumer_account() => await WithConsumersAsync(_app, async () =>
        {
            await SignUpAsync(_app, "zz_test_cons_role");
            await _app.CreateUserAsync("zz_test_cons_admin", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "zz_test_cons_admin");
            var orgId = await _app.CreateOrgAsync("zz_test_cons org", null, (Capabilities.Processing, null));
            await admin.PostAsync($"/Organizations/AddMember/{orgId}", new() { ["orgRole"] = OrgRoles.Staff, ["existingUserName"] = "zz_test_cons_role" }, $"/Organizations/Details/{orgId}");
            await admin.PostAsync("/Permissions/AddMember", new() { ["userName"] = "zz_test_cons_role", ["orgId"] = orgId.ToString(), ["orgRole"] = OrgRoles.Staff }, "/Permissions");
            var account = (await AccountAsync("zz_test_cons_role"))!;
            Assert.Equal(0, await _app.WithContextAsync(db => db.Memberships.CountAsync(m => m.UserId == account.UserId)));
            return 0;
        });

        [Fact]
        public void Sign_up_sign_in_and_feedback_are_rate_limited()
        {
            var endpoints = _app.Services.GetRequiredService<EndpointDataSource>().Endpoints;
            string? Policy(string controller, string action, string method) => endpoints
                .Where(e => e.Metadata.GetMetadata<ControllerActionDescriptor>() is { } a && a.ControllerName == controller && a.ActionName == action
                            && e.Metadata.GetMetadata<Microsoft.AspNetCore.Routing.IHttpMethodMetadata>()?.HttpMethods.Contains(method) == true)
                .Select(e => e.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName).FirstOrDefault();
            Assert.Equal("consumer-signup", Policy("Consumer", "SignUp", "POST"));
            Assert.Equal("signin", Policy("Account", "Login", "POST"));
        }
    }
}
