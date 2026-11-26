using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Services.OrgModel;
using TOP4HoneyChains.Web.Tests.Demo;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // D-98: a new installation. The database is built only from the repository: db/install (schema and reference data, twice to
    // prove it can run again), then every script of db/migrations, twice. Nothing is copied from the development database.
    public sealed class InstallApp : E2EApp
    {
        public static readonly string InstallDatabase = Database + "_Install";
        protected override string DatabaseName => InstallDatabase;

        public List<string> Log { get; } = new();

        public override async Task InitializeAsync()
        {
            await DropDatabaseAsync();
            await MasterAsync($"CREATE DATABASE [{InstallDatabase}]");
            foreach (var round in new[] { 1, 2 })
            {
                foreach (var file in new[] { "01_schema.sql", "02_reference_data.sql" })
                    await DemoReference.RunScriptAsync(ConnectionString, await File.ReadAllTextAsync(DemoReference.RepoFile("db", "install", file)));
                foreach (var file in Directory.GetFiles(DemoReference.RepoFile("db", "migrations"), "*.sql").OrderBy(f => Path.GetFileName(f), StringComparer.Ordinal))
                {
                    await DemoReference.RunScriptAsync(ConnectionString, await File.ReadAllTextAsync(file));
                    Log.Add($"{round} {Path.GetFileName(file)}");
                }
            }
        }
    }

    [CollectionDefinition("Install")]
    public class InstallCollection : ICollectionFixture<InstallApp> { }

    // Answers of the person at the console, in order
    public sealed class ScriptedSecrets : ISecretReader
    {
        private readonly Queue<string> _answers;
        public ScriptedSecrets(bool interactive, params string[] answers) { Interactive = interactive; _answers = new(answers); }
        public bool Interactive { get; }
        public int Asked { get; private set; }
        public string Read(string prompt) { Asked++; return _answers.Dequeue(); }
    }

    [Collection("Install")]
    [Trait("Category", "E2E")]
    public class InstallTests
    {
        private const string Admin = "zz_test_first_admin";
        private const string Phone = "+905550000199";
        private readonly InstallApp _app;
        public InstallTests(InstallApp app) => _app = app;

        private Task<int> CommandAsync(ScriptedSecrets secrets, StringWriter output, params string[] args) =>
            FirstAdminCommand.RunAsync(args, secrets, output,
                config => config.AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:DefaultConnection"] = _app.ConnectionString }));

        private async Task<int> CountAsync(string sql)
        {
            await using var conn = new SqlConnection(_app.ConnectionString);
            await conn.OpenAsync();
            return Convert.ToInt32(await new SqlCommand(sql, conn).ExecuteScalarAsync());
        }

        [Fact]
        public async Task A_new_installation_gets_its_first_administrator_once()
        {
            // the empty database after db/install and all migrations: reference data, the platform organization of 004, no people
            Assert.Equal(0, await CountAsync("SELECT COUNT(*) FROM dbo.AspNetUsers"));
            Assert.Equal(81, await CountAsync("SELECT COUNT(*) FROM dbo.Provinces"));
            Assert.True(await CountAsync("SELECT COUNT(*) FROM dbo.CriticalControlPointParameters") > 50);
            Assert.True(await CountAsync("SELECT COUNT(*) FROM dbo.ReferenceTranslations") > 1000);
            Assert.Equal(1, await CountAsync("SELECT COUNT(*) FROM dbo.Organizations WHERE IsPlatform = 1"));
            Assert.Equal(0, await CountAsync("SELECT COUNT(*) FROM dbo.Organizations WHERE IsPlatform = 0"));
            Assert.Equal(0, await CountAsync("SELECT COUNT(*) FROM dbo.RawHoneyProductionPeriods"));
            Assert.Equal(0, await CountAsync("SELECT COUNT(*) FROM dbo.Notices WHERE IsDraft = 0 OR OrgId IS NOT NULL"));
            Assert.Equal(0, await CountAsync("SELECT COUNT(*) FROM dbo.AspNetRoles WHERE NormalizedName NOT IN (N'ADMINISTRATOR', N'BEEKEEPER')"));
            Assert.Equal(0, await CountAsync("SELECT COUNT(*) FROM dbo.Memberships"));
            // a second run of the migrations does not bring back a constraint a later migration replaced (023 / 034)
            Assert.Equal(0, await CountAsync("SELECT COUNT(*) FROM sys.check_constraints WHERE name = N'CK_Samples_Kind'"));
            Assert.Equal(1, await CountAsync("SELECT COUNT(*) FROM sys.check_constraints WHERE name = N'CK_Samples_Kind2'"));
            // no constraint twice after the second run of db/install
            Assert.Equal(0, await CountAsync("SELECT COUNT(*) FROM (SELECT parent_object_id, definition FROM sys.check_constraints GROUP BY parent_object_id, definition HAVING COUNT(*) > 1) d"));
            Assert.Equal(0, await CountAsync("SELECT COUNT(*) FROM (SELECT parent_object_id, parent_column_id FROM sys.foreign_key_columns GROUP BY parent_object_id, parent_column_id, referenced_object_id HAVING COUNT(*) > 1) d"));

            // refusals: nothing is written for them
            var output = new StringWriter();
            Assert.Equal(5, await CommandAsync(new ScriptedSecrets(true, "Kovan-Bal-2026!", "Kovan-Bal-2027!"), output, "--user", Admin, "--phone", Phone));
            Assert.Contains("The two passwords are different", output.ToString());
            output = new StringWriter();
            Assert.Equal(6, await CommandAsync(new ScriptedSecrets(true, "short", "short"), output, "--user", Admin, "--phone", Phone));
            Assert.Contains("at least 10 characters long", output.ToString());
            output = new StringWriter();
            Assert.Equal(6, await CommandAsync(new ScriptedSecrets(true, "x" + Admin + "1", "x" + Admin + "1"), output, "--user", Admin, "--phone", Phone));
            Assert.Contains("must not contain the user name", output.ToString());
            output = new StringWriter();
            Assert.Equal(6, await CommandAsync(new ScriptedSecrets(true, "password123", "password123"), output, "--user", Admin, "--phone", Phone));
            Assert.Contains("too common", output.ToString());
            output = new StringWriter();
            Assert.Equal(6, await CommandAsync(new ScriptedSecrets(true, E2EApp.Password, E2EApp.Password), output, "--user", Admin, "--phone", "12"));
            Assert.Contains("valid phone number", output.ToString());
            Assert.Equal(0, await CountAsync("SELECT COUNT(*) FROM dbo.AspNetUsers"));
            Assert.Equal(0, await CountAsync("SELECT COUNT(*) FROM dbo.Memberships"));

            // the first administrator: Administrator role, the OrgAdmin membership of the platform organization, an audit entry
            output = new StringWriter();
            Assert.Equal(0, await CommandAsync(new ScriptedSecrets(true, E2EApp.Password, E2EApp.Password), output, "--user", Admin, "--phone", "0555 000 01 99", "--email", "zz.test.first.admin@example.invalid"));
            Assert.Contains($"The administrator {Admin} was created", output.ToString());
            var user = await CountAsync($"SELECT COUNT(*) FROM dbo.AspNetUsers u JOIN dbo.AspNetUserRoles ur ON ur.UserId = u.Id JOIN dbo.AspNetRoles r ON r.Id = ur.RoleId WHERE u.UserName = N'{Admin}' AND r.Name = N'Administrator' AND u.PhoneNumber = N'{Phone}'");
            Assert.Equal(1, user);
            var membership = await _app.WithContextAsync(db => db.Memberships.Include(m => m.Org).SingleAsync());
            Assert.True(membership.Org!.IsPlatform);
            Assert.Equal(OrgRoles.OrgAdmin, membership.OrgRole);
            Assert.Equal(MembershipStatus.Active, membership.Status);
            var audit = await _app.WithContextAsync(db => db.AuditEntries.SingleAsync(a => a.Action == FirstAdminService.AuditAction));
            Assert.Equal(membership.UserId, audit.UserId);
            Assert.DoesNotContain(E2EApp.Password, audit.NewValues);
            Assert.True((await _app.WithContextAsync(db => AuditChain.VerifyAsync(db))).Ok);

            // the administrator signs in and reaches the organizations
            var admin = await UserClient.LoginAsync(_app, Admin);
            Assert.Equal(200, await admin.StatusAsync("/Organizations"));

            // once an administrator exists, the command refuses before it asks for a password, and db/install refuses the database
            output = new StringWriter();
            var secrets = new ScriptedSecrets(true, E2EApp.Password, E2EApp.Password);
            Assert.Equal(4, await CommandAsync(secrets, output, "--user", "zz_test_second_admin", "--phone", Phone));
            Assert.Contains("An administrator already exists", output.ToString());
            Assert.Equal(0, secrets.Asked);
            var refused = await Assert.ThrowsAsync<SqlException>(() =>
                DemoReference.RunScriptAsync(_app.ConnectionString, File.ReadAllText(DemoReference.RepoFile("db", "install", "02_reference_data.sql"))));
            Assert.Contains("already has user accounts", refused.Message);
            Assert.Equal(1, await CountAsync("SELECT COUNT(*) FROM dbo.AspNetUsers"));

            // migration 001 (lock out non-administrators) still refuses nothing and locks nobody here: the only account is the administrator
            await DemoReference.RunScriptAsync(_app.ConnectionString, File.ReadAllText(Directory.GetFiles(DemoReference.RepoFile("db", "migrations"), "001_*.sql").Single()));
            Assert.Equal(0, await CountAsync("SELECT COUNT(*) FROM dbo.AspNetUsers WHERE LockoutEnd IS NOT NULL"));
        }

        [Fact]
        public async Task The_command_never_takes_the_password_from_arguments_files_or_pipes()
        {
            var output = new StringWriter();
            var secrets = new ScriptedSecrets(true);
            Assert.Equal(2, await CommandAsync(secrets, output, "--user", "zz_test_x", "--phone", Phone, "--password", "Kovan-Bal-2026!"));
            Assert.Contains("never given on the command line", output.ToString());

            output = new StringWriter();
            Assert.Equal(2, await CommandAsync(new ScriptedSecrets(false, "Kovan-Bal-2026!", "Kovan-Bal-2026!"), output, "--user", "zz_test_x", "--phone", Phone));
            Assert.Contains("interactive window", output.ToString());

            output = new StringWriter();
            Assert.Equal(2, await CommandAsync(secrets, output, "--phone", Phone));
            Assert.Contains("Usage:", output.ToString());
            Assert.Equal(0, secrets.Asked);
        }

        [Fact]
        public void Every_migration_ran_twice_after_db_install()
        {
            var migrations = Directory.GetFiles(DemoReference.RepoFile("db", "migrations"), "*.sql").Length;
            Assert.Equal(2 * migrations, _app.Log.Count);
            Assert.Contains(_app.Log, l => l.StartsWith("2 001_", StringComparison.Ordinal));
        }
    }
}
