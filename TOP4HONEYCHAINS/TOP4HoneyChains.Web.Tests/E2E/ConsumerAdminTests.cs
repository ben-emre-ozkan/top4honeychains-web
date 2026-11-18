using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.Identity;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-87 part 6: the platform administrator sees the account kind on the user list (with a filter), locks, unlocks and deletes
    // (anonymises) a consumer account, all audited by pseudonym; the admin home counts consumer accounts; the retention dry run
    // counts the consumer rows.
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class ConsumerAdminTests
    {
        private readonly E2EApp _app;

        public ConsumerAdminTests(E2EApp app) => _app = app;

        private async Task<UserClient> AdminAsync(string name)
        {
            await _app.CreateUserAsync(name, "Administrator");
            return await UserClient.LoginAsync(_app, name);
        }

        [Fact]
        public async Task The_user_list_shows_the_kind_and_the_administrator_locks_unlocks_and_deletes_a_consumer() => await ConsumerAccountTests.WithConsumersAsync(_app, async () =>
        {
            var admin = await AdminAsync("zz_test_cadm_admin");
            await _app.CreateUserAsync("zz_test_cadm_staffer");
            await ConsumerAccountTests.SignUpAsync(_app, "zz_test_cadm_cons");
            var account = await _app.WithContextAsync(async db =>
            {
                using var scope = _app.Services.CreateScope();
                var user = await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByNameAsync("zz_test_cadm_cons");
                return await db.ConsumerAccounts.AsNoTracking().FirstAsync(c => c.UserId == user!.Id);
            });

            var consumers = await admin.HtmlAsync("/UserManagement/Index?kind=consumer");
            Assert.Contains("zz_test_cadm_cons", consumers);
            Assert.Contains(account.Pseudonym, consumers);
            Assert.DoesNotContain("zz_test_cadm_staffer", consumers);
            Assert.Contains("zz_test_cadm_staffer", await admin.HtmlAsync("/UserManagement/Index?kind=staff"));
            Assert.DoesNotContain("zz_test_cadm_cons", await admin.HtmlAsync("/UserManagement/Index?kind=staff"));

            await admin.PostAsync($"/UserManagement/LockConsumer/{account.UserId}", new(), "/UserManagement/Index");
            var (_, locked) = await UserClient.SignInAsync(_app, "zz_test_cadm_cons", E2EApp.Password);
            Assert.Equal(HttpStatusCode.OK, locked.StatusCode);
            await admin.PostAsync($"/UserManagement/Unlock/{account.UserId}", new(), "/UserManagement/Index");
            var (_, open) = await UserClient.SignInAsync(_app, "zz_test_cadm_cons", E2EApp.Password);
            Assert.Equal(HttpStatusCode.Redirect, open.StatusCode);

            // issued sign-in details and an administrator's password are refused for consumers
            await admin.PostAsync($"/UserManagement/IssueCredentials/{account.UserId}", new() { ["phone"] = "+905320007788" }, "/UserManagement/Index");
            Assert.Equal(0, await _app.WithContextAsync(db => db.UserCredentialIssues.CountAsync(i => i.UserId == account.UserId)));

            await admin.PostAsync("/UserManagement/Delete", new() { ["id"] = account.UserId }, "/UserManagement/Index");
            Assert.False(await _app.WithContextAsync(db => db.ConsumerAccounts.AnyAsync(c => c.UserId == account.UserId)));
            var actions = await _app.WithContextAsync(db => db.AuditEntries.AsNoTracking()
                .Where(a => a.Entity == "ConsumerAccounts" && a.EntityKey == account.Pseudonym).Select(a => a.Action).ToListAsync());
            Assert.Contains("Lock", actions);
            Assert.Contains("Unlock", actions);
            Assert.Contains("Delete", actions);
            return 0;
        });

        [Fact]
        public async Task The_admin_home_counts_consumer_accounts_only_while_switched_on_and_retention_counts_consumer_rows()
        {
            var admin = await AdminAsync("zz_test_cadm_home");
            Assert.DoesNotContain("kpi-consumers", await admin.HtmlAsync("/Dashboard/Index"));
            await ConsumerAccountTests.WithConsumersAsync(_app, async () =>
            {
                Assert.Contains("kpi-consumers", await admin.HtmlAsync("/Dashboard/Index"));
                var retention = await admin.HtmlAsync("/Retention");
                Assert.Contains("data-rule=\"consumers\"", retention);
                Assert.Contains("data-rule=\"consumersaved\"", retention);
                Assert.Contains("data-rule=\"consumerfeedback\"", retention);
                return 0;
            });
        }
    }
}
