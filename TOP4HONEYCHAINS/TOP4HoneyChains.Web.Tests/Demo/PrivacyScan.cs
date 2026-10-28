using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace TOP4HoneyChains.Web.Tests.Demo
{
    // D-86: the privacy scan of a demo database. It never prints a value, only table, column and counts.
    //  1. Every text and id column of every table of the demo database is compared with the identifying values of the local database
    //     (a copy of production): user ids, user names, e-mails, phones, beekeeper and apiary names, business and registry numbers,
    //     organization and wholesaler names, tax numbers, addresses. An exact match, or a long value contained in a demo text (an
    //     audit entry, an SMS), is a finding. Phones are compared by their digits.
    //  2. Every row that describes a person or an organization must be a demo row: [DEMO] names, @example.invalid e-mail
    //     addresses, the demo user names, the demo phone numbers.
    //  3. Uploaded files (App_Data) of the demo differ from every file of the local application.
    public static class PrivacyScan
    {
        public record Finding(string Table, string Column, string Rule, int Rows);

        public record Report(List<Finding> Findings, int Tables, int Columns, int SensitiveValues, int Files)
        {
            public string Summary => Findings.Count == 0
                ? $"clean: {Tables} tables, {Columns} text/id columns checked against {SensitiveValues} identifying values of the local database; {Files} files"
                : string.Join("\n", Findings.Select(f => $"{f.Table}.{f.Column}: {f.Rule} ({f.Rows} rows)"));
        }

        // the identifying columns of the local database (table, column)
        private static readonly (string Table, string Column)[] Sensitive =
        {
            ("AspNetUsers", "Id"), ("AspNetUsers", "UserName"), ("AspNetUsers", "Email"), ("AspNetUsers", "PhoneNumber"),
            ("Beekeepers", "BeekeeperId"), ("Beekeepers", "BeekeeperBusinessNumber"), ("Beekeepers", "BeekeeperPhoneNumber"),
            ("Apiaries", "ApiaryTitle"), ("Apiaries", "Locality"), ("ApiaryPhotos", "ApiaryPhotoDesc"), ("ApiaryPhotos", "ApiaryPhotoFileName"),
            ("BeekeeperRegistry", "RegistryNumber"), ("RegistryVerifications", "EnterpriseNumber"),
            ("Organizations", "DisplayName"), ("Organizations", "LegalName"), ("Organizations", "TaxNo"),
            ("Organizations", "DataControllerName"), ("Organizations", "DataControllerAddress"), ("Organizations", "DataControllerContact"),
            ("Wholesalers", "WholesalerTitle"), ("Wholesalers", "WholesalerTaxNo"), ("Wholesalers", "WholesalerContactName"),
            ("Wholesalers", "WholesalerPhone"), ("Wholesalers", "WholesalerEmail"), ("Wholesalers", "WholesalerAddress"),
            ("FinalHoneyProductions", "FinalHoneyProductionTitle"), ("SmsMessages", "ToPhoneHash"), ("OrgDocuments", "StoredFileName")
        };

        private static readonly string[] NotPersonal = { "Platform administration" };

        private static string Digits(string s) => new(s.Where(char.IsAsciiDigit).ToArray());

        private static async Task<List<string>> ColumnValuesAsync(SqlConnection conn, string database, string table, string column)
        {
            var values = new List<string>();
            var exists = await new SqlCommand($"SELECT COL_LENGTH('[{database}].dbo.[{table}]', '{column}')", conn).ExecuteScalarAsync();
            if (exists is DBNull or null)
                return values;
            await using var cmd = new SqlCommand($"SELECT DISTINCT CAST([{column}] AS nvarchar(max)) FROM [{database}].dbo.[{table}] WHERE [{column}] IS NOT NULL", conn) { CommandTimeout = 300 };
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                values.Add(r.GetString(0).Trim());
            return values;
        }

        public static async Task<Report> RunAsync(string connectionString, string demoDatabase, string sourceDatabase, string? demoFiles, string? sourceFiles)
        {
            var findings = new List<Finding>();
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            // ---- 1. identifying values of the local database (kept in memory only)
            var exact = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var phones = new HashSet<string>();
            foreach (var (table, column) in Sensitive)
                foreach (var v in await ColumnValuesAsync(conn, sourceDatabase, table, column))
                {
                    if (v.Length >= 4)
                        exact.Add(v);
                    if (column.Contains("Phone", StringComparison.Ordinal) && Digits(v).Length >= 9)
                        phones.Add(Digits(v)[^9..]);
                }
            // full names (a first name or a common surname alone identifies nobody), as they appear in texts
            await using (var cmd = new SqlCommand($"SELECT BeekeeperFirstName + ' ' + BeekeeperLastName FROM [{sourceDatabase}].dbo.Beekeepers WHERE BeekeeperFirstName IS NOT NULL AND BeekeeperLastName IS NOT NULL", conn))
            await using (var r = await cmd.ExecuteReaderAsync())
                while (await r.ReadAsync())
                    exact.Add(r.GetString(0).Trim());
            // the platform organization's name comes from migration 004, not from a person
            exact.ExceptWith(NotPersonal);
            var contained = exact.Where(v => v.Length >= 10).ToList();

            // ---- every text and id column of every table of the demo database
            var columns = new List<(string Table, string Column)>();
            await using (var cmd = new SqlCommand($@"SELECT t.name, c.name FROM [{demoDatabase}].sys.tables t JOIN [{demoDatabase}].sys.columns c ON c.object_id = t.object_id
                JOIN [{demoDatabase}].sys.types ty ON ty.user_type_id = c.user_type_id
                WHERE ty.name IN ('nvarchar','varchar','nchar','char','ntext','text','uniqueidentifier') ORDER BY t.name, c.column_id", conn))
            await using (var r = await cmd.ExecuteReaderAsync())
                while (await r.ReadAsync())
                    columns.Add((r.GetString(0), r.GetString(1)));
            var tables = columns.Select(c => c.Table).Distinct().Count();
            var reference = DemoReference.Tables.Select(t => t.Table).ToHashSet();
            // the reference tables are copied on purpose (CCP definitions, provinces, role texts…), without any person's id
            foreach (var (table, column) in columns.Where(c => !reference.Contains(c.Table)))
            {
                var values = await ColumnValuesAsync(conn, demoDatabase, table, column);
                var hits = values.Count(v => exact.Contains(v) || (v.Length >= 10 && contained.Any(s => v.Contains(s, StringComparison.OrdinalIgnoreCase))));
                if (hits > 0)
                    findings.Add(new Finding(table, column, "value of the local database", hits));
                if (column.Contains("Phone", StringComparison.Ordinal) && !column.Contains("Hash", StringComparison.Ordinal) && !column.Contains("Masked", StringComparison.Ordinal))
                {
                    var phoneHits = values.Count(v => Digits(v).Length >= 9 && phones.Contains(Digits(v)[^9..]));
                    if (phoneHits > 0)
                        findings.Add(new Finding(table, column, "phone of the local database", phoneHits));
                }
            }

            // ---- 2. rows about people and organizations are demo rows
            async Task RuleAsync(string table, string column, string where, string rule)
            {
                var n = Convert.ToInt32(await new SqlCommand($"SELECT COUNT(*) FROM [{demoDatabase}].dbo.[{table}] WHERE NOT ({where})", conn).ExecuteScalarAsync());
                if (n > 0)
                    findings.Add(new Finding(table, column, rule, n));
            }
            const string demoUser = "(UserName LIKE '[a-z][a-z].%' AND UserName NOT LIKE '%[^a-z.]%') OR UserName LIKE '[A-Z][A-Z]-BEE-DEMO-[0-9][0-9][0-9][0-9][0-9][0-9]'";
            await RuleAsync("AspNetUsers", "UserName", demoUser, "not a demo user name");
            await RuleAsync("AspNetUsers", "Email", "Email IS NULL OR Email LIKE '%@example.invalid'", "not an @example.invalid address");
            await RuleAsync("Beekeepers", "BeekeeperFirstName", "BeekeeperFirstName LIKE '[[]DEMO]%'", "beekeeper not marked [DEMO]");
            await RuleAsync("Organizations", "DisplayName", "DisplayName LIKE '[[]DEMO]%' OR IsPlatform = 1", "organization not marked [DEMO]");
            await RuleAsync("Wholesalers", "WholesalerTitle", "WholesalerTitle LIKE '[[]DEMO]%'", "wholesaler not marked [DEMO]");
            await RuleAsync("Apiaries", "ApiaryBeekeeperId", $"ApiaryBeekeeperId IN (SELECT BeekeeperId FROM [{demoDatabase}].dbo.Beekeepers WHERE BeekeeperFirstName LIKE '[[]DEMO]%')", "apiary of a non-demo beekeeper");
            await RuleAsync("ApiaryPhotos", "ApiaryPhotoId", "1 = 0", "apiary photos in the demo");
            await RuleAsync("Notices", "OrgId", $"OrgId IS NULL OR OrgId IN (SELECT OrgId FROM [{demoDatabase}].dbo.Organizations WHERE DisplayName LIKE '[[]DEMO]%')", "notice of a non-demo organization");
            await RuleAsync("AuditEntries", "UserId", $"UserId IS NULL OR UserId IN (SELECT Id FROM [{demoDatabase}].dbo.AspNetUsers)", "audit entry of an unknown user");
            const string demoPhone = "{0} IS NULL OR {0} = '' OR {0} LIKE '+549343455%' OR {0} LIKE '+48512000%' OR {0} LIKE '+3712000%' OR {0} LIKE '+90555000%'";
            await RuleAsync("Beekeepers", "BeekeeperPhoneNumber", string.Format(demoPhone, "BeekeeperPhoneNumber"), "not a demo phone");
            await RuleAsync("AspNetUsers", "PhoneNumber", string.Format(demoPhone, "PhoneNumber"), "not a demo phone");

            // ---- 3. no uploaded file of the local application
            var files = 0;
            if (demoFiles != null && Directory.Exists(demoFiles))
            {
                var local = new HashSet<string>();
                if (sourceFiles != null && Directory.Exists(sourceFiles))
                    foreach (var f in Directory.EnumerateFiles(sourceFiles, "*", SearchOption.AllDirectories).Where(f => !f.Contains(Path.DirectorySeparatorChar + "logs" + Path.DirectorySeparatorChar)))
                        local.Add(Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(f))));
                foreach (var f in Directory.EnumerateFiles(demoFiles, "*", SearchOption.AllDirectories))
                {
                    files++;
                    if (local.Contains(Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(f)))))
                        findings.Add(new Finding("App_Data", Path.GetFileName(Path.GetDirectoryName(f)!), "file of the local application", 1));
                }
            }
            return new Report(findings, tables, columns.Count, exact.Count + phones.Count, files);
        }
    }
}
