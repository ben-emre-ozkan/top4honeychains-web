using System.Net;
using System.Text.RegularExpressions;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-88: with the organization model the platform administrator's menu has My work, the dashboard, Set-up and Oversight; the
    // old data-entry menus (beekeepers, raw honey production, wholesalers, final honey productions) are gone and what is still
    // needed is the read-only Records page
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class AdminMenuTests
    {
        private readonly E2EApp _app;

        public AdminMenuTests(E2EApp app) => _app = app;

        [Fact]
        public async Task The_administrator_menu_has_set_up_and_oversight_and_no_data_entry()
        {
            await _app.CreateUserAsync("zz_test_menu_admin", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "zz_test_menu_admin");
            var html = await admin.HtmlAsync("/Dashboard/Index");
            var menu = Regex.Match(html, "<ul class=\"navbar-nav\" id=\"admin-menu\">.*?</ul>\\s*</div>\\s*</nav>", RegexOptions.Singleline).Value;
            Assert.NotEmpty(menu);

            foreach (var url in new[] { "/MyWork/Index", "/Dashboard/Index", "/Organizations/Index", "/Capabilities/Index", "/Permissions/Index", "/CcpResponsibilities/Index",
                                        "/Registry/Index", "/ReferenceData/Index", "/ReferenceTranslations/Index", "/Records/Index", "/Audit/Index", "/UserManagement/Index",
                                        "/Notices/Index", "/Retention/Index", "/CCPMonitoring/Corrections" })
                Assert.Contains($"href=\"{url}\"", menu);
            Assert.Contains("id=\"topnav-setup\"", menu);
            Assert.Contains("id=\"topnav-oversight\"", menu);
            foreach (var legacy in new[] { "/Beekeepers/", "/Apiaries/", "/RawHoneyProductions/", "/Wholesalers/", "/FinalHoneyProductions/", "/Registration/",
                                           "/Shipments/", "/Samples/", "/Storefronts/", "/OrgWorkspace/", "/HoneyTypes/", "/CriticalControlPoints/" })
                Assert.DoesNotContain($"href=\"{legacy}", menu);
            // the dashboard offers set-up and oversight, not data entry
            Assert.Contains("id=\"qa-records\"", html);
            Assert.DoesNotContain("href=\"/Beekeepers/Create\"", html);
            Assert.DoesNotContain("href=\"/Wholesalers/Index\"", html);

            // the reference lists are one page away
            var reference = await admin.HtmlAsync("/ReferenceData/Index");
            foreach (var url in new[] { "/HoneyTypes/Index", "/RawHoneyProductionPeriods/Index", "/BeekeeperEducationLevels/Index", "/BeekeepingPurposeTypes/Index",
                                        "/BeekeepingTypes/Index", "/CriticalControlPoints/Index", "/CriticalControlPointParameterCategories/Index", "/CriticalControlPointParameters/Index" })
                Assert.Contains($"href=\"{url}\"", reference);
        }

        [Fact]
        public async Task Records_are_read_only_and_for_administrators_only()
        {
            var plant = await _app.CreateOrgAsync("zz_test Menu Plant", null, (Capabilities.Processing, null), (Capabilities.Packing, null));
            var batchId = await _app.WithContextAsync(async db =>
            {
                var w = new Wholesaler { WholesalerTitle = "zz_test Menu Plant", WholesalerIsActive = true, CreatedAt = DateTime.Now };
                db.Wholesalers.Add(w);
                await db.SaveChangesAsync();
                var b = new FinalHoneyProduction
                {
                    FinalHoneyProductionTitle = "zz_test menu batch", FinalHoneyProductionDate = DateTime.Today, FinalHoneyProductionTotalAmount = 10,
                    FinalHoneyProductionQR = Guid.NewGuid(), OwnerOrgId = plant, FinalHoneyProductionWholesalerId = w.WholesalerId
                };
                db.FinalHoneyProductions.Add(b);
                await db.SaveChangesAsync();
                return b.FinalHoneyProductionId;
            });
            await _app.CreateUserAsync("zz_test_records_admin", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "zz_test_records_admin");
            var batches = await admin.HtmlAsync($"/Records/Index?org={plant}");
            Assert.Contains($"data-batch-id=\"{batchId}\"", batches);
            Assert.Contains("zz_test Menu Plant", batches);
            var page = Regex.Match(batches, "id=\"records-page\".*", RegexOptions.Singleline).Value;
            Assert.DoesNotContain("method=\"post\"", page);
            Assert.Contains("id=\"records-lots\"", await admin.HtmlAsync("/Records/Index?tab=lots"));

            var member = await _app.CreateUserAsync("zz_test_records_member");
            await _app.AddMemberAsync(plant, member, OrgRoles.OrgAdmin);
            var client = await UserClient.LoginAsync(_app, "zz_test_records_member");
            Assert.NotEqual(HttpStatusCode.OK, (await client.GetAsync("/Records/Index")).StatusCode);
            Assert.NotEqual(HttpStatusCode.OK, (await client.GetAsync("/ReferenceData/Index")).StatusCode);
        }
    }
}
