using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-78 P1 (D78-SPEC 2.1, 2.5): where the registration officer and the Compliance Officer may be placed
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class D78RolesTests
    {
        private readonly E2EApp _app;
        public D78RolesTests(E2EApp app) => _app = app;

        [Fact]
        public async Task Registration_officer_only_in_an_organization_with_the_registration_capability()
        {
            await _app.CreateUserAsync("zz_test_d78_admin", "Administrator");
            await _app.CreateUserAsync("zz_test_d78_ro");
            var plant = await _app.CreateOrgAsync("zz_test d78 plant", null, (Capabilities.Processing, null));
            var directorate = await _app.CreateOrgAsync("zz_test d78 directorate", null, (Capabilities.BeekeeperRegistration, null));
            var admin = await UserClient.LoginAsync(_app, "zz_test_d78_admin");

            Dictionary<string, string> Form(string role) => new() { ["orgRole"] = role, ["existingUserName"] = "zz_test_d78_ro" };

            Assert.Equal("The beekeeper registration officer role needs an organization with the Beekeeper registration capability.",
                await admin.PostFlashAsync($"/Organizations/AddMember/{plant}", Form(OrgRoles.RegistrationOfficer), $"/Organizations/Details/{plant}"));
            Assert.Contains("added as", await admin.PostFlashAsync($"/Organizations/AddMember/{directorate}", Form(OrgRoles.RegistrationOfficer), $"/Organizations/Details/{directorate}"));

            // the same rule on Roles and permissions -> Members
            var orgB = await _app.CreateOrgAsync("zz_test d78 plant b", null, (Capabilities.Processing, null));
            Assert.Equal("The beekeeper registration officer role needs an organization with the Beekeeper registration capability.",
                await admin.PostFlashAsync("/Permissions/AddMember", new() { ["userName"] = "zz_test_d78_ro", ["orgId"] = orgB.ToString(), ["orgRole"] = OrgRoles.RegistrationOfficer }, "/Permissions/Index?tab=members"));

            // the capability cannot be removed while officers hold the role
            await _app.WithContextAsync(async db =>
            {
                db.OrgCapabilities.Add(new OrgCapability { OrgId = directorate, Capability = Capabilities.PublicAuthority });
                return await db.SaveChangesAsync();
            });
            Assert.Equal("Revoke the beekeeper registration officers of this organization first.",
                await admin.PostFlashAsync($"/Organizations/RemoveCapability/{directorate}", new() { ["capability"] = Capabilities.BeekeeperRegistration }, $"/Organizations/Details/{directorate}"));
        }

        [Fact]
        public async Task A_verified_organization_without_a_compliance_officer_is_flagged()
        {
            await _app.CreateUserAsync("zz_test_d78_admin2", "Administrator");
            var co = await _app.CreateUserAsync("zz_test_d78_co");
            var org = await _app.CreateOrgAsync("zz_test d78 lab", null, (Capabilities.Laboratory, "zz-acc-1"));
            var admin = await UserClient.LoginAsync(_app, "zz_test_d78_admin2");

            Assert.Contains("id=\"missing-compliance-officer\"", await admin.HtmlAsync($"/Organizations/Details/{org}"));
            await _app.AddMemberAsync(org, co, OrgRoles.ComplianceOfficer);
            Assert.DoesNotContain("id=\"missing-compliance-officer\"", await admin.HtmlAsync($"/Organizations/Details/{org}"));

            // the Compliance Officer reads the organization's intakes and CCP records by default (migration 018)
            var perms = await _app.WithContextAsync(db => db.RolePermissions.Where(p => p.OrgRole == OrgRoles.ComplianceOfficer).Select(p => p.Area + ":" + p.Action).ToListAsync());
            Assert.Contains("Evidence:Approve", perms);
            Assert.Contains("Intake:Read", perms);
            Assert.Contains("Ccp:Read", perms);
        }
    }
}
