using Microsoft.Data.SqlClient;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // The EF model must describe the real database (database-first, CLAUDE.md): the schema EF creates for the
    // test database is compared column by column with the local development database (types, lengths,
    // nullability). Only schema metadata is read, never rows.
    [Collection("E2E")]
    [Trait("Category", "E2E")]
    public class SchemaDriftTests
    {
        private readonly E2EApp _app;

        public SchemaDriftTests(E2EApp app) => _app = app;

        private const string Columns = @"
SELECT c.TABLE_NAME, c.COLUMN_NAME, c.DATA_TYPE, ISNULL(c.CHARACTER_MAXIMUM_LENGTH, 0), ISNULL(c.NUMERIC_PRECISION, 0), ISNULL(c.NUMERIC_SCALE, 0), c.IS_NULLABLE
FROM [{0}].INFORMATION_SCHEMA.COLUMNS c
JOIN [{0}].INFORMATION_SCHEMA.TABLES t ON t.TABLE_NAME = c.TABLE_NAME AND t.TABLE_TYPE = 'BASE TABLE'
WHERE c.TABLE_SCHEMA = 'dbo' AND c.TABLE_NAME NOT IN ('sysdiagrams', '__EFMigrationsHistory')";

        private static async Task<Dictionary<string, string>> ReadAsync(SqlConnection conn, string database)
        {
            var result = new Dictionary<string, string>();
            await using var cmd = new SqlCommand(string.Format(Columns, database), conn);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                var type = r.GetString(2);
                var detail = type is "decimal" or "numeric" ? $"{type}({r.GetByte(4)},{r.GetInt32(5)})"
                    : r.GetInt32(3) != 0 ? $"{type}({r.GetInt32(3)})" : type;
                result[$"{r.GetString(0)}.{r.GetString(1)}"] = detail + (r.GetString(6) == "YES" ? " null" : " not null");
            }
            return result;
        }

        // v1.0 entities mark these columns [Required] (form validation) although the database allows NULL;
        // EF therefore creates them NOT NULL. Kept on purpose: changing them would change v1.0 queries (D-46).
        private static readonly HashSet<string> KnownNullability = new()
        {
            "Apiaries.ApiaryIsActive", "ApiaryPhotos.ApiaryPhotoApiaryId", "Beekeepers.BeekeeperBusinessNumber",
            "CriticalControlPointParameterCategories.CategoryName", "CriticalControlPointParameterContents.IsActive",
            "CriticalControlPointParameterContents.SortOrder", "CriticalControlPoints.AppliesTo",
            "FinalHoneyCCPAssignments.FinalHoneyCCPAssignmentCCP", "FinalHoneyCCPAssignments.FinalHoneyCCPAssignmentStartDate",
            "FinalHoneyCCPAssignments.FinalHoneyProductionId", "FinalHoneyProductions.FinalHoneyProductionDate",
            "FinalHoneyProductions.FinalHoneyProductionTitle", "RawHoneyCCPAssignments.RawHoneyCCPAssignmentCCP",
            "RawHoneyCCPAssignments.RawHoneyCCPAssignmentStartDate", "RawHoneyCCPAssignments.RawHoneyProductionId",
            "RawHoneyProductionPeriods.RawHoneyProductionPeriodEndDate", "RawHoneyProductionPeriods.RawHoneyProductionPeriodStartDate",
            "RawHoneyProductionPeriods.RawHoneyProductionPeriodTitle", "RawHoneyProductions.RawHoneyProductionAmount",
            "RawHoneyProductions.RawHoneyProductionApiaryId", "RawHoneyProductions.RawHoneyProductionHoneyType",
            "RawHoneyProductions.RawHoneyProductionPeriodId", "RawHoneyProductions.RawHoneyProductionWholesalerId"
        };

        private static string WithoutNullability(string column) => column.Replace(" not null", "").Replace(" null", "");

        [Fact]
        public async Task Ef_model_matches_the_database()
        {
            await using var conn = new SqlConnection(_app.ConnectionString);
            await conn.OpenAsync();
            var model = await ReadAsync(conn, E2EApp.Database);
            var real = await ReadAsync(conn, E2EApp.SourceDatabase);
            var differences = real.Keys.Intersect(model.Keys)
                .Where(k => real[k] != model[k] && !(KnownNullability.Contains(k) && WithoutNullability(real[k]) == WithoutNullability(model[k])))
                .Select(k => $"{k}: database {real[k]}, EF {model[k]}")
                .Concat(real.Keys.Where(k => !model.ContainsKey(k) && model.Keys.Any(m => m.StartsWith(k.Split('.')[0] + ".")))
                    .Select(k => $"{k}: missing in the EF model"))
                .OrderBy(x => x).ToList();
            Assert.True(differences.Count == 0, "Schema drift:\n" + string.Join("\n", differences));
        }
    }
}
