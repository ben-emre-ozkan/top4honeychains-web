using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // With the organization model the only global role is the platform administrator (organization roles are
    // memberships), and role codes inside server messages are shown in the user's language
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class UserRolesTests
    {
        private readonly E2EApp _app;

        public UserRolesTests(E2EApp app) => _app = app;

        [Fact]
        public async Task Create_user_offers_only_the_platform_administrator_role()
        {
            await _app.CreateUserAsync("e2e_um_admin", "Administrator");
            await _app.CreateUserAsync("e2e_um_retired", "SampleAcceptanceOfficer");   // the retired global role still exists
            var admin = await UserClient.LoginAsync(_app, "e2e_um_admin");

            var form = await admin.HtmlAsync("/UserManagement/Create");
            Assert.Contains("<option value=\"Administrator\">", form);
            Assert.DoesNotContain("value=\"SampleAcceptanceOfficer\"", form);
            Assert.Contains("id=\"role-note\"", form);
            Assert.Contains("/Organizations/Index", form);

            Dictionary<string, string> NewUser(string name, string role) => new()
            {
                ["UserName"] = name, ["Email"] = name + "@example.invalid", ["Password"] = E2EApp.Password, ["ConfirmPassword"] = E2EApp.Password, ["Role"] = role
            };

            // no global role: created, access only through memberships
            var created = await admin.PostAsync("/UserManagement/Create", NewUser("e2e_um_member", ""), "/UserManagement/Create");
            Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);
            Assert.Equal(0, await _app.WithContextAsync(async db =>
                await db.Database.SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM AspNetUserRoles ur JOIN AspNetUsers u ON u.Id = ur.UserId WHERE u.UserName = 'e2e_um_member'").FirstAsync()));

            // a role that is not offered is refused, also when posted directly
            var refused = await admin.PostAsync("/UserManagement/Create", NewUser("e2e_um_bad", "SampleAcceptanceOfficer"), "/UserManagement/Create");
            Assert.Equal(HttpStatusCode.OK, refused.StatusCode);
            Assert.Equal(0, await _app.WithContextAsync(async db =>
                await db.Database.SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM AspNetUsers WHERE UserName = 'e2e_um_bad'").FirstAsync()));
        }

        [Fact]
        public async Task Role_codes_in_messages_are_translated()
        {
            var names = new Dictionary<string, string> { ["Staff"] = "Personel-tr", ["SampleAcceptanceOfficer"] = "Numune-kabul-tr", ["OrgAdmin"] = "Kurum-yon-tr" };
            await _app.WithContextAsync(async db =>
            {
                foreach (var (code, name) in names)
                    if (!await db.ReferenceTranslations.AnyAsync(t => t.Entity == RefEntity.OrgRole && t.EntityKey == code && t.Culture == "tr"))
                        db.ReferenceTranslations.Add(new ReferenceTranslation { Entity = RefEntity.OrgRole, EntityKey = code, Field = "Name", Culture = "tr", Value = name, UpdatedAt = DateTime.UtcNow });
                return await db.SaveChangesAsync();
            });
            _app.Services.GetRequiredService<TOP4HoneyChains.Web.Services.RefText>().Invalidate();
            var expected = await _app.WithContextAsync(db => db.ReferenceTranslations
                .Where(t => t.Entity == RefEntity.OrgRole && t.Culture == "tr" && names.Keys.Contains(t.EntityKey)).ToDictionaryAsync(t => t.EntityKey, t => t.Value));

            var culture = System.Globalization.CultureInfo.CurrentUICulture;
            try
            {
                System.Globalization.CultureInfo.CurrentUICulture = new System.Globalization.CultureInfo("tr");
                using var scope = _app.Services.CreateScope();
                var translator = scope.ServiceProvider.GetRequiredService<TOP4HoneyChains.Web.Services.MessageTranslator>();

                // one role, and a list of roles ("A, B" and "A or B")
                var one = translator.T("Your role (Staff) cannot record deliveries; roles: SampleAcceptanceOfficer.");
                Assert.Contains(expected["Staff"], one);
                Assert.Contains(expected["SampleAcceptanceOfficer"], one);
                Assert.DoesNotContain("SampleAcceptanceOfficer", one);
                var list = translator.T("Your role (Staff) cannot record deliveries; roles: SampleAcceptanceOfficer, OrgAdmin.");
                Assert.Contains(expected["SampleAcceptanceOfficer"] + ", " + expected["OrgAdmin"], list);
                // a list with a part that is not reference data stays as it is
                Assert.Contains("SampleAcceptanceOfficer, nobody", translator.T("Your role (Staff) cannot record deliveries; roles: SampleAcceptanceOfficer, nobody."));
            }
            finally
            {
                System.Globalization.CultureInfo.CurrentUICulture = culture;
            }
        }
    }
}
