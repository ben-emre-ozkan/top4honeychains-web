using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-78 P5 (PLANNED-APP-CHANGES A1-A6, B1, D-84): role and capability names and the texts around them in five languages
    [Collection("E2E")]
    public class ReferenceTextsTests
    {
        private readonly E2EApp _app;

        public ReferenceTextsTests(E2EApp app) => _app = app;

        private static readonly Dictionary<string, string> QualityAssurance = new()
        {
            ["en"] = "Quality Assurance Officer", ["tr"] = "Kalite Güvence Sorumlusu", ["es"] = "Responsable de aseguramiento de la calidad",
            ["pl"] = "Specjalista ds. zapewnienia jakości", ["lv"] = "Kvalitātes nodrošināšanas speciālists"
        };
        private static readonly Dictionary<string, string> GroundRules = new()
        {
            ["en"] = "Ground rules", ["tr"] = "Temel kurallar", ["es"] = "Reglas básicas", ["pl"] = "Zasady podstawowe", ["lv"] = "Pamatnoteikumi"
        };
        private static readonly Dictionary<string, string> Beekeeping = new()
        {
            ["en"] = "Beekeeping enterprise", ["tr"] = "Arıcılık İşletmesi", ["es"] = "Explotación apícola", ["pl"] = "Pszczelarstwo", ["lv"] = "Biškopības saimniecība"
        };
        private static readonly Dictionary<string, string> Able = new()
        {
            ["en"] = "Able to", ["tr"] = "bilir", ["es"] = "Capaz de", ["pl"] = "Zdolność do", ["lv"] = "Spēj"
        };

        // migration 026 on the test database, twice (idempotent)
        private async Task ApplyMigrationAsync()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "db", "migrations")))
                dir = dir.Parent;
            var script = await File.ReadAllTextAsync(Path.Combine(dir!.FullName, "db", "migrations", "026_d78_reference_texts.sql"));
            await using var conn = new SqlConnection(_app.ConnectionString);
            await conn.OpenAsync();
            for (var run = 0; run < 2; run++)
                foreach (var batch in Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline).Select(b => b.Trim()).Where(b => b.Length > 0))
                    await new SqlCommand(batch, conn).ExecuteNonQueryAsync();
            _app.Services.GetRequiredService<RefText>().Invalidate();
        }

        [Fact]
        public async Task Roles_capabilities_and_rule_labels_have_their_names_in_five_languages()
        {
            await ApplyMigrationAsync();
            var ui = CultureInfo.CurrentUICulture;
            try
            {
                foreach (var c in ResourceTests.Languages)
                {
                    CultureInfo.CurrentUICulture = new CultureInfo(c);
                    using var scope = _app.Services.CreateScope();
                    var refText = scope.ServiceProvider.GetRequiredService<RefText>();
                    var translator = scope.ServiceProvider.GetRequiredService<MessageTranslator>();
                    var L = scope.ServiceProvider.GetRequiredService<IStringLocalizer<SharedResource>>();

                    // A1: the role name; the code is unchanged
                    Assert.Equal(QualityAssurance[c], refText.Role(OrgRoles.QualityApprover));
                    // A2: messages name the role, never its code
                    foreach (var message in new[]
                    {
                        $"Finalizing requested; a {OrgRoles.QualityApprover} (another person) must confirm it.",
                        $"Only a {OrgRoles.QualityApprover} of the organization can approve.",
                    })
                    {
                        var shown = translator.T(message);
                        Assert.Contains(QualityAssurance[c], shown);
                        Assert.DoesNotContain(OrgRoles.QualityApprover, shown);
                    }
                    foreach (var message in new[]
                    {
                        $"Report submitted; a {OrgRoles.LabManager} (another person) must approve it.",
                        $"The report is submitted; the {OrgRoles.LabManager} approves it or returns it for changes.",
                    })
                        Assert.DoesNotContain(OrgRoles.LabManager, translator.T(message));
                    // A4
                    Assert.StartsWith(GroundRules[c], L["Ground rules (cannot be switched off):"].Value);
                    // A5, A6, B1: abilities and names instead of codes
                    Assert.Equal(Beekeeping[c], refText.Capability(Capabilities.Beekeeping));
                    foreach (var code in new[] { Capabilities.CentralAssociation, Capabilities.PublicAuthority, Capabilities.PlatformAdmin, Capabilities.BeekeeperRegistration })
                        Assert.NotEqual(code, refText.Capability(code));
                    foreach (var code in new[] { Capabilities.Association, Capabilities.Processing, Capabilities.Packing, Capabilities.Laboratory, Capabilities.Wholesale,
                                                 Capabilities.Retail, Capabilities.CentralAssociation, Capabilities.PublicAuthority, Capabilities.Beekeeping })
                    {
                        var description = await _app.WithContextAsync(db => Task.FromResult(db.Capabilities.Where(x => x.Code == code).Select(x => x.Description).First()));
                        Assert.Contains(Able[c], refText.CapabilityDescription(code, description));
                    }
                }
            }
            finally
            {
                CultureInfo.CurrentUICulture = ui;
            }
        }

        [Fact]
        public async Task The_permissions_matrix_uses_the_languages_ccp_abbreviation()
        {
            // B2: the matrix rows are CCP codes; A4: the ground rules label
            await _app.CreateUserAsync("zz_test_b2_admin", "Administrator");
            var admin = await UserClient.LoginAsync(_app, "zz_test_b2_admin");
            foreach (var (culture, abbreviation) in new[] { ("lv", "KKP"), ("tr", "KKN"), ("es", "PCC"), ("pl", "KPK") })
            {
                var html = await admin.HtmlAsync($"/Permissions/Index?tab=roles&role={OrgRoles.Staff}&culture={culture}");
                Assert.Contains($"<td>{abbreviation} 0", html);
                Assert.DoesNotContain("<td>CCP 0", html);
                Assert.Contains(GroundRules[culture], System.Net.WebUtility.HtmlDecode(html));
            }
        }

        [Fact]
        public void English_texts_spell_organization_and_name_no_role_codes()
        {
            var values = XDocument.Load(Path.Combine(ResourceTests.WebRootPath, "Resources", "SharedResource.en.resx"))
                .Root!.Elements("data").Select(d => (Key: (string)d.Attribute("name")!, Value: (string)d.Element("value")!)).ToList();
            // A3
            Assert.Empty(values.Where(v => Regex.IsMatch(v.Value, @"\b[Oo]rganis(ation|ations|e|ed)\b|\b[Ff]inalis")).Select(v => v.Key));
            // A2: a role code in a message text (not in a placeholder of a template)
            var codes = new[] { OrgRoles.QualityApprover, OrgRoles.LabManager, OrgRoles.SampleAcceptanceOfficer, OrgRoles.OrgAdmin, OrgRoles.FieldInspector,
                                OrgRoles.RegistrationOfficer, OrgRoles.ComplianceOfficer };
            Assert.Empty(values.Where(v => codes.Any(code => Regex.IsMatch(v.Value, $@"\b{code}\b"))).Select(v => v.Key));
            Assert.Contains(values, v => v.Value == "Ground rules (cannot be switched off):");
        }
    }
}
