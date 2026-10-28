using System.Globalization;
using System.Text;
using Microsoft.Data.SqlClient;
using TOP4HoneyChains.Web.Services;

namespace TOP4HoneyChains.Web.Tests.Demo
{
    // D-86: writes the demo database as two T-SQL scripts for SQL Server 2019 (SSMS or sqlcmd):
    //  - demo-database.sql: on a new, empty database only: the schema (db/demo/schema.sql), the demo marker and every row;
    //  - demo-reset.sql: on the demo database only (it must carry the marker): deletes every row and loads the same rows again.
    // Both refuse to run anywhere else; neither ever touches another database.
    public static class DemoScript
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        private record Column(string Name, string Type);

        private record TableData(string Name, List<Column> Columns, bool Identity, string? IdentityColumn, List<object?[]> Rows);

        public static async Task<(string Create, string Reset, int Tables, int Rows)> BuildAsync(string connectionString, string schemaSql, string date)
        {
            var tables = await ReadAsync(connectionString);
            var enable = await EnableSqlAsync(connectionString);
            var data = Data(tables, enable);
            var rows = tables.Sum(t => t.Rows.Count);

            var create = new StringBuilder();
            create.AppendLine("-- TOP4HoneyChains DEMO database (D-86), " + date + ". Creates the schema and loads the demo data.");
            create.AppendLine("-- Run it ONCE on a NEW, EMPTY database (e.g. TOP4HoneyChainsDemo). It refuses to run on a database that has tables.");
            create.AppendLine("-- Never run it on the live database. Demo data only: no real person, beekeeper, organization, phone or file.");
            create.AppendLine("SET NOCOUNT ON;");
            create.AppendLine("GO");
            create.AppendLine("IF DB_NAME() IN (N'master', N'model', N'msdb', N'tempdb') OR EXISTS (SELECT 1 FROM sys.tables WHERE is_ms_shipped = 0)");
            create.AppendLine("BEGIN");
            create.AppendLine("    RAISERROR(N'STOP: this script runs only on a new, empty database. Nothing was changed.', 16, 1);");
            create.AppendLine("    SET NOEXEC ON;");
            create.AppendLine("END;");
            create.AppendLine("GO");
            create.AppendLine("ALTER DATABASE CURRENT SET COMPATIBILITY_LEVEL = 140;");
            create.AppendLine("GO");
            create.AppendLine(schemaSql.TrimStart('﻿'));
            create.AppendLine("-------------------------------------------------------------------- demo marker");
            create.AppendLine($"EXEC sys.sp_addextendedproperty @name = N'{DemoDatabase.MarkerName}', @value = N'1';");
            create.AppendLine("GO");
            create.Append(data);
            create.AppendLine("PRINT N'TOP4HoneyChains demo database created.';");
            create.AppendLine("GO");
            create.AppendLine("SET NOEXEC OFF;");
            create.AppendLine("GO");

            var reset = new StringBuilder();
            reset.AppendLine("-- TOP4HoneyChains DEMO database reset (D-86), " + date + ". Deletes every row of the demo database and loads the demo data again.");
            reset.AppendLine("-- Runs only on a database that carries the demo marker (created by demo-database.sql); the live database never has it.");
            reset.AppendLine("SET NOCOUNT ON;");
            reset.AppendLine("GO");
            reset.AppendLine($"IF NOT EXISTS (SELECT 1 FROM sys.extended_properties WHERE class = 0 AND name = N'{DemoDatabase.MarkerName}' AND CAST(value AS nvarchar(20)) = N'1')");
            reset.AppendLine("BEGIN");
            reset.AppendLine("    RAISERROR(N'STOP: this is not the demo database (no demo marker). Nothing was changed.', 16, 1);");
            reset.AppendLine("    SET NOEXEC ON;");
            reset.AppendLine("END;");
            reset.AppendLine("GO");
            reset.AppendLine("-------------------------------------------------------------------- delete every row");
            reset.AppendLine(DisableSql);
            reset.AppendLine("GO");
            foreach (var t in Enumerable.Reverse(tables))
                reset.AppendLine($"DELETE FROM dbo.[{t.Name}];");
            reset.AppendLine("GO");
            reset.Append(data);
            reset.AppendLine("-------------------------------------------------------------------- identity counters");
            foreach (var t in tables.Where(t => t.Identity))
            {
                // a table that never had a row keeps its counter (a reseed to 0 would give the next row the id 0)
                var max = t.Rows.Count == 0 ? 0 : t.Rows.Max(r => Convert.ToInt64(r[t.Columns.FindIndex(c => c.Name == t.IdentityColumn)], Inv));
                reset.AppendLine($"IF (SELECT last_value FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'dbo.[{t.Name}]')) IS NOT NULL DBCC CHECKIDENT ('dbo.[{t.Name}]', RESEED, {max}) WITH NO_INFOMSGS;");
            }
            reset.AppendLine("PRINT N'TOP4HoneyChains demo database reset.';");
            reset.AppendLine("GO");
            reset.AppendLine("SET NOEXEC OFF;");
            reset.AppendLine("GO");
            return (create.ToString(), reset.ToString(), tables.Count, rows);
        }

        private const string DisableSql =
            "DECLARE @sql nvarchar(max) = N''; SELECT @sql += N'ALTER TABLE dbo.' + QUOTENAME(name) + N' NOCHECK CONSTRAINT ALL; ALTER TABLE dbo.' + QUOTENAME(name) + N' DISABLE TRIGGER ALL; ' FROM sys.tables WHERE is_ms_shipped = 0; EXEC sys.sp_executesql @sql;";

        // the constraints and triggers that are on in the schema are switched on again (and the constraints checked); a constraint a
        // migration switched off (e.g. CK_Consents_Purpose, replaced by CK_Consents_Purpose2 in 025) stays off
        private static async Task<string> EnableSqlAsync(string connectionString)
        {
            var sql = new StringBuilder();
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            await using (var cmd = new SqlCommand(@"SELECT OBJECT_NAME(parent_object_id), name FROM sys.check_constraints WHERE is_disabled = 0
                                                    UNION ALL SELECT OBJECT_NAME(parent_object_id), name FROM sys.foreign_keys WHERE is_disabled = 0 ORDER BY 1, 2", conn))
            await using (var r = await cmd.ExecuteReaderAsync())
                while (await r.ReadAsync())
                    sql.AppendLine($"ALTER TABLE dbo.[{r.GetString(0)}] WITH CHECK CHECK CONSTRAINT [{r.GetString(1)}];");
            await using (var cmd = new SqlCommand("SELECT OBJECT_NAME(parent_id), name FROM sys.triggers WHERE parent_class = 1 AND is_disabled = 0 ORDER BY 1, 2", conn))
            await using (var r = await cmd.ExecuteReaderAsync())
                while (await r.ReadAsync())
                    sql.AppendLine($"ALTER TABLE dbo.[{r.GetString(0)}] ENABLE TRIGGER [{r.GetString(1)}];");
            return sql.ToString();
        }

        // the data section: constraints and triggers off, every row, constraints checked and triggers on again
        private static string Data(List<TableData> tables, string enable)
        {
            var sql = new StringBuilder();
            sql.AppendLine("-------------------------------------------------------------------- demo data");
            sql.AppendLine(DisableSql);
            sql.AppendLine("GO");
            foreach (var t in tables.Where(t => t.Rows.Count > 0))
            {
                sql.AppendLine($"-- {t.Name}: {t.Rows.Count} rows");
                if (t.Identity)
                    sql.AppendLine($"SET IDENTITY_INSERT dbo.[{t.Name}] ON;");
                var columns = string.Join(", ", t.Columns.Select(c => $"[{c.Name}]"));
                for (var i = 0; i < t.Rows.Count; i += 200)
                {
                    sql.AppendLine($"INSERT INTO dbo.[{t.Name}] ({columns}) VALUES");
                    var chunk = t.Rows.Skip(i).Take(200).ToList();
                    for (var j = 0; j < chunk.Count; j++)
                        sql.Append("(").Append(string.Join(", ", chunk[j].Select((v, k) => Literal(v, t.Columns[k].Type)))).AppendLine(j == chunk.Count - 1 ? ");" : "),");
                }
                if (t.Identity)
                    sql.AppendLine($"SET IDENTITY_INSERT dbo.[{t.Name}] OFF;");
                sql.AppendLine("GO");
            }
            sql.Append(enable);
            sql.AppendLine("GO");
            return sql.ToString();
        }

        private static string Literal(object? value, string type)
        {
            if (value == null || value is DBNull)
                return "NULL";
            return type switch
            {
                "nvarchar" or "nchar" or "ntext" or "xml" => "N'" + ((string)value).Replace("'", "''") + "'",
                "varchar" or "char" or "text" => "'" + ((string)value).Replace("'", "''") + "'",
                "bit" => (bool)value ? "1" : "0",
                "datetime" or "smalldatetime" => "'" + ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss.fff", Inv) + "'",
                "datetime2" => "'" + ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss.fffffff", Inv) + "'",
                "date" => "'" + ((DateTime)value).ToString("yyyy-MM-dd", Inv) + "'",
                "time" => "'" + ((TimeSpan)value).ToString("c", Inv) + "'",
                "datetimeoffset" => "'" + ((DateTimeOffset)value).ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", Inv) + "'",
                "uniqueidentifier" => "'" + ((Guid)value).ToString() + "'",
                "varbinary" or "binary" or "image" or "timestamp" => "0x" + Convert.ToHexString((byte[])value),
                "float" => ((double)value).ToString("R", Inv),
                "real" => ((float)value).ToString("R", Inv),
                "decimal" or "numeric" or "money" or "smallmoney" => Convert.ToDecimal(value, Inv).ToString(Inv),
                _ => Convert.ToString(value, Inv)!
            };
        }

        private static async Task<List<TableData>> ReadAsync(string connectionString)
        {
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            var names = new List<string>();
            await using (var cmd = new SqlCommand("SELECT name FROM sys.tables WHERE is_ms_shipped = 0 ORDER BY name", conn))
            await using (var r = await cmd.ExecuteReaderAsync())
                while (await r.ReadAsync())
                    names.Add(r.GetString(0));
            var edges = new List<(string Parent, string Child)>();
            await using (var cmd = new SqlCommand("SELECT OBJECT_NAME(referenced_object_id), OBJECT_NAME(parent_object_id) FROM sys.foreign_keys WHERE referenced_object_id <> parent_object_id", conn))
            await using (var r = await cmd.ExecuteReaderAsync())
                while (await r.ReadAsync())
                    edges.Add((r.GetString(0), r.GetString(1)));

            // parents before children (Kahn); ties by name, so the script is stable
            var order = new List<string>();
            var remaining = new SortedSet<string>(names, StringComparer.Ordinal);
            while (remaining.Count > 0)
            {
                var next = remaining.FirstOrDefault(n => !edges.Any(e => e.Child == n && remaining.Contains(e.Parent) && e.Parent != n))
                           ?? throw new InvalidOperationException("foreign keys form a cycle");
                order.Add(next);
                remaining.Remove(next);
            }

            var tables = new List<TableData>();
            foreach (var name in order)
            {
                var columns = new List<Column>();
                string? identity = null;
                await using (var cmd = new SqlCommand(@"SELECT c.name, ty.name, c.is_identity FROM sys.columns c JOIN sys.types ty ON ty.user_type_id = c.user_type_id
                                                        WHERE c.object_id = OBJECT_ID(@t) AND c.is_computed = 0 AND ty.name <> 'timestamp' ORDER BY c.column_id", conn))
                {
                    cmd.Parameters.AddWithValue("@t", "dbo." + name);
                    await using var r = await cmd.ExecuteReaderAsync();
                    while (await r.ReadAsync())
                    {
                        columns.Add(new Column(r.GetString(0), r.GetString(1)));
                        if (r.GetBoolean(2))
                            identity = r.GetString(0);
                    }
                }
                var key = new List<string>();
                await using (var cmd = new SqlCommand(@"SELECT c.name FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
                                                        JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
                                                        WHERE i.object_id = OBJECT_ID(@t) AND i.is_primary_key = 1 ORDER BY ic.key_ordinal", conn))
                {
                    cmd.Parameters.AddWithValue("@t", "dbo." + name);
                    await using var r = await cmd.ExecuteReaderAsync();
                    while (await r.ReadAsync())
                        key.Add(r.GetString(0));
                }
                var rows = new List<object?[]>();
                var orderBy = key.Count > 0 ? string.Join(", ", key.Select(k => $"[{k}]")) : "(SELECT NULL)";
                await using (var cmd = new SqlCommand($"SELECT {string.Join(", ", columns.Select(c => $"[{c.Name}]"))} FROM dbo.[{name}] ORDER BY {orderBy}", conn) { CommandTimeout = 300 })
                await using (var r = await cmd.ExecuteReaderAsync())
                    while (await r.ReadAsync())
                    {
                        var row = new object?[columns.Count];
                        for (var i = 0; i < columns.Count; i++)
                            row[i] = r.IsDBNull(i) ? null : columns[i].Type == "date" ? r.GetDateTime(i) : r.GetValue(i);
                        rows.Add(row);
                    }
                tables.Add(new TableData(name, columns, identity != null, identity, rows));
            }
            return tables;
        }
    }
}
