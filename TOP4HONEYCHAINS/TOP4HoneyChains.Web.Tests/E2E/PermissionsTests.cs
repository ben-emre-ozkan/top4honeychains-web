using System.Net;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-56: the platform administrator edits members, the role matrix and capabilities; the fixed safety rules hold
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class PermissionsTests
    {
        private readonly E2EApp _app;

        public PermissionsTests(E2EApp app) => _app = app;

        private static async Task<List<string>> RightsAsync(UserClient admin, string role)
        {
            var html = await admin.HtmlAsync($"/Permissions/Index?tab=roles&role={role}");
            return Regex.Matches(html, @"name=""allow"" value=""([^""]+)"" checked=""checked""").Select(m => m.Groups[1].Value).ToList();
        }

        private static Task<HttpResponseMessage> SaveAsync(UserClient admin, string role, IEnumerable<string> rights) =>
            admin.PostPairsAsync("/Permissions/SaveRole",
                rights.Select(r => new KeyValuePair<string, string>("allow", r)).Prepend(new("role", role)),
                $"/Permissions/Index?tab=roles&role={role}");

        [Fact]
        public async Task The_matrix_changes_rights_and_the_fixed_rules_hold()
        {
            await _app.CreateUserAsync("e2e_perm_admin", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "e2e_perm_admin");
            var org = await _app.CreateOrgAsync("PERM Processor", null, (Capabilities.Processing, null), (Capabilities.Packing, null));
            await _app.AddMemberAsync(org, await _app.CreateUserAsync("e2e_perm_sao"), OrgRoles.SampleAcceptanceOfficer);
            await _app.AddMemberAsync(org, await _app.CreateUserAsync("e2e_perm_staff"), OrgRoles.Staff);
            var sao = await UserClient.LoginAsync(_app, "e2e_perm_sao");
            var staff = await UserClient.LoginAsync(_app, "e2e_perm_staff");
            var saoRights = await RightsAsync(admin, OrgRoles.SampleAcceptanceOfficer);
            var staffRights = await RightsAsync(admin, OrgRoles.Staff);
            var managerRights = await RightsAsync(admin, OrgRoles.LabManager);
            Assert.Contains("Intake::Enter", saoRights);
            Assert.DoesNotContain("Intake::Enter", staffRights);

            try
            {
                // the default: the SampleAcceptanceOfficer records deliveries, Staff does not
                Assert.Equal(HttpStatusCode.OK, (await sao.GetAsync("/Intake/Create")).StatusCode);
                Assert.Equal(HttpStatusCode.Redirect, (await staff.GetAsync("/Intake/Create")).StatusCode);

                // moved to Staff
                Assert.Contains("saved", await admin.FlashAfterAsync(await SaveAsync(admin, OrgRoles.Staff, staffRights.Concat(new[] { "Intake::Enter", "Intake::Read" }))));
                Assert.Contains("saved", await admin.FlashAfterAsync(await SaveAsync(admin, OrgRoles.SampleAcceptanceOfficer, saoRights.Where(r => r != "Intake::Enter"))));
                Assert.Equal(HttpStatusCode.OK, (await staff.GetAsync("/Intake/Create")).StatusCode);
                Assert.Equal(HttpStatusCode.Redirect, (await sao.GetAsync("/Intake/Create")).StatusCode);

                // a right without any role is refused, and nothing changes
                Assert.Contains("Every right needs at least one role", await admin.FlashAfterAsync(
                    await SaveAsync(admin, OrgRoles.LabManager, managerRights.Where(r => r != "LabResults::Approve"))));
                Assert.Contains("LabResults::Approve", await RightsAsync(admin, OrgRoles.LabManager));

                // the changes are in the audit log
                Assert.True(await _app.WithContextAsync(db => db.AuditEntries.AnyAsync(a => a.Entity == "RolePermissions")));

                // a role that may enter and approve a CCP still cannot approve its own values (fixed rule)
                var ccp2 = await _app.WithContextAsync(db => db.CriticalControlPoints.Where(c => c.Code == "CCP 2").Select(c => c.Id).FirstAsync());
                Assert.Contains("saved", await admin.FlashAfterAsync(await SaveAsync(admin, OrgRoles.Staff, (await RightsAsync(admin, OrgRoles.Staff)).Append($"Ccp:{ccp2}:Approve"))));
                var lot = await _app.WithContextAsync(async db =>
                {
                    var w = new Wholesaler { WholesalerTitle = "PERM Wholesaler", WholesalerIsActive = true, CreatedAt = DateTime.Now };
                    db.Wholesalers.Add(w);
                    var bk = new Beekeeper { BeekeeperId = Guid.NewGuid().ToString(), BeekeeperFirstName = "PERM", BeekeeperLastName = "Beekeeper", BeekeeperBirthDate = new DateOnly(1980, 1, 1), BeekeeperBusinessNumber = "PERM-BEE-1", IsActive = true };
                    db.Beekeepers.Add(bk);
                    await db.SaveChangesAsync();
                    var ap = new Apiary { ApiaryBeekeeperId = bk.BeekeeperId, ApiaryTitle = "PERM Apiary", ApiaryBeehivesCount = 3, ApiaryQRCode = Guid.NewGuid() };
                    db.Apiaries.Add(ap);
                    await db.SaveChangesAsync();
                    var r = new RawHoneyProduction { RawHoneyProductionApiaryId = ap.ApiaryId, RawHoneyProductionAmount = 10, RawHoneyProductionDate = DateOnly.FromDateTime(DateTime.Today),
                        RawHoneyProductionHoneyType = await db.HoneyTypes.Select(h => h.HoneyTypeId).FirstAsync(), RawHoneyProductionPeriodId = await db.RawHoneyProductionPeriods.Select(p => p.RawHoneyProductionPeriodId).FirstAsync(),
                        RawHoneyProductionWholesalerId = w.WholesalerId, OwnerOrgId = org };
                    db.RawHoneyProductions.Add(r);
                    await db.SaveChangesAsync();
                    db.RawHoneyCCPAssignments.Add(new RawHoneyCCPAssignment { RawHoneyProductionId = r.RawHoneyProductionId, RawHoneyCCPAssignmentCCP = ccp2, RawHoneyCCPAssignmentStatus = AssignmentStatusIds.Assigned, RawHoneyCCPAssignmentStartDate = DateTime.Now });
                    await db.SaveChangesAsync();
                    return r.RawHoneyProductionId;
                });
                var parameters = await _app.WithContextAsync(db => db.CriticalControlPointParameters.AsNoTracking()
                    .Where(p => p.CCPId == ccp2 && p.DataType != CcpDataType.File).OrderBy(p => p.ParameterId).ToListAsync());
                var form = new Dictionary<string, string> { ["CCPId"] = ccp2.ToString(), ["RawHoneyProductionId"] = lot.ToString() };
                for (var i = 0; i < parameters.Count; i++)
                {
                    form[$"Groups[0].Parameters[{i}].ParameterId"] = parameters[i].ParameterId.ToString();
                    if (parameters[i].DataType == CcpDataType.YesNo) form[$"Groups[0].Parameters[{i}].MeasuredTextValue"] = "Yes";
                    else if (parameters[i].DataType == CcpDataType.Text) form[$"Groups[0].Parameters[{i}].MeasuredTextValue"] = "ok";
                    else form[$"Groups[0].Parameters[{i}].MeasuredNumericValue"] = "1";
                }
                var page = $"/CCPMonitoring/RawHoneyProductionManageCCP?rawHoneyProductionId={lot}&ccpId={ccp2}";
                Assert.Contains("values saved", await staff.PostFlashAsync("/CCPMonitoring/RawHoneyProductionManageCCP", form, page));
                Assert.Contains("Four-eyes", await staff.PostFlashAsync("/CCPMonitoring/Approve", new() { ["ownerKind"] = "raw", ["ownerId"] = lot.ToString(), ["ccpId"] = ccp2.ToString() }, page));
                Assert.Null(await _app.WithContextAsync(db => db.RawHoneyCCPAssignments.Where(a => a.RawHoneyProductionId == lot && a.RawHoneyCCPAssignmentCCP == ccp2).Select(a => a.ApprovedAt).FirstAsync()));

                // capabilities: a beekeeping enterprise that sells its own honey gets Packing and Retail, and its staff sees shipments
                var farm = await _app.CreateOrgAsync("PERM Farm", null, (Capabilities.Beekeeping, null));
                await _app.AddMemberAsync(farm, await _app.CreateUserAsync("e2e_perm_farm"), OrgRoles.Staff);
                var farmer = await UserClient.LoginAsync(_app, "e2e_perm_farm");
                Assert.DoesNotContain("/Shipments/Index", await farmer.HtmlAsync("/MyWork/Index"));
                Assert.Contains("saved", await admin.FlashAfterAsync(await admin.PostPairsAsync("/Permissions/SaveCapabilities",
                    new KeyValuePair<string, string>[] { new("orgId", farm.ToString()), new("capabilities", Capabilities.Beekeeping), new("capabilities", Capabilities.Packing), new("capabilities", Capabilities.Retail) },
                    "/Permissions/Index?tab=capabilities")));
                Assert.Equal(new[] { Capabilities.Beekeeping, Capabilities.Packing, Capabilities.Retail }.OrderBy(c => c),
                    (await _app.WithContextAsync(db => db.OrgCapabilities.Where(c => c.OrgId == farm).Select(c => c.Capability).ToListAsync())).OrderBy(c => c));
                Assert.Contains("/Shipments/Index", await farmer.HtmlAsync("/MyWork/Index"));

                // members: added and revoked in one place
                await _app.CreateUserAsync("e2e_perm_new");
                Assert.Contains("added as", await admin.PostFlashAsync("/Permissions/AddMember",
                    new() { ["userName"] = "e2e_perm_new", ["orgId"] = farm.ToString(), ["orgRole"] = OrgRoles.QualityApprover }, "/Permissions/Index?tab=members"));
                var membership = await _app.WithContextAsync(db => db.Memberships.Where(m => m.OrgId == farm && m.OrgRole == OrgRoles.QualityApprover).Select(m => m.MembershipId).FirstAsync());
                Assert.Contains("revoked", await admin.PostFlashAsync("/Permissions/RevokeMember", new() { ["membershipId"] = membership.ToString() }, "/Permissions/Index?tab=members"));
                Assert.Equal(MembershipStatus.Revoked, await _app.WithContextAsync(db => db.Memberships.Where(m => m.MembershipId == membership).Select(m => m.Status).FirstAsync()));

                // only the platform administrator opens the screen
                Assert.NotEqual(HttpStatusCode.OK, (await staff.GetAsync("/Permissions/Index")).StatusCode);
            }
            finally
            {
                // other tests use the default matrix
                await SaveAsync(admin, OrgRoles.Staff, staffRights);
                await SaveAsync(admin, OrgRoles.SampleAcceptanceOfficer, saoRights);
            }
        }

        // The capabilities page: every capability with its CCPs, read-only, for the platform administrator only
        [Fact]
        public async Task The_capabilities_page_lists_every_capability_for_administrators_only()
        {
            await _app.CreateUserAsync("e2e_cap_admin", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "e2e_cap_admin");
            var html = await admin.HtmlAsync("/Capabilities/Index");
            foreach (var code in typeof(Capabilities).GetFields().Select(f => (string)f.GetValue(null)!))
                Assert.Contains($"data-capability=\"{code}\"", html);
            var association = html[html.IndexOf("data-capability=\"Association\"")..html.IndexOf("data-capability=\"CentralAssociation\"")];
            Assert.Contains(">CCP 0<", association);
            Assert.DoesNotContain("<form", html[html.IndexOf("id=\"capabilities\"")..]);

            var org = await _app.CreateOrgAsync("CAP Processor", null, (Capabilities.Processing, null));
            await _app.AddMemberAsync(org, await _app.CreateUserAsync("e2e_cap_staff"), OrgRoles.OrgAdmin);
            var member = await UserClient.LoginAsync(_app, "e2e_cap_staff");
            Assert.NotEqual(HttpStatusCode.OK, (await member.GetAsync("/Capabilities/Index")).StatusCode);
            Assert.NotEqual(HttpStatusCode.OK, (await UserClient.Anonymous(_app).GetAsync("/Capabilities/Index")).StatusCode);
        }
    }
}
