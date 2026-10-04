using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TOP4HoneyChains.Web.Data;
using TOP4HoneyChains.Web.Models.Entities;
using TOP4HoneyChains.Web.Models.Identity;
using Xunit;

namespace TOP4HoneyChains.Web.Tests.E2E
{
    // The application hosted in-process with the organization model on, against its own LocalDB database
    // (D-46). The schema comes from the EF model; only reference/configuration tables (CCPs, parameters,
    // limits, statuses, honey types, the responsibility matrix…) are copied from the local development
    // database. No personal or business rows are copied. The database and the temporary content root
    // (uploaded files) are deleted afterwards.
    public sealed class E2EApp : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public const string Password = "E2e-Test-Pw-2026!";
        private const string Server = @"(localdb)\MSSQLLocalDB";
        public static readonly string Database = "TOP4HoneyChainsE2E";
        public static string SourceDatabase => Environment.GetEnvironmentVariable("E2E_SOURCE_DB") ?? "TOP4HoneyChainsDb";

        public string ConnectionString => $"Server={Server};Database={Database};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";
        private readonly string _contentRoot = Path.Combine(Path.GetTempPath(), "t4h-e2e-" + Guid.NewGuid().ToString("N"));

        // Reference tables in dependency order; all other tables stay empty
        private static readonly string[] ReferenceTables =
        {
            "AssignmentStatuses", "HoneyTypes", "RawHoneyProductionPeriods", "BeekeeperEducationLevels", "BeekeepingPurposeTypes", "BeekeepingTypes",
            "Capabilities", "OrgRoles", "CriticalControlPoints", "CriticalControlPointParameterCategories", "CriticalControlPointParameters",
            "CriticalControlPointParameterLimits", "CcpResponsibilities", "CcpResponsibilityCapabilities", "CcpResponsibilityRoles",
            "Provinces", "Districts", "RolePermissions"
        };

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            Directory.CreateDirectory(_contentRoot);
            builder.UseEnvironment("Development");
            builder.UseContentRoot(_contentRoot);
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString,
                ["Features:OrgRoleModel"] = "true",
                ["App:PublicBaseUrl"] = "http://localhost"
            }));
        }

        public async Task InitializeAsync()
        {
            await DropDatabaseAsync();
            using (var scope = Services.CreateScope())
            {
                var business = scope.ServiceProvider.GetRequiredService<TOP4HoneyChainsContext>();
                await business.Database.EnsureCreatedAsync();
                var identity = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await identity.GetService<IRelationalDatabaseCreator>().CreateTablesAsync();
            }
            await CopyReferenceDataAsync();
        }

        private async Task CopyReferenceDataAsync()
        {
            await using var conn = new SqlConnection(ConnectionString);
            await conn.OpenAsync();
            foreach (var table in ReferenceTables)
            {
                var columns = new List<string>();
                await using (var cmd = new SqlCommand("SELECT name FROM sys.columns WHERE object_id = OBJECT_ID(@t) AND is_computed = 0", conn))
                {
                    cmd.Parameters.AddWithValue("@t", "dbo." + table);
                    await using var r = await cmd.ExecuteReaderAsync();
                    while (await r.ReadAsync())
                        columns.Add(r.GetString(0));
                }
                var hasIdentity = await ScalarAsync(conn, $"SELECT COUNT(*) FROM sys.identity_columns WHERE object_id = OBJECT_ID('dbo.{table}')") > 0;
                var list = string.Join(", ", columns.Select(c => $"[{c}]"));
                var sql = (hasIdentity ? $"SET IDENTITY_INSERT dbo.[{table}] ON; " : "")
                          + $"INSERT INTO dbo.[{table}] ({list}) SELECT {list} FROM [{SourceDatabase}].dbo.[{table}];"
                          + (hasIdentity ? $" SET IDENTITY_INSERT dbo.[{table}] OFF;" : "");
                await using var copy = new SqlCommand(sql, conn);
                await copy.ExecuteNonQueryAsync();
            }
        }

        private static async Task<int> ScalarAsync(SqlConnection conn, string sql)
        {
            await using var cmd = new SqlCommand(sql, conn);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        private async Task DropDatabaseAsync()
        {
            await using var conn = new SqlConnection($"Server={Server};Database=master;Trusted_Connection=True;TrustServerCertificate=True");
            await conn.OpenAsync();
            await using var cmd = new SqlCommand($"IF DB_ID('{Database}') IS NOT NULL BEGIN ALTER DATABASE [{Database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{Database}]; END", conn);
            await cmd.ExecuteNonQueryAsync();
        }

        public new async Task DisposeAsync()
        {
            await base.DisposeAsync();
            SqlConnection.ClearAllPools();
            await DropDatabaseAsync();
            try { Directory.Delete(_contentRoot, true); } catch (IOException) { }
        }

        // ---------------------------------------------------------------- data helpers

        public async Task<T> WithContextAsync<T>(Func<TOP4HoneyChainsContext, Task<T>> work)
        {
            using var scope = Services.CreateScope();
            return await work(scope.ServiceProvider.GetRequiredService<TOP4HoneyChainsContext>());
        }

        public async Task<string> CreateUserAsync(string userName, string? role = null)
        {
            using var scope = Services.CreateScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var user = new ApplicationUser { UserName = userName, Email = userName + "@example.invalid", EmailConfirmed = true };
            var result = await users.CreateAsync(user, Password);
            Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(e => e.Description)));
            if (role != null)
            {
                if (!await roles.RoleExistsAsync(role))
                    await roles.CreateAsync(new IdentityRole(role));
                await users.AddToRoleAsync(user, role);
            }
            return user.Id;
        }

        public Task<int> CreateOrgAsync(string name, int? legacyWholesalerId, params (string Capability, string? Accreditation)[] capabilities) =>
            WithContextAsync(async db =>
            {
                var org = new Organization { DisplayName = name, Status = OrgStatus.Verified, LegacyWholesalerId = legacyWholesalerId, CreatedAt = DateTime.UtcNow };
                db.Organizations.Add(org);
                await db.SaveChangesAsync();
                foreach (var (cap, acc) in capabilities)
                    db.OrgCapabilities.Add(new OrgCapability
                    {
                        OrgId = org.OrgId, Capability = cap, IsAccredited = acc != null, AccreditationNumber = acc,
                        AccreditationValidUntil = acc != null ? DateOnly.FromDateTime(DateTime.Today.AddYears(1)) : null
                    });
                await db.SaveChangesAsync();
                return org.OrgId;
            });

        public Task AddMemberAsync(int orgId, string userId, string role) =>
            WithContextAsync(async db =>
            {
                db.Memberships.Add(new Membership { OrgId = orgId, UserId = userId, OrgRole = role, Status = MembershipStatus.Active, GrantedAt = DateTime.UtcNow });
                return await db.SaveChangesAsync();
            });
    }
}
