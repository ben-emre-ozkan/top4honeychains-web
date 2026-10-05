using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using TOP4HoneyChains.Web.Models.Entities;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-51: every text the UI can show has a translation in every language the site offers
    public class ResourceTests
    {
        public static readonly string[] Languages = { "en", "tr", "es", "pl", "lv" };

        private static readonly Lazy<string> WebRoot = new(() =>
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "TOP4HoneyChains.Web", "Resources")))
                dir = dir.Parent;
            Assert.NotNull(dir);
            return Path.Combine(dir!.FullName, "TOP4HoneyChains.Web");
        });

        private static readonly Lazy<Dictionary<string, Dictionary<string, string>>> Resources = new(() =>
            Languages.ToDictionary(l => l, l => XDocument.Load(Path.Combine(WebRoot.Value, "Resources", $"SharedResource.{l}.resx"))
                .Root!.Elements("data")
                .Where(d => d.Attribute("type") == null)
                .ToDictionary(d => d.Attribute("name")!.Value, d => d.Element("value")?.Value ?? "")));

        private static readonly Regex Placeholder = new(@"\{\d+\}|_[A-Z]+_|%[sd]");

        private static IEnumerable<string> Tokens(string s) => Placeholder.Matches(s).Select(m => m.Value).OrderBy(x => x);

        [Fact]
        public void Every_key_exists_in_every_language_with_a_value()
        {
            var all = Resources.Value.Values.SelectMany(d => d.Keys).Where(k => !k.EndsWith("1") || !new[] { "Name1", "Color1", "Bitmap1", "Icon1" }.Contains(k)).ToHashSet();
            var problems = new List<string>();
            foreach (var lang in Languages)
            {
                var d = Resources.Value[lang];
                problems.AddRange(all.Where(k => !d.ContainsKey(k)).Select(k => $"{lang}: missing \"{k}\""));
                problems.AddRange(d.Where(p => all.Contains(p.Key) && string.IsNullOrWhiteSpace(p.Value)).Select(p => $"{lang}: empty \"{p.Key}\""));
            }
            Assert.True(problems.Count == 0, string.Join("\n", problems.Take(50)) + (problems.Count > 50 ? $"\n… {problems.Count} in all" : ""));
        }

        [Fact]
        public void Translations_keep_the_placeholders_of_the_english_text()
        {
            var en = Resources.Value["en"];
            var problems = new List<string>();
            foreach (var lang in Languages.Where(l => l != "en"))
                foreach (var (key, value) in Resources.Value[lang])
                    if (en.TryGetValue(key, out var source) && !Tokens(source).SequenceEqual(Tokens(value)))
                        problems.Add($"{lang}: \"{key}\" -> \"{value}\"");
            Assert.True(problems.Count == 0, string.Join("\n", problems.Take(50)));
        }

        // Compiled resources drop one of two keys that differ only by letter case
        [Fact]
        public void No_two_keys_differ_only_by_case()
        {
            var dups = Resources.Value["en"].Keys.GroupBy(k => k, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).Select(g => string.Join(" / ", g)).ToList();
            Assert.True(dups.Count == 0, string.Join("\n", dups));
        }

        // Literal keys used in views and code: L["…"], _localizer["…"], GetRequiredService<IStringLocalizer<…>>()["…"]
        private static readonly Regex KeyUse = new(@"(?:(?<![\w.])(?:L|_localizer|Localizer)|SharedResource>>\(\))\[""((?:[^""\\]|\\.)*)""(?:\s*,|\])");

        [Fact]
        public void Every_key_used_in_views_and_code_exists()
        {
            var en = Resources.Value["en"];
            var missing = new List<string>();
            foreach (var file in Directory.EnumerateFiles(WebRoot.Value, "*.*", SearchOption.AllDirectories)
                         .Where(f => (f.EndsWith(".cshtml") || f.EndsWith(".cs")) && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")))
                foreach (Match m in KeyUse.Matches(File.ReadAllText(file)))
                {
                    var key = Regex.Unescape(m.Groups[1].Value);
                    if (!en.ContainsKey(key))
                        missing.Add($"{Path.GetFileName(file)}: \"{key}\"");
                }
            Assert.True(missing.Count == 0, string.Join("\n", missing.Distinct().Take(50)));
        }

        // Field names generated from property names (forms and validation messages) have translations
        [Fact]
        public void Generated_field_names_of_form_fields_exist()
        {
            var en = Resources.Value["en"];
            var missing = new List<string>();
            foreach (var file in Directory.EnumerateFiles(Path.Combine(WebRoot.Value, "Views"), "*.cshtml", SearchOption.AllDirectories))
                foreach (Match m in Regex.Matches(File.ReadAllText(file), @"asp-(?:for|validation-for)=""([^""]+)"""))
                {
                    var property = Regex.Split(m.Groups[1].Value, @"[.\]]").Last();
                    if (!Regex.IsMatch(property, @"^[A-Za-z]\w*$"))
                        continue;
                    var key = TOP4HoneyChains.Web.Services.LocalizedMetadataProvider.Humanize(property);
                    if (!en.ContainsKey(key))
                        missing.Add($"{Path.GetFileName(file)}: \"{key}\"");
                }
            Assert.True(missing.Count == 0, string.Join("\n", missing.Distinct()));
        }

        // Code values the views show through the localizer (statuses, kinds, honey classes, data types)
        public static IEnumerable<object[]> CodeValues()
        {
            var types = new[] { typeof(OrgStatus), typeof(MembershipStatus), typeof(SampleStatus), typeof(ShipmentStatus), typeof(LabReportStatus),
                typeof(RegistryStatus), typeof(CorrectionStatus), typeof(CcpDataType), typeof(CcpAppliesTo) };
            var values = types.SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => f.IsLiteral).Select(f => (string)f.GetRawConstantValue()!))
                .Concat(HoneyClasses.All).Concat(OrgDocumentKinds.All)
                .Concat(new[] { "Create", "Update", "Delete", "Login", "Approve", "Finalize", "Verify", "Administrator", "Beekeeper", "Wholesaler", "Consumer" });
            return values.Distinct().Select(v => new object[] { v });
        }

        [Theory]
        [MemberData(nameof(CodeValues))]
        public void Code_values_have_translations(string value)
        {
            foreach (var lang in Languages)
                Assert.True(Resources.Value[lang].ContainsKey(value), $"{lang}: missing \"{value}\"");
        }

        // Keys the beekeeper page builds at run time: regions, reference kinds, and the demo keys of StorefrontPage
        [Fact]
        public void Storefront_keys_built_at_run_time_exist()
        {
            var keys = Services.ProvinceMap.Regions.All.Select(r => "Region." + r)
                .Concat(typeof(Models.ViewModels.Storefront.ReferenceKinds).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => "Sf.Kind" + f.GetValue(null)))
                .Concat(Regex.Matches(File.ReadAllText(Path.Combine(WebRoot.Value, "Services", "OrgModel", "StorefrontPage.cs")), @"""(Sf\.[A-Za-z.]+)""").Select(m => m.Groups[1].Value))
                .Distinct().ToList();
            Assert.True(keys.Count > 20);
            foreach (var lang in Languages)
                foreach (var key in keys)
                    Assert.True(Resources.Value[lang].TryGetValue(key, out var text) && text.Length > 0, $"{lang}: missing \"{key}\"");
        }

        // The capabilities page explains every capability with the key "Cap.Uses.<code>" (built at run time)
        [Fact]
        public void Every_capability_has_an_explanation()
        {
            var codes = typeof(Capabilities).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => (string)f.GetValue(null)!).ToList();
            Assert.NotEmpty(codes);
            foreach (var lang in Languages)
                foreach (var code in codes)
                    Assert.True(Resources.Value[lang].TryGetValue("Cap.Uses." + code, out var text) && text.Length > 0, $"{lang}: missing \"Cap.Uses.{code}\"");
        }
    }
}
