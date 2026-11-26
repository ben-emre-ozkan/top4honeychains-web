using Microsoft.Data.SqlClient;
using TOP4HoneyChains.Web.Services;
using TOP4HoneyChains.Web.Tests.E2E;

namespace TOP4HoneyChains.Web.Tests.Demo
{
    // D-86: the demo database, built like the professor builds it: the live schema (db/demo/schema.sql, after 001-028), the
    // reference tables copied from the local database without any link to a person, the demo regions and test season, the demo
    // marker; then the application runs as a demo site and DemoSeed records the demo data of the four partner countries through
    // its pages. Nothing else is copied from the local database.
    public sealed class DemoApp : E2EApp
    {
        public static readonly string BuildDatabase = Environment.GetEnvironmentVariable("DEMO_BUILD_DB") ?? "TOP4HoneyChainsDemoBuild";
        public static string SourceDatabase => Environment.GetEnvironmentVariable("DEMO_SOURCE_DB") ?? "TOP4HoneyChainsDb";

        public DemoSeed.Result Seed { get; private set; } = null!;

        protected override string DatabaseName => BuildDatabase;

        protected override void AddSettings(Dictionary<string, string?> settings)
        {
            settings["Demo:Enabled"] = "true";
            settings["Sms:AllowedCountries"] = "TR,AR,PL,LV";
            settings["Sms:DailyLimit"] = "100000";
            settings["Features:CustodyIntake"] = "true";
            settings["Features:BeekeeperAccounts"] = "true";
            settings["Notices:Enforce"] = "false";
            settings["Lab:AnalysisMode"] = "Composite";
        }

        public override async Task InitializeAsync()
        {
            await DropDatabaseAsync();
            await MasterAsync($"CREATE DATABASE [{BuildDatabase}]; ALTER DATABASE [{BuildDatabase}] SET COMPATIBILITY_LEVEL = 140;");
            await DemoReference.RunScriptAsync(ConnectionString, await File.ReadAllTextAsync(DemoReference.RepoFile("db", "demo", "schema.sql")));
            await DemoReference.CopyAsync(ConnectionString, SourceDatabase);
            await DemoReference.AddDemoRowsAsync(ConnectionString);
            await DemoReference.RunScriptAsync(ConnectionString, DemoReference.MarkerSql);
            Seed = await new DemoSeed(this).RunAsync();
        }
    }

    public static class DemoReference
    {
        // the reference tables, in dependency order; a person's id in them is not copied
        public static readonly (string Table, string? Where, string[] NullColumns)[] Tables =
        {
            ("__EFMigrationsHistory", null, Array.Empty<string>()),
            ("AspNetRoles", null, Array.Empty<string>()),
            ("AssignmentStatuses", null, Array.Empty<string>()),
            ("HoneyTypes", null, Array.Empty<string>()),
            ("RawHoneyProductionPeriods", null, Array.Empty<string>()),
            ("BeekeeperEducationLevels", null, Array.Empty<string>()),
            ("BeekeepingPurposeTypes", null, Array.Empty<string>()),
            ("BeekeepingTypes", null, Array.Empty<string>()),
            ("Capabilities", null, Array.Empty<string>()),
            ("OrgRoles", null, Array.Empty<string>()),
            ("CriticalControlPoints", null, Array.Empty<string>()),
            ("CriticalControlPointParameterCategories", null, Array.Empty<string>()),
            ("CriticalControlPointParameters", null, Array.Empty<string>()),
            ("CriticalControlPointParameterLimits", null, Array.Empty<string>()),
            ("CriticalControlPointParameterContents", null, Array.Empty<string>()),
            ("CcpResponsibilities", null, new[] { "UpdatedByUserId" }),
            ("CcpResponsibilityCapabilities", null, Array.Empty<string>()),
            ("CcpResponsibilityRoles", null, Array.Empty<string>()),
            ("Provinces", null, Array.Empty<string>()),
            ("Districts", null, Array.Empty<string>()),
            ("RolePermissions", null, new[] { "UpdatedByUserId" }),
            ("ReferenceTranslations", null, new[] { "UpdatedByUserId" }),
            // the platform's notices only (organization notices belong to organizations that are not copied)
            ("Notices", "OrgId IS NULL", new[] { "PublishedByUserId" })
        };

        public const string DemoPeriodTitle = "[DEMO] Test season 2026–2027 (all countries)";

        public static string MarkerSql =>
            $"EXEC sys.sp_addextendedproperty @name = N'{DemoDatabase.MarkerName}', @value = N'1';";

        public static string RepoFile(params string[] parts)
        {
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            {
                var path = Path.Combine(new[] { dir.FullName }.Concat(parts).ToArray());
                if (File.Exists(path) || Directory.Exists(path))
                    return path;
            }
            throw new FileNotFoundException("not found above the test folder: " + Path.Combine(parts));
        }

        // runs a script with GO batch separators
        public static async Task RunScriptAsync(string connectionString, string script)
        {
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            var batch = new System.Text.StringBuilder();
            foreach (var line in script.Replace("\r\n", "\n").Split('\n').Append("GO"))
            {
                if (!line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
                {
                    batch.AppendLine(line);
                    continue;
                }
                var sql = batch.ToString();
                batch.Clear();
                if (string.IsNullOrWhiteSpace(sql))
                    continue;
                await using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 300 };
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task CopyAsync(string connectionString, string source)
        {
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            foreach (var (table, where, nulls) in Tables)
            {
                var columns = new List<string>();
                await using (var cmd = new SqlCommand("SELECT name FROM sys.columns WHERE object_id = OBJECT_ID(@t) AND is_computed = 0 ORDER BY column_id", conn))
                {
                    cmd.Parameters.AddWithValue("@t", "dbo." + table);
                    await using var r = await cmd.ExecuteReaderAsync();
                    while (await r.ReadAsync())
                        columns.Add(r.GetString(0));
                }
                var identity = Convert.ToInt32(await new SqlCommand($"SELECT COUNT(*) FROM sys.identity_columns WHERE object_id = OBJECT_ID('dbo.[{table}]')", conn).ExecuteScalarAsync()) > 0;
                var list = string.Join(", ", columns.Select(c => $"[{c}]"));
                var select = string.Join(", ", columns.Select(c => nulls.Contains(c) ? "NULL" : $"[{c}]"));
                var sql = (identity ? $"SET IDENTITY_INSERT dbo.[{table}] ON; " : "")
                          + $"INSERT INTO dbo.[{table}] ({list}) SELECT {select} FROM [{source}].dbo.[{table}]" + (where != null ? " WHERE " + where : "") + ";"
                          + (identity ? $" SET IDENTITY_INSERT dbo.[{table}] OFF;" : "");
                await using var copy = new SqlCommand(sql, conn) { CommandTimeout = 300 };
                await copy.ExecuteNonQueryAsync();
            }
        }

        // regions of the partner countries in the province list (A8: the platform knows Türkiye's provinces only), a test season
        // that covers the testing months, and the platform organization (migration 004 creates it on the live database)
        public static async Task AddDemoRowsAsync(string connectionString)
        {
            var sql = new System.Text.StringBuilder();
            foreach (var country in DemoCountries.All)
                foreach (var region in country.Regions)
                {
                    sql.AppendLine($"IF NOT EXISTS (SELECT 1 FROM dbo.Provinces WHERE Code = '{region.Code}') INSERT dbo.Provinces (Code, Name, Source) VALUES ('{region.Code}', N'{region.Name}', N'[DEMO] partner-country region for the demo (A8)');");
                    foreach (var d in region.Districts)
                        sql.AppendLine($"IF NOT EXISTS (SELECT 1 FROM dbo.Districts WHERE ProvinceCode = '{region.Code}' AND Name = N'{d}') INSERT dbo.Districts (ProvinceCode, Name) VALUES ('{region.Code}', N'{d}');");
                }
            sql.AppendLine($"IF NOT EXISTS (SELECT 1 FROM dbo.RawHoneyProductionPeriods WHERE RawHoneyProductionPeriodTitle = N'{DemoPeriodTitle}') "
                           + $"INSERT dbo.RawHoneyProductionPeriods (RawHoneyProductionPeriodTitle, RawHoneyProductionPeriodStartDate, RawHoneyProductionPeriodEndDate) VALUES (N'{DemoPeriodTitle}', '2026-09-01', '2027-12-31');");
            sql.AppendLine("IF NOT EXISTS (SELECT 1 FROM dbo.Organizations WHERE IsPlatform = 1) INSERT dbo.Organizations (DisplayName, Status, IsPlatform, VerifiedAt, CreatedAt) VALUES (N'Platform administration', N'Verified', 1, SYSUTCDATETIME(), SYSUTCDATETIME());");
            sql.AppendLine("INSERT dbo.OrgCapabilities (OrgId, Capability, IsAccredited, AddedAt) SELECT OrgId, N'PlatformAdmin', 0, SYSUTCDATETIME() FROM dbo.Organizations o WHERE IsPlatform = 1 AND NOT EXISTS (SELECT 1 FROM dbo.OrgCapabilities c WHERE c.OrgId = o.OrgId AND c.Capability = N'PlatformAdmin');");
            await RunScriptAsync(connectionString, sql.ToString());
        }
    }
}
