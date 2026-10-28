using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Tests.E2E;
using Xunit;
using Xunit.Abstractions;

namespace TOP4HoneyChains.Web.Tests.Demo
{
    [CollectionDefinition("DemoBuild")]
    public class DemoBuildCollection : ICollectionFixture<DemoApp> { }

    // runs only when DEMO_EXPORT_DIR is set (the demo package is written outside the repository)
    public sealed class ExportFactAttribute : FactAttribute
    {
        public ExportFactAttribute()
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEMO_EXPORT_DIR")))
                Skip = "Set DEMO_EXPORT_DIR (and DEMO_CREDENTIALS_DIR) to write the demo package.";
        }
    }

    // runs only when DEMO_SCAN_DB names a database to check (e.g. one created from demo-database.sql)
    public sealed class ScanFactAttribute : FactAttribute
    {
        public ScanFactAttribute()
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEMO_SCAN_DB")))
                Skip = "Set DEMO_SCAN_DB to scan an existing demo database.";
        }
    }

    // D-86: the privacy scan of a demo database created from the script (DEMO_SCAN_DB), with its files (DEMO_SCAN_FILES)
    [Trait("Category", "E2E")]
    public class DemoDatabaseScanTests
    {
        private readonly ITestOutputHelper _output;
        public DemoDatabaseScanTests(ITestOutputHelper output) => _output = output;

        [ScanFact]
        public async Task A_demo_database_contains_no_data_of_the_local_database()
        {
            var db = Environment.GetEnvironmentVariable("DEMO_SCAN_DB")!;
            var report = await PrivacyScan.RunAsync(E2EApp.ConnectionStringOf(db), db, DemoApp.SourceDatabase,
                Environment.GetEnvironmentVariable("DEMO_SCAN_FILES"), DemoReference.RepoFile("TOP4HONEYCHAINS", "TOP4HoneyChains.Web", "App_Data"));
            _output.WriteLine(report.Summary);
            Assert.True(report.Findings.Count == 0, report.Summary);
        }
    }

    // D-86: the demo database is built through the application on every test run (DemoApp, DemoSeed) and checked here
    [Collection("DemoBuild")]
    [Trait("Category", "E2E")]
    public class DemoBuildTests
    {
        private readonly DemoApp _app;
        private readonly ITestOutputHelper _output;

        public DemoBuildTests(DemoApp app, ITestOutputHelper output)
        {
            _app = app;
            _output = output;
        }

        private static string LocalAppData => DemoReference.RepoFile("TOP4HONEYCHAINS", "TOP4HoneyChains.Web", "App_Data");

        [Fact]
        public async Task The_demo_contains_no_data_of_the_local_database()
        {
            var report = await PrivacyScan.RunAsync(_app.ConnectionString, DemoApp.BuildDatabase, DemoApp.SourceDatabase,
                Path.Combine(_app.ContentRoot, "App_Data"), LocalAppData);
            _output.WriteLine(report.Summary);
            Assert.True(report.Findings.Count == 0, report.Summary);
            Assert.True(report.SensitiveValues > 100, "the scan found the identifying values of the local database");
        }

        [Fact]
        public async Task Every_demo_account_signs_in()
        {
            foreach (var c in _app.Seed.Credentials)
            {
                var (_, response) = await UserClient.SignInAsync(_app, c.UserName, c.Password);
                Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
                var target = response.Headers.Location?.ToString() ?? "";
                Assert.DoesNotContain("/Account/Login", target);
                Assert.DoesNotContain("FirstPassword", target);
            }
            Assert.Equal(4 * (DemoCountries.Accounts.Length + 1), _app.Seed.Credentials.Count);
        }

        [Fact]
        public async Task Every_country_has_a_finished_example_honey_with_its_public_pages()
        {
            var visitor = UserClient.Anonymous(_app);
            foreach (var (code, country) in _app.Seed.Manifest.Countries)
            {
                var example = country.Example!;
                Assert.True(await _app.WithContextAsync(db => db.FinalHoneyProductions.AnyAsync(f => f.FinalHoneyProductionId == example.BatchId && f.FinalHoneyProductionIsFinalized)), code);
                var story = WebUtility.HtmlDecode(await visitor.HtmlAsync($"/honeystory/{example.Qr}?culture=en"));
                Assert.Contains(country.Beekeepers["b1"].Name, story);           // the name is shown: consent at registration
                Assert.Contains("id=\"badge-accredited\"", story);
                Assert.Contains("id=\"badge-origin\"", story);
                Assert.Contains($"/storefront/{example.StorefrontCode}", story);
                Assert.Contains(country.Beekeepers["b1"].Name, WebUtility.HtmlDecode(await visitor.HtmlAsync($"/storefront/{example.StorefrontCode}?culture=en")));
                Assert.Equal(1, await _app.WithContextAsync(db => db.Samples.CountAsync(s => s.RawHoneyProductionId == example.LotId && s.Status == TOP4HoneyChains.Web.Models.Entities.SampleStatus.Rejected)));
            }
        }

        [Fact]
        public async Task The_demo_inbox_shows_the_codes_of_the_plant()
        {
            var c = _app.Seed.Credentials.First(x => x.Country == "AR" && x.AccountKey == "reception");
            var (client, _) = await UserClient.SignInAsync(_app, c.UserName, c.Password);
            var inbox = await client.HtmlAsync("/Demo/Inbox");
            Assert.Contains("class=\"demo-code\"", inbox);
            Assert.Contains("id=\"demo-banner\"", inbox);
        }

        // the generated scripts: the create script builds the same database on an empty one and refuses a second run; the reset
        // script restores the demo rows after changes and refuses a database without the marker
        [Fact]
        public async Task The_scripts_create_and_reset_the_demo_database()
        {
            const string scratch = "TOP4HoneyChainsDemoScript";
            var cs = E2EApp.ConnectionStringOf(scratch);
            var schema = await File.ReadAllTextAsync(DemoReference.RepoFile("db", "demo", "schema.sql"));
            var (create, reset, _, rows) = await DemoScript.BuildAsync(_app.ConnectionString, schema, "test");
            async Task<long> RowsAsync()
            {
                await using var conn = new Microsoft.Data.SqlClient.SqlConnection(cs);
                await conn.OpenAsync();
                return Convert.ToInt64(await new Microsoft.Data.SqlClient.SqlCommand(
                    "SELECT SUM(p.rows) FROM sys.tables t JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0, 1)", conn).ExecuteScalarAsync());
            }
            async Task MasterAsync(string sql)
            {
                await using var conn = new Microsoft.Data.SqlClient.SqlConnection(E2EApp.ConnectionStringOf("master"));
                await conn.OpenAsync();
                await new Microsoft.Data.SqlClient.SqlCommand(sql, conn).ExecuteNonQueryAsync();
            }
            var drop = $"IF DB_ID('{scratch}') IS NOT NULL BEGIN ALTER DATABASE [{scratch}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{scratch}]; END";
            await MasterAsync(drop + $"; CREATE DATABASE [{scratch}]");
            try
            {
                // the reset refuses a database without the marker
                var refused = await Assert.ThrowsAnyAsync<Microsoft.Data.SqlClient.SqlException>(() => DemoReference.RunScriptAsync(cs, reset));
                Assert.Contains("not the demo database", refused.Message);

                await DemoReference.RunScriptAsync(cs, create);
                Assert.Equal(rows, await RowsAsync());
                Assert.True(await DemoDatabaseIsCheckedAsync(cs));
                Assert.Contains("only on a new, empty database", (await Assert.ThrowsAnyAsync<Microsoft.Data.SqlClient.SqlException>(() => DemoReference.RunScriptAsync(cs, create))).Message);

                // changes by testers are undone by the reset; the counters start after the demo rows again
                await DemoReference.RunScriptAsync(cs, "DELETE FROM dbo.Shipments; INSERT dbo.Wholesalers (WholesalerTitle, WholesalerIsActive, CreatedAt) VALUES (N'[DEMO] tester', 1, SYSDATETIME());");
                await DemoReference.RunScriptAsync(cs, reset);
                Assert.Equal(rows, await RowsAsync());
                Assert.True(await DemoDatabaseIsCheckedAsync(cs));
                await DemoReference.RunScriptAsync(cs, "INSERT dbo.Wholesalers (WholesalerTitle, WholesalerIsActive, CreatedAt) VALUES (N'[DEMO] after reset', 1, SYSDATETIME());");
                await using var conn = new Microsoft.Data.SqlClient.SqlConnection(cs);
                await conn.OpenAsync();
                Assert.Equal(5, Convert.ToInt32(await new Microsoft.Data.SqlClient.SqlCommand("SELECT MAX(WholesalerId) FROM dbo.Wholesalers", conn).ExecuteScalarAsync()));
            }
            finally
            {
                Microsoft.Data.SqlClient.SqlConnection.ClearAllPools();
                await MasterAsync(drop);
            }
        }

        // every constraint and trigger the schema switches on is on and trusted again after loading the rows
        private static async Task<bool> DemoDatabaseIsCheckedAsync(string cs)
        {
            await using var conn = new Microsoft.Data.SqlClient.SqlConnection(cs);
            await conn.OpenAsync();
            var off = Convert.ToInt32(await new Microsoft.Data.SqlClient.SqlCommand(@"SELECT (SELECT COUNT(*) FROM sys.check_constraints WHERE is_not_trusted = 1 AND name <> 'CK_Consents_Purpose')
                + (SELECT COUNT(*) FROM sys.foreign_keys WHERE is_not_trusted = 1) + (SELECT COUNT(*) FROM sys.triggers WHERE is_disabled = 1)", conn).ExecuteScalarAsync());
            return off == 0;
        }

        [ExportFact]
        public async Task Export_the_demo_package()
        {
            var outDir = Environment.GetEnvironmentVariable("DEMO_EXPORT_DIR")!;
            var date = Environment.GetEnvironmentVariable("DEMO_DATE") ?? DateTime.Today.ToString("yyyy-MM-dd");
            var report = await PrivacyScan.RunAsync(_app.ConnectionString, DemoApp.BuildDatabase, DemoApp.SourceDatabase,
                Path.Combine(_app.ContentRoot, "App_Data"), LocalAppData);
            Assert.True(report.Findings.Count == 0, report.Summary);

            var schema = await File.ReadAllTextAsync(DemoReference.RepoFile("db", "demo", "schema.sql"));
            var (create, reset, tables, rows) = await DemoScript.BuildAsync(_app.ConnectionString, schema, date);
            var utf8 = new UTF8Encoding(true);
            Directory.CreateDirectory(Path.Combine(outDir, "SQL"));
            await File.WriteAllTextAsync(Path.Combine(outDir, "SQL", "demo-database.sql"), create.Replace("\r\n", "\n").Replace("\n", "\r\n"), utf8);
            await File.WriteAllTextAsync(Path.Combine(outDir, "SQL", "demo-reset.sql"), reset.Replace("\r\n", "\n").Replace("\n", "\r\n"), utf8);

            // the uploaded files of the demo (lab reports, signed forms, consent documents)
            var files = Path.Combine(outDir, "DemoFiles", "App_Data");
            if (Directory.Exists(files))
                Directory.Delete(files, true);
            var source = Path.Combine(_app.ContentRoot, "App_Data");
            foreach (var f in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories).Where(f => !Path.GetRelativePath(source, f).StartsWith("logs", StringComparison.Ordinal)))
            {
                var target = Path.Combine(files, Path.GetRelativePath(source, f));
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(f, target);
            }

            // the manifest for the partner test guide (no passwords)
            var guide = Path.Combine(DemoReference.RepoFile("docs"), "partner-test");
            Directory.CreateDirectory(guide);
            await File.WriteAllTextAsync(Path.Combine(guide, "demo-manifest.json"),
                JsonSerializer.Serialize(_app.Seed.Manifest, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }) + "\n",
                new UTF8Encoding(false));

            // the credential sheets, outside the repository
            if (Environment.GetEnvironmentVariable("DEMO_CREDENTIALS_DIR") is string pack && pack.Length > 0)
                await DemoCredentials.WriteAsync(pack, _app.Seed, date, _app.ConnectionString);
            _output.WriteLine($"{tables} tables, {rows} rows; {report.Summary}");
        }
    }
}
