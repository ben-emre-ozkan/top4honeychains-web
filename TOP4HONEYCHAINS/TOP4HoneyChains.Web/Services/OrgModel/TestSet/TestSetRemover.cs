using System.Data;
using System.Data.Common;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TOP4HoneyChains.Web.Data;

namespace TOP4HoneyChains.Web.Services.OrgModel.TestSet
{
    public sealed record RemovalCount(string Table, int Rows);
    // a real record that refers to a record of the test set: the removal stops and lists it
    public sealed record RemovalBlock(string Table, string Key, string Column, string RefersTo, string RefersToKey);

    public sealed record RemovalReport(IReadOnlyList<RemovalCount> Counts, IReadOnlyList<RemovalBlock> Blocks, int AuditEntriesKept, bool Removed, int FilesDeleted)
    {
        public int Total => Counts.Sum(c => c.Rows);
    }

    // D-112: remove-test-set. Decides which rows are test data from the flags (Organizations.IsTestData,
    // RawHoneyProductionPeriods.IsTestData, TestSetAccounts) and what the test set's accounts and organizations created, collects them
    // table by table (#TestSet), then checks every foreign key of the database: a row outside the test set that refers to a row of
    // it stops the removal and is listed (nothing is deleted). Without --commit it only counts. With --commit it deletes the rows in
    // one transaction, children first, through the passage of migration 037 (session context top4.test_set_removal); the audit
    // entries stay (AuditEntries is append-only), and one more entry records the removal. The stored files of the removed rows
    // (documents, reports, forms) are deleted from App_Data afterwards.
    public sealed class TestSetRemover
    {
        public const string AuditAction = "RemoveTestSet";

        private readonly TOP4HoneyChainsContext _db;
        private readonly string _contentRoot;

        public TestSetRemover(TOP4HoneyChainsContext db, string contentRoot)
        {
            _db = db;
            _contentRoot = contentRoot;
        }

        private static string I(string table) => $"(SELECT KI FROM #TestSet WHERE Tbl = '{table}')";
        private static string S(string table) => $"(SELECT KS FROM #TestSet WHERE Tbl = '{table}')";

        // the rows of the test set, in the order they are found (each rule may use the tables before it). x is the row.
        private static readonly (string Table, string Rule)[] Rules =
        {
            ("Organizations", "x.IsTestData = 1 OR x.CreatedByUserId IN (SELECT UserId FROM dbo.TestSetAccounts)"),
            ("Beekeepers", $"EXISTS (SELECT 1 FROM dbo.BeekeeperRegistrations r WHERE r.BeekeeperId = x.BeekeeperId AND r.RegisteringOrgId IN {I("Organizations")}) " +
                           $"AND NOT EXISTS (SELECT 1 FROM dbo.BeekeeperRegistrations r WHERE r.BeekeeperId = x.BeekeeperId AND r.RegisteringOrgId NOT IN {I("Organizations")})"),
            ("AspNetUsers", "x.Id IN (SELECT UserId FROM dbo.TestSetAccounts) " +
                            $"OR x.Id IN (SELECT UserId FROM dbo.BeekeeperAccounts WHERE BeekeeperId IN {S("Beekeepers")}) " +
                            $"OR (EXISTS (SELECT 1 FROM dbo.Memberships m WHERE m.UserId = x.Id) AND NOT EXISTS (SELECT 1 FROM dbo.Memberships m WHERE m.UserId = x.Id AND m.OrgId NOT IN {I("Organizations")}) " +
                            "    AND NOT EXISTS (SELECT 1 FROM dbo.AspNetUserRoles ur JOIN dbo.AspNetRoles ro ON ro.Id = ur.RoleId WHERE ur.UserId = x.Id AND ro.NormalizedName = N'ADMINISTRATOR'))"),
            ("Wholesalers", $"x.WholesalerId IN (SELECT LegacyWholesalerId FROM dbo.Organizations WHERE OrgId IN {I("Organizations")} AND LegacyWholesalerId IS NOT NULL)"),
            ("RawHoneyProductionPeriods", "x.IsTestData = 1"),
            ("HoneyTypes", $"x.CreatedByOrgId IN {I("Organizations")}"),
            ("Apiaries", $"x.ApiaryBeekeeperId IN {S("Beekeepers")}"),
            ("RawHoneyProductions", $"x.OwnerOrgId IN {I("Organizations")} OR x.RawHoneyProductionWholesalerId IN {I("Wholesalers")} " +
                                    $"OR x.RawHoneyProductionId IN (SELECT RawHoneyProductionId FROM dbo.LotIntakes WHERE ReceivingOrgId IN {I("Organizations")})"),
            ("LotIntakes", $"x.RawHoneyProductionId IN {I("RawHoneyProductions")} OR x.ReceivingOrgId IN {I("Organizations")}"),
            ("FinalHoneyProductions", $"x.OwnerOrgId IN {I("Organizations")} OR x.FinalHoneyProductionWholesalerId IN {I("Wholesalers")}"),
            ("OrgContainers", $"x.OrgId IN {I("Organizations")}"),
            ("Containers", $"x.IntakeId IN {I("LotIntakes")} OR x.RawHoneyProductionId IN {I("RawHoneyProductions")} OR x.OrgContainerId IN {I("OrgContainers")}"),
            ("Samples", $"x.RawHoneyProductionId IN {I("RawHoneyProductions")} OR x.FinalHoneyProductionId IN {I("FinalHoneyProductions")} " +
                        $"OR x.ContainerId IN {I("Containers")} OR x.TakingOrgId IN {I("Organizations")}"),
            // a composite sample of test jars (twice: a composite of composites)
            ("Samples", $"x.SampleId IN (SELECT CompositeSampleId FROM dbo.SampleSources WHERE SourceSampleId IN {I("Samples")})"),
            ("Samples", $"x.SampleId IN (SELECT CompositeSampleId FROM dbo.SampleSources WHERE SourceSampleId IN {I("Samples")})"),
            ("LabReports", $"x.SampleId IN {I("Samples")} OR x.LabOrgId IN {I("Organizations")}"),
            ("SealBatches", $"x.OrgId IN {I("Organizations")}"),
            ("SealCodes", $"x.OrgId IN {I("Organizations")} OR x.BatchId IN {I("SealBatches")}"),
            ("Notices", $"x.OrgId IN {I("Organizations")}"),
            ("SmsMessages", $"x.Provider = N'TestInbox' OR x.OrgId IN {I("Organizations")} OR x.SentByUserId IN {S("AspNetUsers")}"),
            ("OneTimeCodes", $"x.SmsId IN {I("SmsMessages")}"),
            ("HolderStorageRecords", $"x.HolderOrgId IN {I("Organizations")}"),
            ("HolderStorageValues", $"x.RecordId IN {I("HolderStorageRecords")}"),
            ("Shipments", $"x.FromOrgId IN {I("Organizations")}"),
            ("ConsumerAccounts", $"x.UserId IN {S("AspNetUsers")}"),
            ("ConsumerFeedback", $"x.ConsumerUserId IN {S("AspNetUsers")}"),
            ("ApiaryPhotos", $"x.ApiaryPhotoApiaryId IN {I("Apiaries")}"),
            ("BeekeeperAccounts", $"x.BeekeeperId IN {S("Beekeepers")} OR x.UserId IN {S("AspNetUsers")}"),
            ("BeekeeperAssociations", $"x.BeekeeperId IN {S("Beekeepers")} OR x.AssociationOrgId IN {I("Organizations")}"),
            ("BeekeeperRegistrations", $"x.BeekeeperId IN {S("Beekeepers")} OR x.RegisteringOrgId IN {I("Organizations")}"),
            ("BeekeeperRegistry", $"x.BeekeeperId IN {S("Beekeepers")}"),
            ("BeekeeperStorefronts", $"x.BeekeeperId IN {S("Beekeepers")}"),
            ("RegistryVerifications", $"x.BeekeeperId IN {S("Beekeepers")}"),
            ("Consents", $"x.BeekeeperId IN {S("Beekeepers")} OR x.RegisteringOrgId IN {I("Organizations")} OR x.AssociationOrgId IN {I("Organizations")}"),
            ("DataSubjectRequests", $"x.BeekeeperId IN {S("Beekeepers")} OR x.OrgId IN {I("Organizations")}"),
            ("NoticeAcceptances", $"x.NoticeId IN {I("Notices")} OR x.BeekeeperId IN {S("Beekeepers")} OR x.UserId IN {S("AspNetUsers")} OR x.CodeId IN {I("OneTimeCodes")}"),
            ("ComplianceReviews", $"x.OrgId IN {I("Organizations")} OR x.ReviewedByUserId IN {S("AspNetUsers")}"),
            ("ContainerEmptyings", $"x.ContainerId IN {I("Containers")} OR x.FinalHoneyProductionId IN {I("FinalHoneyProductions")}"),
            ("ContainerSealEvents", $"x.ContainerId IN {I("Containers")}"),
            ("CriticalControlPointMonitoringLogs", $"x.RawHoneyProductionId IN {I("RawHoneyProductions")} OR x.FinalHoneyProductionId IN {I("FinalHoneyProductions")} " +
                                                   $"OR x.RecordedByOrgId IN {I("Organizations")} OR x.LabReportId IN {I("LabReports")}"),
            ("CcpCorrections", $"x.RequestedByUserId IN {S("AspNetUsers")}"),
            ("DeliveryCorrections", $"x.IntakeId IN {I("LotIntakes")} OR x.RawHoneyProductionId IN {I("RawHoneyProductions")}"),
            ("RawHoneyCCPAssignments", $"x.RawHoneyProductionId IN {I("RawHoneyProductions")}"),
            ("FinalHoneyCCPAssignments", $"x.FinalHoneyProductionId IN {I("FinalHoneyProductions")}"),
            ("FinalHoneyProductionDetails", $"x.FinalHoneyProductionId IN {I("FinalHoneyProductions")}"),
            ("LabResults", $"x.ReportId IN {I("LabReports")}"),
            ("LotPrices", $"x.RawHoneyProductionId IN {I("RawHoneyProductions")}"),
            ("Memberships", $"x.OrgId IN {I("Organizations")} OR x.UserId IN {S("AspNetUsers")}"),
            ("OrgDocuments", $"x.OrgId IN {I("Organizations")}"),
            ("OrgVerificationEvents", $"x.OrgId IN {I("Organizations")}"),
            ("SampleCustodyEvents", $"x.SampleId IN {I("Samples")} OR x.FromOrgId IN {I("Organizations")}"),
            ("UserCredentialIssues", $"x.UserId IN {S("AspNetUsers")} OR x.SmsId IN {I("SmsMessages")}"),
            ("UserSettings", $"x.UserId IN {S("AspNetUsers")}"),
            ("HoneyTypeMerges", $"x.MergedByUserId IN {S("AspNetUsers")} OR x.SourceHoneyTypeId IN {I("HoneyTypes")}"),
            ("TestSetAccounts", "1 = 1"),
            // tables with a composite key (nobody refers to them)
            ("BatchOriginShares", $"x.FinalHoneyProductionId IN {I("FinalHoneyProductions")}"),
            ("BeekeeperHoneyTypes", $"x.BeekeeperId IN {S("Beekeepers")}"),
            ("ConsumerSavedBatches", $"x.ConsumerUserId IN {S("AspNetUsers")}"),
            ("OrgCapabilities", $"x.OrgId IN {I("Organizations")}"),
            ("SampleSources", $"x.CompositeSampleId IN {I("Samples")}")
        };

        // the stored files of the rows that are removed: table, file column, App_Data folder
        private static readonly (string Table, string Key, string Column, string Folder)[] Files =
        {
            ("OrgDocuments", "DocumentId", "StoredFileName", FileStore.OrgDocuments),
            ("LabReports", "ReportId", "StoredFileName", LabService.Reports),
            ("LotIntakes", "IntakeId", "FormStoredFileName", Controllers.IntakeController.IntakeForms),
            ("BeekeeperRegistrations", "RegistrationId", "EvidenceStoredFileName", RegistrationService.EvidenceFolder),
            ("RegistryVerifications", "Id", "StoredFileName", FileStore.RegistryProofs),
            ("CriticalControlPointMonitoringLogs", "LogId", "StoredFileName", "App_Data/ccp-files"),
            ("Consents", "ConsentId", "DocumentStoredFileName", StorefrontService.ConsentDocuments)
        };

        private sealed record Key(string Column, bool IsText);

        public async Task<RemovalReport> RunAsync(bool commit, CancellationToken ct = default)
        {
            var conn = _db.Database.GetDbConnection();
            await _db.Database.OpenConnectionAsync(ct);
            try
            {
                await using var tx = await conn.BeginTransactionAsync(IsolationLevel.Serializable, ct);
                await ExecAsync(conn, tx, "IF OBJECT_ID('tempdb..#TestSet') IS NOT NULL DROP TABLE #TestSet; " +
                                          "CREATE TABLE #TestSet (Tbl sysname NOT NULL, KI bigint NULL, KS nvarchar(450) NULL); " +
                                          "CREATE INDEX IX_TestSet ON #TestSet (Tbl, KI, KS);", ct);

                // the single-column primary keys of the tables
                var keys = new Dictionary<string, Key>(StringComparer.OrdinalIgnoreCase);
                foreach (var row in await QueryAsync(conn, tx, @"
SELECT t.name, MAX(c.name), MAX(ty.name)
FROM sys.tables t JOIN sys.indexes i ON i.object_id = t.object_id AND i.is_primary_key = 1
JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = ic.column_id
JOIN sys.types ty ON ty.user_type_id = c.user_type_id
WHERE t.schema_id = SCHEMA_ID('dbo') GROUP BY t.name HAVING COUNT(*) = 1", ct))
                    keys[(string)row[0]] = new Key((string)row[1], (string)row[2] is "nvarchar" or "varchar" or "nchar" or "char");

                // collect
                var composite = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var (table, rule) in Rules)
                {
                    if (keys.TryGetValue(table, out var key))
                        await ExecAsync(conn, tx, $"INSERT INTO #TestSet (Tbl, {(key.IsText ? "KS" : "KI")}) SELECT DISTINCT '{table}', x.[{key.Column}] FROM dbo.[{table}] x " +
                                                  $"WHERE ({rule}) AND NOT EXISTS (SELECT 1 FROM #TestSet t WHERE t.Tbl = '{table}' AND t.{(key.IsText ? "KS" : "KI")} = x.[{key.Column}])", ct);
                    else
                        composite[table] = composite.TryGetValue(table, out var r) ? $"({r}) OR ({rule})" : rule;
                }

                // the counts
                var counts = new List<RemovalCount>();
                foreach (var row in await QueryAsync(conn, tx, "SELECT Tbl, COUNT(*) FROM #TestSet GROUP BY Tbl", ct))
                    counts.Add(new RemovalCount((string)row[0], Convert.ToInt32(row[1])));
                foreach (var (table, rule) in composite)
                    counts.Add(new RemovalCount(table, Convert.ToInt32(await ScalarAsync(conn, tx, $"SELECT COUNT(*) FROM dbo.[{table}] x WHERE {rule}", ct))));
                counts = counts.Where(c => c.Rows > 0).OrderBy(c => c.Table, StringComparer.Ordinal).ToList();
                var audit = Convert.ToInt32(await ScalarAsync(conn, tx,
                    $"SELECT COUNT(*) FROM dbo.AuditEntries WHERE UserId IN {S("AspNetUsers")} OR OrgId IN {I("Organizations")}", ct));

                // every foreign key of the database into a table of the test set: a referring row outside the test set stops the removal
                var fks = await QueryAsync(conn, tx, @"
SELECT OBJECT_NAME(fk.parent_object_id), COL_NAME(fkc.parent_object_id, fkc.parent_column_id), OBJECT_NAME(fk.referenced_object_id), COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id)
FROM sys.foreign_keys fk JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
WHERE fk.delete_referential_action = 0", ct);
                var inSet = counts.Select(c => c.Table).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var blocks = new List<RemovalBlock>();
                foreach (var fk in fks)
                {
                    string child = (string)fk[0], column = (string)fk[1], parent = (string)fk[2];
                    if (!inSet.Contains(parent) || !keys.TryGetValue(parent, out var pk))
                        continue;
                    var parentKeys = pk.IsText ? S(parent) : I(parent);
                    var outside = keys.TryGetValue(child, out var ck)
                        ? $"NOT EXISTS (SELECT 1 FROM #TestSet t WHERE t.Tbl = '{child}' AND t.{(ck.IsText ? "KS" : "KI")} = x.[{ck.Column}])"
                        : composite.TryGetValue(child, out var rule) ? $"CASE WHEN {rule} THEN 1 ELSE 0 END = 0" : "1 = 1";
                    var keyColumn = ck?.Column ?? column;
                    foreach (var row in await QueryAsync(conn, tx,
                                 $"SELECT TOP (50) CAST(x.[{keyColumn}] AS nvarchar(450)), CAST(x.[{column}] AS nvarchar(450)) FROM dbo.[{child}] x WHERE x.[{column}] IN {parentKeys} AND {outside}", ct))
                        blocks.Add(new RemovalBlock(child, (string)row[0], column, parent, (string)row[1]));
                }

                if (!commit || blocks.Count > 0 || counts.Count == 0)
                {
                    await tx.RollbackAsync(ct);
                    return new RemovalReport(counts, blocks, audit, false, 0);
                }

                // the stored files of the removed rows (deleted after the commit)
                var files = new List<string>();
                foreach (var (table, keyColumn, fileColumn, folder) in Files)
                    if (inSet.Contains(table))
                        foreach (var row in await QueryAsync(conn, tx, $"SELECT x.[{fileColumn}] FROM dbo.[{table}] x WHERE x.[{keyColumn}] IN {I(table)} AND x.[{fileColumn}] IS NOT NULL", ct))
                            files.Add(Path.Combine(_contentRoot, folder, Path.GetFileName((string)row[0])));

                // delete, children first (the foreign keys between the tables of the test set)
                await ExecAsync(conn, tx, "EXEC sys.sp_set_session_context @key = N'top4.test_set_removal', @value = N'1';", ct);
                var edges = fks.Select(f => ((string)f[0], (string)f[2])).Where(e => inSet.Contains(e.Item1) && inSet.Contains(e.Item2) && e.Item1 != e.Item2).ToList();
                foreach (var table in ChildrenFirst(inSet, edges))
                {
                    var where = keys.TryGetValue(table, out var key)
                        ? $"x.[{key.Column}] IN {(key.IsText ? S(table) : I(table))}"
                        : composite[table];
                    await ExecAsync(conn, tx, $"DELETE x FROM dbo.[{table}] x WHERE {where};", ct);
                }
                await ExecAsync(conn, tx, "EXEC sys.sp_set_session_context @key = N'top4.test_set_removal', @value = NULL;", ct);
                await tx.CommitAsync(ct);

                var deleted = 0;
                foreach (var file in files.Distinct())
                    try
                    {
                        if (File.Exists(file))
                        {
                            File.Delete(file);
                            deleted++;
                        }
                    }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }

                await AuditChain.AppendAsync(_db, new[]
                {
                    new AuditRecord(DateTime.UtcNow, null, "remove-test-set (D-112)", null, AuditAction, "TestSet", null, null,
                        JsonSerializer.Serialize(new { Rows = counts.ToDictionary(c => c.Table, c => c.Rows), AuditEntriesKept = audit, FilesDeleted = deleted }), "TestSet", null)
                }, ct);
                return new RemovalReport(counts, blocks, audit, true, deleted);
            }
            finally
            {
                await _db.Database.CloseConnectionAsync();
            }
        }

        // a deletion order in which a table comes before every table it refers to
        public static List<string> ChildrenFirst(IEnumerable<string> tables, IReadOnlyCollection<(string Child, string Parent)> edges)
        {
            var left = tables.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var order = new List<string>();
            while (left.Count > 0)
            {
                // a table no remaining table refers to
                var next = left.Where(t => !edges.Any(e => left.Contains(e.Child) && string.Equals(e.Parent, t, StringComparison.OrdinalIgnoreCase)))
                    .OrderBy(t => t, StringComparer.Ordinal).FirstOrDefault()
                    ?? throw new InvalidOperationException("The tables of the test set refer to each other in a cycle: " + string.Join(", ", left));
                order.Add(next);
                left.Remove(next);
            }
            return order;
        }

        private static async Task ExecAsync(DbConnection conn, DbTransaction tx, string sql, CancellationToken ct)
        {
            await using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;
            cmd.CommandTimeout = 300;
            await cmd.ExecuteNonQueryAsync(ct);
        }

        private static async Task<object?> ScalarAsync(DbConnection conn, DbTransaction tx, string sql, CancellationToken ct)
        {
            await using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;
            cmd.CommandTimeout = 300;
            return await cmd.ExecuteScalarAsync(ct);
        }

        private static async Task<List<object[]>> QueryAsync(DbConnection conn, DbTransaction tx, string sql, CancellationToken ct)
        {
            await using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;
            cmd.CommandTimeout = 300;
            var rows = new List<object[]>();
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                var values = new object[r.FieldCount];
                r.GetValues(values);
                rows.Add(values);
            }
            return rows;
        }
    }
}
